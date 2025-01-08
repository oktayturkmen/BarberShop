using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Barbershop.Models;
using Microsoft.AspNetCore.Identity;

namespace Barbershop.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Barber> Barbers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Haircut> Haircuts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Barbershop.Models.Haircut>? Haircut { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Appointment ile IdentityUser ilişkisi
            builder.Entity<Appointment>()
                .HasOne<IdentityUser>(a => a.User) // User ilişkisi
                .WithMany() // Bir kullanıcının birden fazla randevusu olabilir
                .HasForeignKey(a => a.CustomerId) // CustomerId üzerinden ilişki
                .OnDelete(DeleteBehavior.SetNull); // Kullanıcı silinirse randevular anonim kalır
        }
    }
}
