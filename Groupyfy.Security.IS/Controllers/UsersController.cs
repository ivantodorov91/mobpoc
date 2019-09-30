using Groupyfy.Security.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Groupyfy.Security.IS.Controllers
{
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly GroupyfyUserManager _userManager;

        public UsersController(GroupyfyUserManager userManager)
        {
            _userManager = userManager;
        }


        [HttpPost("candidate/offerloginlink")]
        public async Task<ActionResult<string>> GenerateOfferLoginLink(OfferLoginLinkCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());

            if (await _userManager.IsInGroupyfyRoleAsync(user, "candidate", command.CorporateId))
            {
                var offerToken = await _userManager.GenerateOfferTokenAsync(user, command.OfferId);

                var link = "http://localhost:4200/#/candidate/login?" + $"userid={command.UserId}&corporateid={command.CorporateId}&offerid={command.OfferId}&token={WebUtility.UrlEncode(offerToken)}";

                return link;
            }

            return BadRequest();
        }
    }


    public class OfferLoginLinkCommand
    {
        [Required]
        public Guid CorporateId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid OfferId { get; set; }
    }
}
