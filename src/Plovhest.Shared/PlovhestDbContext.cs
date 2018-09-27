using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Plovhest.Shared
{
    public class PlovhestDbContext : DbContext
    {
       

        public PlovhestDbContext(DbContextOptions<PlovhestDbContext> options)
            : base(options)
        { }

        public static void Initialize(IServiceProvider serviceProvider, bool delete = false)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetService<PlovhestDbContext>())
            {
                

                // auto migration
                if (delete)
                    context.Database.EnsureDeleted();
                context.Database.Migrate();
                // Seed the database.
                //Seed(context);
            }
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(e => e.Callback)
                .HasConversion(
                    v => v.ToString(),
                    v => new Uri(v));

            modelBuilder.Entity<Order>()
                .Property(e => e.Data)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, JsonHelper.JsonSerializerSettings),
                    v => JsonConvert.DeserializeObject<JObject>(v, JsonHelper.JsonSerializerSettings));
        }
    }
}