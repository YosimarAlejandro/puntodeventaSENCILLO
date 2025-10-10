using Microsoft.EntityFrameworkCore;
using System.IO;

namespace MiPOS.Models
{
    public class PosDbContext : DbContext
    {
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ruta a ProgramData para que la app pueda escribir sin elevar privilegios
            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS", "data");
            Directory.CreateDirectory(dataFolder);
            var dbPath = Path.Combine(dataFolder, "pos.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
