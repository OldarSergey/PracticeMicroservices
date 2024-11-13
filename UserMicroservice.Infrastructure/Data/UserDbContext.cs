using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UserMicroservice.Domain.Entities;

namespace UserMicroservice.Infrastructure.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var adminRoleId = Guid.NewGuid();
            var userRoleId = Guid.NewGuid();
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, IsDeleted = false, Name = "Admin" },
                new Role { Id = userRoleId, IsDeleted = false, Name = "User" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin User",
                    Email = "admin@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin1234"),
                    RoleId = adminRoleId,
                    IsDeleted = false,
                    IsEmailConfirmed = true
                }
            );

            base.OnModelCreating(modelBuilder);

        }
    }
}
