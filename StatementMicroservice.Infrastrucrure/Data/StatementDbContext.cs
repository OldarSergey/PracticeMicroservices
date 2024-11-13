using Microsoft.EntityFrameworkCore;
using StatementMicroservice.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace StatementMicroservice.Infrastrucrure.Data
{
    public class StatementDbContext : DbContext
    {
        public StatementDbContext(DbContextOptions<StatementDbContext> options)
            : base(options)
        {
        }

        public DbSet<Statement> Statements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
