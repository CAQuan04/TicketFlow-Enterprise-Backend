using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TicketBooking.Domain.Common;
using TicketBooking.Domain.Enums;


namespace TicketBooking.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }

        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public string? VerificationToken { get; set; } // Stores the 6-digit OTP.
        public DateTime? VerificationTokenExpires { get; set; } // Stores expiration time.
        public bool EmailConfirmed { get; set; } = false; // Flag to check if verified.

        // --- REFRESH TOKEN FIELDS ---
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

    }
}
