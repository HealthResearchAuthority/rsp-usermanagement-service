using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Rsp.UsersService.Domain.Attributes;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.Infrastructure.Helpers;

public class IrasUserAuditTrailHandler : IAuditTrailHandler
{
    public bool CanHandle(object entity) => entity is IrasUser;

    public async Task<IEnumerable<UserAuditTrail>> GenerateAuditTrails(EntityEntry entity, IrasUser systemAdmin, IrasIdentityDbContext? context = null)
    {
        if (entity.Entity is not IrasUser irasUser)
        {
            return [];
        }

        var auditTrailRecords = new List<UserAuditTrail>();

        switch (entity.State)
        {
            case EntityState.Added:
                var addAuditTrail = new UserAuditTrail()
                {
                    DateTimeStamp = DateTime.UtcNow,
                    Description = $"{irasUser.Email} was created",
                    UserId = irasUser.Id,
                    SystemAdministratorId = string.IsNullOrEmpty(systemAdmin?.Id) ? null : systemAdmin.Id
                };
                auditTrailRecords.Add(addAuditTrail);
                break;

            case EntityState.Modified:
                // PropertyEntry.IsModified is not helpful as it returns true for all properties, even when
                // PropertyEntry.OriginalValue == PropertyEntry.CurrentValue. Presumably this is because we pass
                // the entire IrasUser entity to the update endpoint and it overwrite all values, even if there
                // is no change. Therefore, we must use !Equals(PropertyEntry.OriginalValue, PropertyEntry.CurrentValue)
                // to check for modifications.
                var modifiedAuditableProps = entity.Properties.Where
                (
                    p =>
                        Attribute.IsDefined(p.Metadata.PropertyInfo!, typeof(AuditableAttribute)) &&
                        !Equals(p.OriginalValue, p.CurrentValue)
                );

                foreach (var property in modifiedAuditableProps)
                {
                    var updateAuditTrail = new UserAuditTrail()
                    {
                        DateTimeStamp = DateTime.UtcNow,
                        UserId = irasUser.Id,
                        SystemAdministratorId = string.IsNullOrEmpty(systemAdmin?.Id) ? null : systemAdmin.Id
                    };

                    if (property.Metadata.Name == nameof(IrasUser.Status))
                    {
                        var newStatus = property.CurrentValue!.ToString() == "active" ? "enabled" : "disabled";

                        updateAuditTrail.Description = $"{irasUser.Email} was {newStatus}";
                    }
                    else
                    {
                        const string emptyValue = "(null)"; // business is yet to decide how to handle this case, using '(null)' for now
                        var oldValue = property.OriginalValue ?? emptyValue;
                        var newValue = property.CurrentValue ?? emptyValue;

                        if (property.Metadata.Name == nameof(IrasUser.Country))
                        {
                            oldValue = property.OriginalValue?.ToString()?.Replace(",", ", ");
                            if (string.IsNullOrEmpty(oldValue as string))
                            {
                                oldValue = emptyValue;
                            }

                            newValue = property.CurrentValue?.ToString()?.Replace(",", ", ");
                            if (string.IsNullOrEmpty(newValue as string))
                            {
                                newValue = emptyValue;
                            }
                        }

                        updateAuditTrail.Description =
                            $"{oldValue} was changed to {newValue}";
                    }

                    auditTrailRecords.Add(updateAuditTrail);
                }
                break;

            default:
                break;
        }

        return await Task.FromResult(auditTrailRecords);
    }
}