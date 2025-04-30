using Rsp.UsersService.WebApi.DTOs;

namespace Rsp.UsersService.WebApi.Responses;

public class UserResponse
{
    public UserDto User { get; set; } = null!;

    public IEnumerable<string> Roles { get; set; } = [];

    public IEnumerable<string> AccessRequired { get; set; } = [];
}