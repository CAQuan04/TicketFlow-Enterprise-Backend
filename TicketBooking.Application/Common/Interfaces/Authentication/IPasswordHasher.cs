using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketBooking.Application.Common.Interfaces.Authentication
{
    // Interface defines the contract for password hashing operations.
    public interface IPasswordHasher
    {
        // Hashes a plain text password into a secure string.
        string Hash(string password);

        // Verifies if a plain text password matches a given hash.
        bool Verify(string password, string hashedPassword);
    }
}