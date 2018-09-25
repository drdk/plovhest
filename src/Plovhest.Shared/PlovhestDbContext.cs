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
        private static JsonSerializerSettings _jsonSerializerSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new IsoDateTimeConverter()
                }
                
            };

        public PlovhestDbContext(DbContextOptions<PlovhestDbContext> options)
            : base(options)
        { }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<PlovhestDbContext>();

                // auto migration
                context.Database.Migrate();

                // Seed the database.
                //Seed(context);
            }
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

          modelBuilder.Entity<Order>()
                .Property(e => e.Result)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v, _jsonSerializerSettings),
                    v => JsonConvert.DeserializeObject<JObject>(v, _jsonSerializerSettings));
        }
    }
}