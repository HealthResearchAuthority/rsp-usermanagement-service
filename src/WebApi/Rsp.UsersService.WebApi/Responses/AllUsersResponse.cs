using Rsp.UsersService.WebApi.DTOs;

namespace Rsp.UsersService.WebApi.Responses;

public class AllUsersResponse
{
    public IEnumerable<UserDto> Users { get; set; } = [];

    public int TotalCount { get; set; }
}