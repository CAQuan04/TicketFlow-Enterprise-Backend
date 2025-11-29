using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TicketBooking.Application.Common.Interfaces.Authentication; // Import the interface definition.

namespace TicketBooking.Infrastructure.Authentication
{
    // Implementation of the password hasher using the BCrypt algorithm.
    public class BCryptPasswordHasher : IPasswordHasher
    {
        // Hashes a plain text password.
        public string Hash(string password)
        {
            // BCrypt automatically handles generating a random salt and hashing the password.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifies if the provided password matches the stored hash.
        public bool Verify(string password, string hashedPassword)
        {
            // BCrypt extracts the salt from the hash and checks if the password matches.
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
