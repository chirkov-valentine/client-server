using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Контекст БД Sqlite
    /// </summary>
    public class ClientServerDbContext : DbContext
    {
        private static bool _created = false;
        public ClientServerDbContext()
        {
            if (!_created)
            {
                _created = true;
               // Database.EnsureDeleted();
                Database.EnsureCreated();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlite(@"Data Source=.\ClientServer.db");
        }

        public DbSet<ClientMessage> ClientMessages { get; set; }
        public DbSet<ServerMessage> ServerMessages { get; set; }

    }
}
