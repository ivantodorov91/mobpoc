using Groupyfy.Security.Models.Identity;
using Groupyfy.Security.Persistence;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groupyfy.Security.IS.Extensions
{
    public class GroupyfyProfileService : IProfileService
    {
        private GroupyfyUserManager _userManager;
        private IUserClaimsPrincipalFactory<GroupyfyUser> _claimsFactory;

        public GroupyfyProfileService(GroupyfyUserManager userManager, IUserClaimsPrincipalFactory<GroupyfyUser> claimsFactory)
        {
            _claimsFactory = claimsFactory;
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToList();
            claims.RemoveAll(x => x.Type == JwtClaimTypes.Role);

            if (!string.IsNullOrEmpty(context.ValidatedRequest?.Raw["role"]))
            {
                var corporateId = context.ValidatedRequest.Raw["corporateid"];
                var role = context.ValidatedRequest.Raw["role"];
                var userHasRole = await _userManager.IsInGroupyfyRoleAsync(user, context.ValidatedRequest.Raw["role"], corporateId != null ? Guid.Parse(corporateId) : (Guid?)null);
                if (userHasRole)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role, role));
                    if (!string.IsNullOrEmpty(corporateId))
                        claims.Add(new Claim("corporateId", corporateId));
                }
            }
            else
            {
                var role = context.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Role)?.Value;
                var corporateId = context.Subject.Claims.FirstOrDefault(x => x.Type == "corporateId")?.Value;

                if (!string.IsNullOrEmpty(role))
                    claims.Add(new Claim(JwtClaimTypes.Role, role));
                if (!string.IsNullOrEmpty(corporateId))
                    claims.Add(new Claim("corporateId", corporateId));
            }

            if (!string.IsNullOrEmpty(context.ValidatedRequest?.Raw["offerid"]))
                    claims.Add(new Claim("offerId", context.ValidatedRequest.Raw["offerid"]));

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            //throw new NotImplementedException();

            //await Task.Run(() => Console.WriteLine(1));
        }
    }
}
