using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groupyfy.Security.IS.Extensions
{
    public class GroupyfyProfileService : IProfileService
    {
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims = context.Subject.Claims.ToList();
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //throw new NotImplementedException();

            //await Task.Run(() => Console.WriteLine(1));
        }
    }
}
