using System;
using Microsoft.EntityFrameworkCore;
using vscodecore.Models;

namespace vscodecore.Models
{
    public class EFCoreWebFussballContext : DbContext
    {
        public DbSet<Contester> Contesters { get; set; }
        // public DbSet<Tournament> Tournaments { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {   
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

            if (string.IsNullOrEmpty(connectionString)) {
                throw new Exception("DB connection string environment variable 'SQL_CONNECTION_STRING' is not set!");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}