namespace Rsp.UsersService.WebApi.DTOs;

public record UserDto(string Id,
    string GivenName,
    string FamilyName,
    string Email,
    string? IdentityProviderId,
    string? Title,
    string? JobTitle,
    string? Organisation,
    string? Telephone,
    string? Country,
    string? Status,
    DateTime? LastLogin,
    DateTime? CurrentLogin,
    DateTime? LastUpdated);