using TicketBooking.Domain.Entities; // Assuming we pass some context or DTO.

namespace TicketBooking.Application.Common.Interfaces.Payments
{
    // Strategy Interface for Payment Gateways (VNPay, Momo, Stripe...).
    public interface IPaymentGateway
    {
        // Generates the redirect URL for the user to pay.
        string CreatePaymentUrl(decimal amount, string transactionRef, string ipAddress);
    }
}