// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
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
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUserClaimsEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUserEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.GetUsersInRoleEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.RegisterUserEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.RemoveUserClaimsEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.RemoveUserFromRolesEndpoint;
using static Rsp.UsersService.WebApi.Endpoints.Users.UpdateUserEndpoint;

namespace Rsp.UsersService.WebApi.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add identity endpoints.
/// </summary>
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
    public static IEndpointConventionBuilder MapCustomizedIdentityApi<TUser, TRole>(this IEndpointRouteBuilder endpoints)
        where TUser : IrasUser, new()
        where TRole : IdentityRole, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var routeGroup =
            endpoints
                .MapGroup("")
                .RequireAuthorization();

        var usersGroup = routeGroup
            .MapGroup("/users")
            .WithTags("Users");

        usersGroup
            .MapGet("/all", GetAllUsers<TUser>)
            .WithDescription("Gets all users")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get users";
                return operation;
            });

        usersGroup
            .MapGet("/", GetUserByIdOrEmail<TUser>)
            .WithDescription("Get user by id or email")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get user";
                return operation;
            });

        usersGroup
            .MapGet("/role", GetUsersInRole<TUser>)
            .WithDescription("Get all users in a role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get users in a role";
                return operation;
            });

        usersGroup
            .MapPost("/", RegisterUser<TUser>)
            .WithDescription("Creates a user in the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user";
                return operation;
            });

        usersGroup
            .MapPut("/", UpdateUser<TUser>)
            .WithDescription("Updates a user in the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Updates a user";
                return operation;
            });

        usersGroup
            .MapDelete("/", DeleteUser<TUser>)
            .WithDescription("Deletes a user from the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Deletes a user";
                return operation;
            });

        usersGroup
            .MapPost("/roles", AddUserToRoles<TUser>)
            .WithDescription("Adds a user to a role or multiple roles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Adds a user to role(s)";
                return operation;
            });

        usersGroup
            .MapDelete("/roles", RemoveUserFromRoles<TUser>)
            .WithDescription("Removes a user from a role or multiple roles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Removes a user from role(s)";
                return operation;
            });

        usersGroup
            .MapGet("/claims", GetUserClaims<TUser>)
            .WithDescription("Gets a list of claims belonging to a user")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gets user claims";
                return operation;
            });

        usersGroup
            .MapPost("/claims", AddUserClaims<TUser>)
            .WithDescription("Adds the claims to user")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Adds user claims";
                return operation;
            });

        usersGroup
            .MapDelete("/claims", RemoveUserClaims<TUser>)
            .WithDescription("Removes the claims from user")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Removes user claims";
                return operation;
            });

        // Roles Endpoints
        var rolesGroup = routeGroup
            .MapGroup("/roles")
            .WithTags("Roles");

        rolesGroup
            .MapGet("/", GetAllRoles<TRole>)
            .WithDescription("Gets all roles")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get roles";
                return operation;
            });

        rolesGroup
            .MapPost("/", CreateRole<TRole>)
            .WithDescription("Creates a new role in the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Creates a new role";
                return operation;
            });

        rolesGroup
           .MapPut("/", UpdateRole<TRole>)
           .WithDescription("Updates a role in the database")
           .WithOpenApi(operation =>
           {
               operation.Summary = "Updates a role";
               return operation;
           });

        rolesGroup
            .MapDelete("/", DeleteRole<TRole>)
            .WithDescription("Deletes a role from the database")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Deletes a role";
                return operation;
            });

        rolesGroup
            .MapGet("/claims", GetRoleClaims<TRole>)
            .WithDescription("Gets a list of claims belonging to a role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Gets role claims";
                return operation;
            });

        rolesGroup
            .MapPost("/claims", AddRoleClaim<TRole>)
            .WithDescription("Adds a claim to role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Adds a role claim";
                return operation;
            });

        rolesGroup
            .MapDelete("/claims", RemoveRoleClaim<TRole>)
            .WithDescription("Removes a claim from the role")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Removes a role claim";
                return operation;
            });

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