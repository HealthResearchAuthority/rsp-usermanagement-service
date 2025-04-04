using Microsoft.AspNetCore.Identity;
using Rsp.UsersService.Domain.Attributes;
using Rsp.UsersService.Domain.Interfaces;

namespace Rsp.UsersService.Domain.Entities;

public class UserRole : IdentityUserRole<string>, IAuditable
{
}