using Rsp.UsersService.WebApi.DTOs;

namespace Rsp.UsersService.WebApi.Responses;
public record ClaimsResponse
{
    public IEnumerable<ClaimDto> Claims { get; set; } = [];
}