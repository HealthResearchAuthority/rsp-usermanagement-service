using Rsp.UsersService.WebApi.DTOs;

namespace Rsp.UsersService.WebApi.Responses;

public class UserAuditTrailResponse
{
    public string Name { get; set; } = null!;

    public List<UserAuditTrailDto> Items { get; set; } = [];
}