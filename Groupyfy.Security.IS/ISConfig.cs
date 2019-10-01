// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Groupyfy.Security.IS
{
    /// <summary>
    /// 
    /// </summary>
    public static class ISConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApis(IConfiguration configuration)
        {
            return new ApiResource[]
            {
                new ApiResource("groupyfy-api", "Groupyfy API")
                {
                    ApiSecrets =
                    {
                        new Secret(configuration.GetSection("GroupyfyAPI:Secret").Value.Sha512())
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "groupyfy-app",
                    ClientName = "Groupyfy App Portal",
                    ClientUri = "http://localhost:4200", //to change to js

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    
                    //change to js
                    RedirectUris =
                    {
                        "https://localhost:44352/home/index",
                        "https://localhost:44352/swagger/oauth2-redirect.html",
                        "http://localhost:4200/assets/oidc-login-redirect.html"
                    },

                    //change to js
                    PostLogoutRedirectUris = { "http://localhost:44352/" },
                    AllowedCorsOrigins = { "https://localhost:44352", "http://localhost:4200" },

                    AllowedScopes = { "openid", "profile", "groupyfy-api", "email" },

                    IdentityTokenLifetime = 12000,
                    AccessTokenLifetime = 12000,
                    AccessTokenType = AccessTokenType.Reference
                }
            };
        }
    }
}