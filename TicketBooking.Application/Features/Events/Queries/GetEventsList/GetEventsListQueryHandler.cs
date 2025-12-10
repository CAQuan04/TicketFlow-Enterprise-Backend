using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketBooking.Application.Common.Interfaces;
using TicketBooking.Application.Common.Models;
using TicketBooking.Domain.Enums;

namespace TicketBooking.Application.Features.Events.Queries.GetEventsList
{
    public class GetEventsListQueryHandler : IRequestHandler<GetEventsListQuery, PagedResult<EventListDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetEventsListQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<EventListDto>> Handle(GetEventsListQuery request, CancellationToken cancellationToken)
        {
            // 1. QUERY INITIALIZATION (DEFERRED EXECUTION)
            // Khởi tạo câu truy vấn IQueryable. Tại dòng này, CHƯA CÓ SQL nào được gửi xuống DB.
            // AsNoTracking(): Tắt Change Tracker vì đây là thao tác chỉ đọc (Read-Only). 
            // -> Tăng tốc độ truy vấn lên ~20%.
            var query = _context.Events
                .AsNoTracking()
                .Include(e => e.Venue) // Chuẩn bị Join bảng Venue.
                .Where(e => e.Status == EventStatus.Published) // Business Rule: Chỉ lấy sự kiện đã Public.
                .AsQueryable();

            // 2. DYNAMIC FILTERING (XÂY DỰNG MỆNH ĐỀ WHERE ĐỘNG)
            // EF Core sẽ tự động chuyển đổi các dòng C# này thành SQL "WHERE ... AND ..." tương ứng.

            // Lọc theo từ khóa (SearchTerm)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                // SQL Translation: AND (Name LIKE '%term%' OR Description LIKE '%term%')
                string term = request.SearchTerm.Trim();
                query = query.Where(e => e.Name.Contains(term) || e.Description.Contains(term));
            }

            // Lọc theo địa điểm (VenueId)
            if (request.VenueId.HasValue)
            {
                // SQL Translation: AND VenueId = 'guid-value'
                query = query.Where(e => e.VenueId == request.VenueId);
            }

            // Lọc theo ngày (FromDate)
            if (request.FromDate.HasValue)
            {
                // SQL Translation: AND StartDateTime >= '2025-10-10'
                // Logic: Lấy các sự kiện diễn ra từ ngày được chọn trở về sau.
                var date = request.FromDate.Value.Date;
                query = query.Where(e => e.StartDateTime >= date);
            }

            // 3. SORTING (SẮP XẾP)
            // SQL Translation: ORDER BY StartDateTime ASC
            // Logic: Sự kiện sắp diễn ra hiển thị trước.
            query = query.OrderBy(e => e.StartDateTime);

            // 4. PAGINATION EXECUTION (THỰC THI TRUY VẤN)

            // Bước A: Đếm tổng số bản ghi thỏa mãn điều kiện (để tính số trang).
            // SQL Translation: SELECT COUNT(*) FROM Events WHERE ...
            var totalCount = await query.CountAsync(cancellationToken);

            // Bước B: Lấy dữ liệu trang hiện tại + Projection (Chọn cột cần thiết).
            // SQL Translation: SELECT Id, Name, ... FROM Events ... OFFSET x ROWS FETCH NEXT y ROWS ONLY
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize) // Bỏ qua các trang trước.
                .Take(request.PageSize) // Lấy số lượng của trang hiện tại.
                .Select(e => new EventListDto(
                    e.Id,
                    e.Name,
                    // Cắt chuỗi Description ngay tại DB để giảm băng thông mạng.
                    e.Description.Length > 100 ? e.Description.Substring(0, 100) + "..." : e.Description,
                    e.StartDateTime,
                    e.CoverImageUrl,
                    e.Venue.Name,     // Lấy tên Venue từ bảng đã Join.
                    e.Venue.Address,  // Lấy địa chỉ.
                                      // Tính giá vé thấp nhất để hiển thị "Giá từ...".
                    e.TicketTypes.Any() ? e.TicketTypes.Min(t => t.Price) : 0
                ))
                .ToListAsync(cancellationToken); // Lúc này SQL mới thực sự chạy và trả dữ liệu về.

            // 5. WRAPPING RESULT
            return new PagedResult<EventListDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }
}