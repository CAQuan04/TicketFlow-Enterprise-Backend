namespace TicketBooking.Application.Common.Interfaces.AI
{
    public interface IRecommendationService
    {
        // Nhận vào User hiện tại và danh sách Event đang bán.
        // Trả về danh sách EventId đã được sắp xếp theo độ phù hợp.
        List<Guid> GetRecommendedEvents(Guid userId, List<Guid> candidateEventIds);
    }
}