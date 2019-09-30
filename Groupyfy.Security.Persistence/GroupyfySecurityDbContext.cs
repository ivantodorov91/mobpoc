using Groupyfy.Security.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Groupyfy.Security.Persistence
{
    public class GroupyfySecurityDbContext 
        : IdentityDbContext<
            GroupyfyUser,
            GroupyfyRole, 
            Guid, 
            IdentityUserClaim<Guid>, 
            GroupyfyUserRole, 
            IdentityUserLogin<Guid>, 
            IdentityRoleClaim<Guid>, 
            IdentityUserToken<Guid>>
    {
        public GroupyfySecurityDbContext(DbContextOptions<GroupyfySecurityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
