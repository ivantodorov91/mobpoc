using System;
using System.Collections.Generic;

namespace Groupyfy.Security.IS.Models.Account
{
    [Serializable]
    public class PickRoleViewModel : PickRoleInputModel
    {
        public string Username { get; set; }
        public Dictionary<string, string> Roles { get; set; }
        public string ReturnUrl { get; set; }
    }
}
