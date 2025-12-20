using FluentAssertions; // Thư viện giúp viết Assert dễ đọc.
using Microsoft.EntityFrameworkCore; // Dùng cho InMemory DB.
using Microsoft.Extensions.Caching.Distributed; // Dùng cho IDistributedCache.
using Moq; // Dùng để Mock các dependency bên ngoài.
using TicketBooking.Application.Common.Exceptions; // Exception của dự án.
using TicketBooking.Application.Common.Interfaces; // Interfaces.
using TicketBooking.Application.Common.Interfaces.RealTime; // Notification Interface.
using TicketBooking.Application.Features.Orders.Commands.CreateOrder; // Command cần test.
using TicketBooking.Domain.Entities; // Entities.
using TicketBooking.Domain.Enums; // Enums.
using TicketBooking.Infrastructure.Data; // DbContext thực tế.
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace TicketBooking.UnitTests.Features.Orders
{
    public class CreateOrderCommandHandlerTests
    {
        // Mock các dependencies không phải Database
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<INotificationService> _mockNotificationService;

        public CreateOrderCommandHandlerTests()
        {
            // Setup chung cho các mock
            _mockUserService = new Mock<ICurrentUserService>();
            _mockCache = new Mock<IDistributedCache>();
            _mockNotificationService = new Mock<INotificationService>();

            // Giả lập luôn có một User đang đăng nhập với ID cố định
            _mockUserService.Setup(x => x.UserId).Returns(Guid.NewGuid().ToString());
        }

        // Helper function để tạo DbContext bộ nhớ đệm (Mỗi test case 1 DB riêng biệt)
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())

                // ⚠️ CẤU HÌNH QUAN TRỌNG: Bỏ qua lỗi Transaction
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))

                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_Should_CreateOrder_When_InventoryIsSufficient()
        {
            // ----------------------------------------------------------
            // 1. ARRANGE (Chuẩn bị dữ liệu và môi trường)
            // ----------------------------------------------------------
            var context = GetInMemoryDbContext();
            var ticketTypeId = Guid.NewGuid();

            // Tạo dữ liệu giả trong RAM: 1 Loại vé có số lượng 10
            var ticketType = new TicketType
            {
                Id = ticketTypeId,
                Name = "VIP Ticket",
                Price = 100000,
                Quantity = 10,
                AvailableQuantity = 10, // Còn 10 vé
                RowVersion = new byte[] { 0x01 } // Giả lập Concurrency Token
            };
            context.TicketTypes.Add(ticketType);
            await context.SaveChangesAsync();

            // Tạo Command: Mua 1 vé
            var command = new CreateOrderCommand(ticketTypeId, 1);

            // Khởi tạo Handler với các dependency đã Mock và Context InMemory
            var handler = new CreateOrderCommandHandler(
                context,
                _mockUserService.Object,
                _mockCache.Object,
                _mockNotificationService.Object
            );

            // ----------------------------------------------------------
            // 2. ACT (Thực thi hành động cần test)
            // ----------------------------------------------------------
            var result = await handler.Handle(command, CancellationToken.None);

            // ----------------------------------------------------------
            // 3. ASSERT (Kiểm tra kết quả có đúng như mong đợi không)
            // ----------------------------------------------------------

            // Kiểm tra 1: Kết quả trả về phải là một Guid (OrderId) hợp lệ
            result.Should().NotBeEmpty();

            // Kiểm tra 2: Số lượng vé trong kho phải giảm đi 1 (10 - 1 = 9)
            var updatedTicketType = await context.TicketTypes.FindAsync(ticketTypeId);
            updatedTicketType!.AvailableQuantity.Should().Be(9);

            // Kiểm tra 3: Đơn hàng phải được tạo ra trong Database với trạng thái Pending
            var order = await context.Orders.FindAsync(result);
            order.Should().NotBeNull();
            order!.Status.Should().Be(OrderStatus.Pending);
            order.TotalAmount.Should().Be(100000); // Giá 100k * 1 vé
        }

        [Fact]
        public async Task Handle_Should_ThrowValidationException_When_SoldOut()
        {
            // ----------------------------------------------------------
            // 1. ARRANGE
            // ----------------------------------------------------------
            var context = GetInMemoryDbContext();
            var ticketTypeId = Guid.NewGuid();

            // Tạo dữ liệu giả: Vé đã hết (AvailableQuantity = 0)
            var ticketType = new TicketType
            {
                Id = ticketTypeId,
                Name = "Sold Out Ticket",
                Price = 50000,
                Quantity = 10,
                AvailableQuantity = 0, // HẾT VÉ
                RowVersion = new byte[] { 0x01 }
            };
            context.TicketTypes.Add(ticketType);
            await context.SaveChangesAsync();

            var command = new CreateOrderCommand(ticketTypeId, 1); // Cố mua 1 vé

            var handler = new CreateOrderCommandHandler(
                context,
                _mockUserService.Object,
                _mockCache.Object,
                _mockNotificationService.Object
            );

            // ----------------------------------------------------------
            // 2. ACT (Sử dụng Delegate để bắt Exception)
            // ----------------------------------------------------------
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // ----------------------------------------------------------
            // 3. ASSERT
            // ----------------------------------------------------------
            // Mong đợi hệ thống ném lỗi ValidationException
            await act.Should().ThrowAsync<ValidationException>();

            // Kiểm tra phụ: Kho vé vẫn phải là 0, không được bị âm
            var dbTicketType = await context.TicketTypes.FindAsync(ticketTypeId);
            dbTicketType!.AvailableQuantity.Should().Be(0);
        }
    }
}