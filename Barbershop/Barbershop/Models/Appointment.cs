using Microsoft.AspNetCore.Identity;

namespace Barbershop.Models
{
    public enum AppointmentStatus
    {
        Pending,        // Beklemede
        Confirmed,      // Onaylı
        Rejected,       // Reddedilmiş
        Completed,      // Tamamlanmış
        Canceled,        // İptal Edilmiş
        
    }

    public class Appointment
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public int BarberId { get; set; }
        public int HaircutId { get; set; }
        public string? CustomerId { get; set; }
        public AppointmentStatus Status { get; set; }

        // Parent References
        public Barber? Barber { get; set; }
        public Haircut? Haircut { get; set; }

        // User Reference
        public IdentityUser? User { get; set; }
    }
}
