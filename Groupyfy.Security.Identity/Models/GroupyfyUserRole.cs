using Microsoft.AspNetCore.Identity;
using System;

namespace Groupyfy.Security.Models.Identity
{
    public class GroupyfyUserRole : IdentityUserRole<Guid>
    {
        public GroupyfyUserRole()
        {

        }

        public GroupyfyUserRole(Guid userId, Guid roleId, Guid? corporateId)
        {
            UserId = userId;
            RoleId = roleId;
            CorporateId = corporateId;
        }

        public Guid? CorporateId { get; set; }
    }
}
