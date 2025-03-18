using System.Net.Mime;
using System.Text.Json;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;
using Rsp.Logging.ActionFilters;
using Rsp.Logging.Extensions;
using Rsp.Logging.Interceptors;
using Rsp.Logging.Middlewares.CorrelationId;
using Rsp.Logging.Middlewares.RequestTracing;
using Rsp.ServiceDefaults;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Application.Settings;
using Rsp.UsersService.Configuration.Auth;
using Rsp.UsersService.Configuration.Database;
using Rsp.UsersService.Configuration.Dependencies;
using Rsp.UsersService.Configuration.Swagger;
using Rsp.UsersService.Domain.Entities;
using Rsp.UsersService.Extensions;
using Rsp.UsersService.Infrastructure;
using Rsp.UsersService.Infrastructure.Repositories;
using Rsp.UsersService.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

//Add logger
builder
    .Configuration
    .AddJsonFile("logsettings.json")
    .AddJsonFile("featuresettings.json", true, true)
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

var services = builder.Services;
var configuration = builder.Configuration;

if (!builder.Environment.IsDevelopment())
{
    var azureAppConfigSection = configuration.GetSection(nameof(AppSettings));
    var azureAppConfiguration = azureAppConfigSection.Get<AppSettings>();

    // Load configuration from Azure App Configuration
    builder.Configuration.AddAzureAppConfiguration(
        options =>
        {
            options.Connect
            (
                new Uri(azureAppConfiguration!.AzureAppConfiguration.Endpoint),
                new ManagedIdentityCredential(azureAppConfiguration.AzureAppConfiguration.IdentityClientID)
            )
            .Select(KeyFilter.Any)
            .Select(KeyFilter.Any, AppSettings.ServiceLabel)
            .ConfigureRefresh(refreshOptions =>
                refreshOptions
                .Register("AppSettings:Sentinel", AppSettings.ServiceLabel, refreshAll: true)
                .SetRefreshInterval(new TimeSpan(0, 0, 15))
            );
        }
    );

    services.AddAzureAppConfiguration();
}

var appSettingsSection = configuration.GetSection(nameof(AppSettings));
var appSettings = appSettingsSection.Get<AppSettings>();

// adds sql server database context
services.AddDatabase(configuration);

services
    .AddIdentityApiEndpoints<IrasUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IrasIdentityDbContext>()
    .AddUserStore<UserStore<IrasUser, IdentityRole, IrasIdentityDbContext>>();

// Add services to the container.
services.AddServices();

services.AddHttpContextAccessor();

// routing configuration
services.AddRouting(options => options.LowercaseUrls = true);

// configures the authentication and authorization
services.AddAuthenticationAndAuthorization(appSettings!);

// Creating a feature manager without the use of DI. Injecting IFeatureManager
// via DI is appropriate in consturctor methods. At the startup, it's
// not recommended to call services.BuildServiceProvider and retreive IFeatureManager
// via provider. Instead, the follwing approach is recommended by creating FeatureManager
// with ConfigurationFeatureDefinitionProvider using the existing configuration.
var featureManager = new FeatureManager(new ConfigurationFeatureDefinitionProvider(configuration));

services
    .AddControllers(async options =>
    {
        options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status200OK));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status503ServiceUnavailable));

        if (await featureManager.IsEnabledAsync(Features.InterceptedLogging))
        {
            options.Filters.Add<LogActionFilter>();
        }
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    });

// add default health checks
services.Configure<HealthCheckPublisherOptions>(options => options.Period = TimeSpan.FromSeconds(300));

services.AddHealthChecks();

// adds the Swagger for the Api Documentation
services.AddSwagger();

if (await featureManager.IsEnabledAsync(Features.InterceptedLogging))
{
    services.AddLoggingInterceptor<LoggingInterceptor>();
}

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.MapHealthChecks("/probes/startup");
app.MapHealthChecks("/probes/readiness");
app.MapHealthChecks("/probes/liveness");

// if you using .NET Aspire and have added the ServiceDefaults project
// uncomment the following line
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseAzureAppConfiguration();
}

app.UseHttpsRedirection();

// If you have a specific package or project
// referenced that allows injecting the correlationId in the request
// pipeline, uncomment the following and rename the method if necessary.
app.UseCorrelationId();

app.UseRouting();

app.UseAuthentication();

// If you have a specific package or project
// referenced that allows the request tracing like Serilog request tracing
// uncomment the following and rename the method if necessary.
// NOTE: Needs to be after UseAuthentication in the pipeline so it can extract the claims values
// if needed
app.UseRequestTracing();

app.UseAuthorization();

app.MapControllers();

await app.MapCustomizedIdentityApiAsync<IrasUser, IdentityRole>(featureManager);

// run the database migration and seed the data
await app.MigrateAndSeedDatabaseAsync();

logger.LogAsInformation("Starting Up");

await app.RunAsync();