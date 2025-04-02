using System.Diagnostics.CodeAnalysis;

namespace Rsp.UsersService.WebApi.DTOs;

[ExcludeFromCodeCoverage]
public class UserAuditTrailDto
{
    public DateTime DateTimeStamp { get; set; }
    public string Description { get; set; } = null!;
    public string SystemAdmin { get; set; } = null!;
}