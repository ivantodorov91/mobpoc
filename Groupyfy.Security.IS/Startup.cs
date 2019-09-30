using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Groupyfy.Security.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using IdentityServer4;

namespace Groupyfy.Security.IS
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        private readonly string _appName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        public IHostingEnvironment Environment { get; set; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });
            

            services.AddSwaggerGen(o =>
            {
                o.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "Reference token",
                    Name = "Authorization",
                    Type = "Reference token"
                });
                o.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Flow = "implicit",
                    AuthorizationUrl = $"{Configuration.GetSection("IdentityServer:Authority").Value}/connect/authorize",
                    Scopes = new Dictionary<string, string> {
                        { "groupyfy-api", "Groupyfy API" }
                    }
                });

                o.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });
                o.SwaggerDoc("v1", new Info { Title = _appName, Version = "v1" });
                o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            });
            services.AddDbContext<GroupyfySecurityDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("GroupyfySecurityDatabase")));
            services.AddGroupyfyIdentity(Configuration, Environment);
            services.AddGroupyfyIdentityServer(Configuration, Environment);
            services.AddGroupyfyISAuthentication(Configuration, Environment);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", _appName);
                });
            }

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseApiVersioning();
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.UseMvc();
        }
    }
}
