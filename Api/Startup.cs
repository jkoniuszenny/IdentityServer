using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NLog.Extensions.Logging;
using NLog.Config;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infrastructure.Database;
using System.IO;
using Microsoft.Extensions.Logging;
using Infrastructure.Settings;
using Api.Extensions;
using Core.NLog;
using Infrastructure.IoC;
using Core.NLog.Interfaces;
using Microsoft.AspNetCore.Identity;
using Core.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Infrastructure.Extensions;
using Infrastructure;
using static Infrastructure.IdentityProvider;

namespace Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddRouting();

            services.AddMvc().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

            services.AddControllers();

            services.AddDbContext<DatabaseContext>(ServiceLifetime.Transient);

            services.AddIdentity<AppUser, AppRole> (o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
              
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<DatabaseContext>()
            .AddDefaultTokenProviders();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddIdentityServer().AddDeveloperSigningCredential()
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder => builder.UseSqlServer(Configuration.GetSettings<DatabaseSettings>().ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                }).AddConfigurationStore(options =>
                      options.ConfigureDbContext = builder => builder.UseSqlServer(Configuration.GetSettings<DatabaseSettings>().ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly))
                )
                  .AddAspNetIdentity<AppUser>();

            services.AddAuthentication();

            var builder = new ContainerBuilder();

            builder.Populate(services);
            builder.RegisterModule(new ContainerModule(Configuration));
            builder.RegisterType<NLogLogger>()
                                .As<INLogLogger>()
                                .SingleInstance();
            builder.RegisterType<NLogTimeLogger>()
                    .As<INLogTimeLogger>()
                    .SingleInstance();
            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            string nlogFilePath = "nlog" + env.EnvironmentName + ".config";

            if (File.Exists(nlogFilePath))
            {
                loggerFactory.AddNLog();
                loggerFactory.ConfigureNLog(nlogFilePath);
                LogManager.Configuration.Install(new InstallationContext());
            }


            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            

            app.ConfigureExceptionHandler();

            var responseTimeSettings = app.ApplicationServices.GetService<ResponseTimeSettings>();
            if (responseTimeSettings.Enabled)
            {
                app.ConfigureResponseTime();

            }
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next().ConfigureAwait(false);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
                serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                if (!context.Clients.Any())
                {
                    foreach (var client in Clients.Get())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Resources.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var scope in Resources.GetApiScopes())
                    {
                        context.ApiScopes.Add(scope.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Resources.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                //var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                //if (!userManager.Users.Any())
                //{
                //    foreach (var testUser in Users.Get())
                //    {
                //        var identityUser = new IdentityUser(testUser.Username)
                //        {
                //            Id = testUser.SubjectId
                //        };

                //        userManager.CreateAsync(identityUser, "Password123!").Wait();
                //        userManager.AddClaimsAsync(identityUser, testUser.Claims.ToList()).Wait();
                //    }
                //}
            }
        }
    }
}
