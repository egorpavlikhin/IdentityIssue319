using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using IdentityIssue319.Models;
using Castle.Windsor;
using Castle.MicroKernel.Lifestyle;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Security;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.Runtime;

namespace IdentityIssue319
{
	public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
			Configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; set; }

        private WindsorContainer container = new WindsorContainer();

        public void ConfigureServices(IServiceCollection services)
        {
            services
                    .AddEntityFramework(Configuration)
                    .AddSqlServer()
                    .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.Get("Data:DefaultConnection:ConnectionString")));
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole();  
            app.UseStaticFiles();

            app.UseServices(services => {
                services
                    .AddEntityFramework(Configuration)
                    .AddSqlServer()
                    .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.Get("Data:DefaultConnection:ConnectionString")));
                services.Configure<IdentityDbContextOptions>(options =>
                {
                    options.DefaultAdminUserName = Configuration.Get("DefaultAdminUsername");
                    options.DefaultAdminPassword = Configuration.Get("DefaultAdminPassword");
                });

                //services.Configure<PasswordHasherOptions>(options => { }); // ERROR: uncommenting this fixes the problem
				services.Configure<EmailTokenProviderOptions>(options => { });
				services.Configure<PhoneNumberTokenProviderOptions>(options => { });
				services.Configure<AuthorizationOptions>(options => { });

                services.AddOptions();

				// Add Identity services to the services container.
				services
                    .AddIdentity<ApplicationUser, IdentityRole>(Configuration)
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();                

                // Add MVC services to the services container.
                services
                    .AddMvc();

                container.Populate(services);

				container.BeginScope();

				return container.Resolve<IServiceProvider>();
            });

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

			app.UseCookieAuthentication(options =>
			{
				options.AuthenticationMode = AuthenticationMode.Passive;
				options.AuthenticationType = IdentityOptions.ApplicationCookieAuthenticationType;
				options.LoginPath = new PathString("/Account/Login");
			});
			
			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });

            SampleData.InitializeIdentityDatabaseAsync(app.ApplicationServices).Wait();
        }
    }
}
