using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
u

namespace Library.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Device> Devices { get; set; }
       
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=DESKTOP-3PD981G\\SQLEXPRESS01",
                    b => b.MigrationsAssembly("Library"));  
            }
        }
    }
}
