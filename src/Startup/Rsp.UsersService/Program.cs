using System.Net.Mime;
using System.Text.Json;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Rsp.Logging.Middlewares.CorrelationId;
using Rsp.Logging.Middlewares.RequestTracing;
using Rsp.ServiceDefaults;
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
    .AddJsonFile("logsettings.json");

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
            .Select(KeyFilter.Any, "users")
            .ConfigureRefresh(refreshOptions =>
                refreshOptions
                .Register("AppSettings:Sentinel:UsersService", refreshAll: true)
                .SetCacheExpiration(new TimeSpan(0, 0, 15))
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

services
    .AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute(MediaTypeNames.Application.Json));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status200OK));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
        options.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status503ServiceUnavailable));
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

var app = builder.Build();

app.MapHealthChecks("/probes/startup");
app.MapHealthChecks("/probes/readiness");
app.MapHealthChecks("/probes/liveness");

// if you using .NET Aspire and have added the ServiceDefaults project
// uncomment the following line
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
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

app.MapCustomizedIdentityApi<IrasUser, IdentityRole>();

// run the database migration and seed the data
await app.MigrateAndSeedDatabaseAsync();

await app.RunAsync();