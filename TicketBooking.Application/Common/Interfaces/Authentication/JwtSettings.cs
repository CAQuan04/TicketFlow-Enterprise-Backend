using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBooking.Infrastructure.Authentication
{
    // A POCO class to hold JWT configuration settings mapped from appsettings.json.
    public class JwtSettings
    {
        // The constant name of the section in the configuration file.
        public const string SectionName = "JwtSettings";

        // The secret key used to sign the token (must be strong).
        public string Secret { get; init; } = null!;

        // The duration in minutes before the token expires.
        public int ExpiryMinutes { get; init; }

        // The issuer of the token (e.g., our server).
        public string Issuer { get; init; } = null!;

        // The intended audience for the token (e.g., our clients).
        public string Audience { get; init; } = null!;
    }
}