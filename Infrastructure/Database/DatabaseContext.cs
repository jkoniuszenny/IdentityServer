using Core.Models.Entities;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Database
{
    public class DatabaseContext : IdentityDbContext<
        AppUser, 
        AppRole, 
        Guid,
        IdentityUserClaim<Guid>,
        AppUserRoles,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        private readonly DatabaseSettings _settings;


        public DatabaseContext(DbContextOptions<DatabaseContext> dbContextOptions, DatabaseSettings settings) : base(dbContextOptions)
        {
            _settings = settings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_settings.ConnectionString, s => s.CommandTimeout(60));
            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasKey(p => p.Id).ForSqlServerIsClustered(false).HasName("IX_AppUser_Id");
            modelBuilder.Entity<AppUser>().HasIndex(p => p.ClusterID).IsUnique().ForSqlServerIsClustered(true).HasName("IX_AppUser_ClusterID");

            modelBuilder.Entity<AppRole>().HasKey(p => p.Id).ForSqlServerIsClustered(false).HasName("IX_AppRole_Id");
            modelBuilder.Entity<AppRole>().HasIndex(p => p.ClusterID).IsUnique().ForSqlServerIsClustered(true).HasName("IX_AppRole_ClusterID");



            //modelBuilder.UseEncryption(_encryptedService);

            base.OnModelCreating(modelBuilder);
        }
    }
}
