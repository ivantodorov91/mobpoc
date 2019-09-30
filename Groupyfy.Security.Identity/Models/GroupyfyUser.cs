using Microsoft.AspNetCore.Identity;
using System;

namespace Groupyfy.Security.Models.Identity
{
    public class GroupyfyUser : IdentityUser<Guid>
    {
        public GroupyfyUser()
        {
        }
    }
}
