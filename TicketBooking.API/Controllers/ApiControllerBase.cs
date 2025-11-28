using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TicketBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender? _mediator;

        // Nếu _mediator chưa có, hãy lấy nó từ RequestServices (Service Locator)
        // Các Controller con chỉ cần gọi this.Mediator là dùng được ngay.
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    }
}