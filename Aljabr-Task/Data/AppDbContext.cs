using Microsoft.EntityFrameworkCore;
using SlotManagement.Models;
using System.Collections.Generic;


namespace SlotManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Slot> Slots { get; set; }
    }
}