// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement;
using Rsp.Logging.ActionFilters;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Domain.Entities;
using static Rsp.UsersService.WebApi.Endpoints.Roles.AddRoleClaimEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Roles.CreateRoleEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Roles.DeleteRoleEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Roles.GetAllRolesEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Roles.GetRoleClaimsEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Roles.RemoveRoleClaimEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Roles.UpdateRoleEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.AddUserClaimsEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.AddUserToRolesEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.DeleteUserEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetAllUsersEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUserAuditTrailEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUserClaimsEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUserEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUsersEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUsersInRoleEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.RegisterUserEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.RemoveUserClaimsEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.RemoveUserFromRolesEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.SearchUsersEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.UpdateUserEndpoint;

namespace Rsp.UsersService.WebApi.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add identity endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public static class IdentityApiEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Add CRUD endpoints for users, roles and cliams, using ASP.NET Core Identity APIs.
    /// </summary>
    /// <typeparam name="TUser">The type describing the user. This should match the generic parameter in <see cref="UserManager{TUser}"/>.</typeparam>
    /// <typeparam name="TRole">The type describing the role. This should match the generic parameter in <see cref="RoleManager{TRole}"/>.</typeparam>
    /// <param name="endpoints">
    /// The <see cref="IEndpointRouteBuilder"/> to add the identity endpoints to.
    /// Call <see cref="EndpointRouteBuilderExtensions.MapGroup(IEndpointRouteBuilder, string)"/> to add a prefix to all the endpoints.
    /// </param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> to further customize the added endpoints.</returns>
    public static async Task<IEndpointConventionBuilder> MapCustomizedIdentityApiAsync<TUser, TRole>(this IEndpointRouteBuilder endpoints, FeatureManager featureManager)
        where TUser : IrasUser, new()
        where TRole : IdentityRole, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var routeGroup =
            endpoints
                .MapGroup("")
                .RequireAuthorization();

        if (await featureManager.IsEnabledAsync(Features.InterceptedLogging))
        {
            // adding endpoint filter to all endpoints
            routeGroup.AddEndpointFilter<LogActionFilter>();
        }

        var usersGroup = routeGroup
            .MapGroup(RoutePatterns.Users)
            .WithTags(nameof(RoutePatterns.Users));

        // mapping endpoints with name and type metadata
        // this metadata will be used in LogActionFilter to create logger
        // and get the endpoint name.
        usersGroup
            .MapPost(RoutePatterns.All, GetAllUsers<TUser>)
            .WithDescription("Gets all users")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get users";
                return operation;
            })
            .WithName(nameof(GetAllUsers))
            .WithMetadata(typeof(Endpoints.Users.GetAllUsersEndpoint));

        usersGroup
            .MapGet(RoutePatterns.Root, GetUserByIdOrEmail<TUser>)
            .WithDescription("Get user by id or email")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get user";
                return operation;
            })
            .WithName(nameof(GetUserByIdOrEmail))
            .WithMetadata(typeof(Endpoints.Users.GetUserEndpoint));

        usersGroup
            .MapPost(RoutePatterns.ByIds, GetUsersById<TUser>)
            .WithDescription("Get users by ids")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get users by ids";
                return operation;
            })
            .WithName(nameof(GetUsersById))
            .WithMetadata(typeof(Endpoints.Users.GetUsersEndpoint));

        usersGroup
            .MapPost(RoutePatterns.Search, SearchUsers<TUser>)
            .WithDescription("Search users by their name or email")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Search users by their name or email";
                return operation;
            })
            .WithName(nameof(SearchUsers))
            .WithMetadata(typeof(Endpoints.Users.GetUsersEndpoint));

        usersGroup
            .MapGet(RoutePatterns.Role, GetUsersInRole<TUser>)
            .WithDescription("Get all users in a role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get users in a role";
                return operation;
            })
            .WithName(nameof(GetUsersInRole))
            .WithMetadata(typeof(Endpoints.Users.GetUsersInRoleEndpoint));

        usersGroup
            .MapPost(RoutePatterns.Root, RegisterUser<TUser>)
            .WithDescription("Creates a user in the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user";
                return operation;
            })
            .WithName(nameof(RegisterUser))
            .WithMetadata(typeof(Endpoints.Users.RegisterUserEndpoint));

        usersGroup
            .MapPut(RoutePatterns.Root, UpdateUser<TUser>)
            .WithDescription("Updates a user in the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Updates a user";
                return operation;
            })
            .WithName(nameof(UpdateUser))
            .WithMetadata(typeof(Endpoints.Users.UpdateUserEndpoint));

        usersGroup
            .MapDelete(RoutePatterns.Root, DeleteUser<TUser>)
            .WithDescription("Deletes a user from the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Deletes a user";
                return operation;
            })
            .WithName(nameof(DeleteUser))
            .WithMetadata(typeof(Endpoints.Users.DeleteUserEndpoint));

        usersGroup
            .MapPost(RoutePatterns.Roles, AddUserToRoles<TUser>)
            .WithDescription("Adds a user to a role or multiple roles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Adds a user to role(s)";
                return operation;
            })
            .WithName(nameof(AddUserToRoles))
            .WithMetadata(typeof(Endpoints.Users.AddUserToRolesEndpoint));

        usersGroup
            .MapDelete(RoutePatterns.Roles, RemoveUserFromRoles<TUser>)
            .WithDescription("Removes a user from a role or multiple roles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Removes a user from role(s)";
                return operation;
            })
            .WithName(nameof(RemoveUserFromRoles))
            .WithMetadata(typeof(Endpoints.Users.RemoveUserFromRolesEndpoint));

        usersGroup
            .MapGet(RoutePatterns.Claims, GetUserClaims<TUser>)
            .WithDescription("Gets a list of claims belonging to a user")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gets user claims";
                return operation;
            })
            .WithName(nameof(GetUserClaims))
            .WithMetadata(typeof(Endpoints.Users.GetUserClaimsEndpoint));

        usersGroup
            .MapPost(RoutePatterns.Claims, AddUserClaims<TUser>)
            .WithDescription("Adds the claims to user")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Adds user claims";
                return operation;
            })
            .WithName(nameof(AddUserClaims))
            .WithMetadata(typeof(Endpoints.Users.AddUserClaimsEndpoint));

        usersGroup
            .MapDelete(RoutePatterns.Claims, RemoveUserClaims<TUser>)
            .WithDescription("Removes the claims from user")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Removes user claims";
                return operation;
            })
            .WithName(nameof(RemoveUserClaims))
            .WithMetadata(typeof(Endpoints.Users.RemoveUserClaimsEndpoint));

        usersGroup
            .MapGet(RoutePatterns.Audit, GetUserAuditTrail)
            .WithDescription("Get a user's audit trail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get user audit trail";
                return operation;
            })
            .WithName(nameof(GetUserAuditTrail))
            .WithMetadata(typeof(Endpoints.Users.GetUserAuditTrailEndpoint));

        // Roles Endpoints
        var rolesGroup = routeGroup
            .MapGroup(RoutePatterns.Roles)
            .WithTags(nameof(RoutePatterns.Roles));

        rolesGroup
            .MapGet(RoutePatterns.Root, GetAllRoles<TRole>)
            .WithDescription("Gets all roles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get roles";
                return operation;
            })
            .WithName(nameof(GetAllRoles))
            .WithMetadata(typeof(Endpoints.Roles.GetAllRolesEndpoint));

        rolesGroup
            .MapPost(RoutePatterns.Root, CreateRole<TRole>)
            .WithDescription("Creates a new role in the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Creates a new role";
                return operation;
            })
            .WithName(nameof(CreateRole))
            .WithMetadata(typeof(Endpoints.Roles.CreateRoleEndpoint));

        rolesGroup
           .MapPut(RoutePatterns.Root, UpdateRole<TRole>)
           .WithDescription("Updates a role in the database")
           .WithOpenApi(operation =>
           {
               operation.Summary = "Updates a role";
               return operation;
           })
            .WithName(nameof(UpdateRole))
            .WithMetadata(typeof(Endpoints.Roles.UpdateRoleEndpoint));

        rolesGroup
            .MapDelete(RoutePatterns.Root, DeleteRole<TRole>)
            .WithDescription("Deletes a role from the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Deletes a role";
                return operation;
            })
            .WithName(nameof(DeleteRole))
            .WithMetadata(typeof(Endpoints.Roles.DeleteRoleEndpoint));

        rolesGroup
            .MapGet(RoutePatterns.Claims, GetRoleClaims<TRole>)
            .WithDescription("Gets a list of claims belonging to a role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gets role claims";
                return operation;
            })
            .WithName(nameof(GetRoleClaims))
            .WithMetadata(typeof(Endpoints.Roles.GetRoleClaimsEndpoint));

        rolesGroup
            .MapPost(RoutePatterns.Claims, AddRoleClaim<TRole>)
            .WithDescription("Adds a claim to role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Adds a role claim";
                return operation;
            })
            .WithName(nameof(AddRoleClaim))
            .WithMetadata(typeof(Endpoints.Roles.AddRoleClaimEndpoint));

        rolesGroup
            .MapDelete(RoutePatterns.Claims, RemoveRoleClaim<TRole>)
            .WithDescription("Removes a claim from the role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Removes a role claim";
                return operation;
            })
            .WithName(nameof(RemoveRoleClaim))
            .WithMetadata(typeof(Endpoints.Roles.RemoveRoleClaimEndpoint));

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }

    // Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
    private sealed class IdentityEndpointsConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
    {
        private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

        public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);

        public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromBodyAttribute : Attribute, IFromBodyMetadata;

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromServicesAttribute : Attribute, IFromServiceMetadata;

    [AttributeUsage(AttributeTargets.Parameter)]
    private sealed class FromQueryAttribute : Attribute, IFromQueryMetadata
    {
        public string? Name => null;
    }
}