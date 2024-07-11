using Library.Models;
using Microsoft.EntityFrameworkCore;
using Library.Interfaces;

namespace RMSAPI
{
    public class APIAppDBContext : DbContext, IAppDbContext
    {
        public DbSet<Device> Devices { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public APIAppDBContext(DbContextOptions<APIAppDBContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=DESKTOP-3PD981G\\SQLEXPRESS01;Database=YourDatabaseName;Integrated Security=True;TrustServerCertificate=True",
                    b => b.MigrationsAssembly("RMSAPI"));  
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 기본 키 설정
            modelBuilder.Entity<Device>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<ErrorLog>()
                .HasKey(e => e.Id);
        }
    }
}
