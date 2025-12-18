using MediatR; // Dùng thư viện MediatR.

namespace TicketBooking.Application.Features.Admin.Queries.GetAdminStats
{
    // Request yêu cầu lấy thống kê Admin.
    // Trả về AdminStatsDto.
    public record GetAdminStatsQuery : IRequest<AdminStatsDto>;
}