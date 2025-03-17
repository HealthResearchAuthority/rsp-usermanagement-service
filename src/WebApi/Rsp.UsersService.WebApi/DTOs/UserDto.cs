namespace Rsp.UsersService.WebApi.DTOs;

public record UserDto(string Id,
    string FirstName,
    string LastName,
    string Email,
    string? title,
    string? JobTitle,
    string? Organisation,
    string? Telephone,
    string? Country,
    string? Status,
    DateTime? lastLogin,
    DateTime? lastUpdated);