using System.Diagnostics.CodeAnalysis;
using KellermanSoftware.CompareNetObjects;
using Rsp.UsersService.Application.Constants;
using Rsp.UsersService.Domain.Entities;

namespace Rsp.UsersService.WebApi.Helpers;

[ExcludeFromCodeCoverage]
public static class AuditTrailHelper
{
    public static IEnumerable<UserAuditTrail> GenerateAuditTrail(IrasUser user, string action, string adminId, IrasUser? oldUser = null, IEnumerable<string>? roles = null)
    {
        var result = new List<UserAuditTrail>();

        switch (action)
        {
            case AuditTrailActions.Create:
                result.Add(new UserAuditTrail
                {
                    DateTimeStamp = DateTime.UtcNow,
                    Description = $"{user.Email} was created",
                    UserId = user.Id,
                    SystemAdministratorId = adminId,
                });
                break;

            case AuditTrailActions.Update:
                var compareConfig = new CompareLogic
                (
                    new ComparisonConfig
                    {
                        MaxDifferences = 20,
                        MembersToInclude =
                        [
                            nameof(IrasUser.Email),
                            nameof(IrasUser.FirstName),
                            nameof(IrasUser.LastName),
                            nameof(IrasUser.Title),
                            nameof(IrasUser.Telephone),
                            nameof(IrasUser.Organisation),
                            nameof(IrasUser.Country),
                            nameof(IrasUser.JobTitle),
                            nameof(IrasUser.Status),
                        ]
                    }
                );

                var compareObjectsResult = compareConfig.Compare(user, oldUser);

                if (!compareObjectsResult.AreEqual)
                {
                    foreach (var diff in compareObjectsResult.Differences)
                    {
                        if (diff.PropertyName == nameof(IrasUser.Status))
                        {
                            var newStatus = diff.Object1Value == "active" ? "enabled" : "disabled";

                            result.Add(new UserAuditTrail
                            {
                                DateTimeStamp = DateTime.UtcNow,
                                Description = $"{user.Email} was {newStatus}",
                                UserId = user.Id,
                                SystemAdministratorId = adminId,
                            });
                        }
                        else
                        {
                            if (diff.PropertyName == nameof(IrasUser.Country))
                            {
                                diff.Object1Value = String.Join(", ", diff.Object1Value.Split(","));
                                diff.Object2Value = String.Join(", ", diff.Object2Value.Split(","));
                            }

                            result.Add(new UserAuditTrail
                            {
                                DateTimeStamp = DateTime.UtcNow,
                                Description = $"{diff.Object2Value} was changed to {diff.Object1Value}",
                                UserId = user.Id,
                                SystemAdministratorId = adminId,
                            });
                        }
                    }
                }
                break;

            case AuditTrailActions.AddRole:
                roles ??= [];

                foreach (var role in roles)
                {
                    result.Add(new UserAuditTrail
                    {
                        DateTimeStamp = DateTime.UtcNow,
                        Description = $"{user.Email} was assigned {role} role",
                        UserId = user.Id,
                        SystemAdministratorId = adminId,
                    });
                }
                break;

            case AuditTrailActions.RemoveRole:
                roles ??= [];

                foreach (var role in roles)
                {
                    result.Add(new UserAuditTrail
                    {
                        DateTimeStamp = DateTime.UtcNow,
                        Description = $"{user.Email} was unassigned {role} role",
                        UserId = user.Id,
                        SystemAdministratorId = adminId,
                    });
                }
                break;
        }

        return result;
    }
}