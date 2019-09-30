using System;

namespace Groupyfy.Security.IS.Models.Account
{
    public class PickRoleInputModel
    {
        public string Role { get; set; }
        public Guid? CorporateId { get; set; }
        public string ReturnUrl { get; set; }
    }
}
