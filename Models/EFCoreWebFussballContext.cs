using Microsoft.EntityFrameworkCore;
using vscodecore.Models;

namespace vscodecore.Models
{
    public class EFCoreWebFussballContext : DbContext
    {
        public DbSet<Contester> Contesters { get; set; }
        // public DbSet<Tournament> Tournaments { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {// TODO: change _
            optionsBuilder.UseSqlServer(@"Server=tcp:mwfussballdbserver.database.windows.net,1433;Initial Catalog=mwfussballdb;Persist Security Info=False;User ID=admin!23;Password=mwfussball!23;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
}