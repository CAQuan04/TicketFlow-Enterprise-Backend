using MediatR;
namespace TicketBooking.Application.Features.Payments.Commands.CreateDeposit
{
    // Command to initiate a deposit. Returns the VNPay URL.
    public record CreateDepositCommand(decimal Amount) : IRequest<string>;
}