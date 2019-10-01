using Groupyfy.Security.Models.Identity;
using Groupyfy.Security.IS;
using Groupyfy.Security.Persistence;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Security.Claims;
using IdentityServer4.Services;
using Groupyfy.Security.IS.Extensions;
using IdentityModel;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class GroupyfyIdentityServerServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IdentityBuilder AddGroupyfyIdentity(this IServiceCollection service, IConfiguration configuration, IHostingEnvironment env)
            => service
            .AddIdentity<GroupyfyUser, GroupyfyRole>()
                .AddEntityFrameworkStores<GroupyfySecurityDbContext>()
                .AddUserManager<GroupyfyUserManager>()
                .AddUserStore<GroupyfyUserStore>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<OfferTokenProvider>("offertoken");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddGroupyfyIdentityServer(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment env)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var builder = services
                .AddIdentityServer()
                .AddAspNetIdentity<GroupyfyUser>()
                .AddInMemoryIdentityResources(ISConfig.GetIdentityResources())
                .AddInMemoryClients(ISConfig.GetClients())
                .AddInMemoryApiResources(ISConfig.GetApis(configuration))
                .AddInMemoryPersistedGrants()
                .AddInMemoryCaching()
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(configuration.GetConnectionString("GroupyfySecurityDatabase"),
                            db => db.MigrationsAssembly(migrationsAssembly));
                });

            services.AddTransient<IProfileService, GroupyfyProfileService>();
            if (env.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
                
            }

            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddGroupyfyISAuthentication(this IServiceCollection services, IConfiguration configuration, IHostingEnvironment env)
        {
            var builder = services
                    .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(x =>
                    {
                        
                        x.Authority = configuration.GetSection("IdentityServer:Authority").Value;
                        x.ApiName = "groupyfy-api";
                        x.RequireHttpsMetadata = true;
                        x.JwtValidationClockSkew = TimeSpan.Zero;
                        x.SupportedTokens = SupportedTokens.Reference;
                        x.EnableCaching = true;
                        x.CacheDuration = TimeSpan.FromMinutes(1);
                        x.RoleClaimType = JwtClaimTypes.Role;
                        x.ApiSecret = configuration.GetSection("GroupyfyAPI:Secret").Value;
                        x.Validate();
                    });

            return builder;
        }
    }
}
