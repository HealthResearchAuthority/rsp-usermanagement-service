using Rsp.UsersService.WebApi.DTOs;

namespace Rsp.UsersService.WebApi.Responses;

public class AllRolesResponse
{
    public IEnumerable<RoleDto> Roles { get; set; } = [];

    public int TotalCount { get; set; }
}