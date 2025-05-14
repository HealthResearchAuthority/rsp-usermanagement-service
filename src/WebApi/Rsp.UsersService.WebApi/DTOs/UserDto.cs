namespace Rsp.UsersService.WebApi.DTOs;

public record UserDto(string Id,
    string FirstName,
    string LastName,
    string Email,
    string? Title,
    string? JobTitle,
    string? Organisation,
    string? Telephone,
    string? Country,
    string? Status,
    DateTime? LastLogin,
    DateTime? LastUpdated);