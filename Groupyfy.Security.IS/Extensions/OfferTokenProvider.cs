using Groupyfy.Security.Models.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;

namespace Groupyfy.Security.IS.Extensions
{
    /// <summary>
    /// Provides protection and validation of offer tokens.
    /// </summary>
    public class OfferTokenProvider : DataProtectorTokenProvider<GroupyfyUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectorTokenProvider{GroupyfyUser}"/> class.
        /// </summary>
        /// <param name="dataProtectionProvider">The system data protection provider.</param>
        /// <param name="options">The configured <see cref="DataProtectionTokenProviderOptions"/>.</param>
        public OfferTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<DataProtectionTokenProviderOptions> options)
            :base(dataProtectionProvider, options)
        {
            Options.TokenLifespan = TimeSpan.FromDays(30);
        }
    }
}
