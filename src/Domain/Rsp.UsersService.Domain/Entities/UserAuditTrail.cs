using System.Diagnostics.CodeAnalysis;

namespace Rsp.UsersService.Domain.Entities;

[ExcludeFromCodeCoverage]
public class UserAuditTrail
{
    public Guid Id { get; set; }
    public DateTime DateTimeStamp { get; set; }
    public string Description { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string SystemAdministratorId { get; set; } = null!;

    // Navigation properties
    public IrasUser User { get; set; } = null!;

    public IrasUser SystemAdministrator = null!;
}