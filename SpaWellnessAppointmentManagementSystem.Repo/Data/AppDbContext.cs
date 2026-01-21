using Microsoft.EntityFrameworkCore;
using SpaWellnessAppointmentManagementSystem.Repo.Models;

namespace SpaWellnessAppointmentManagementSystem.Repo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        // This tells EF Core to create a 'Services' table in SQL Server
        public DbSet<SpaService> Services { get; set; }
        public DbSet<Staffs> Staffs { get; set; }
        public DbSet<Appointment> Appointments { get; set; } 

    }
}