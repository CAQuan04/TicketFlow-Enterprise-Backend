using Microsoft.AspNetCore.Http;

namespace TicketBooking.Application.Common.Interfaces.Payments
{
    // Interface này giúp lớp Application nhờ lớp Infrastructure kiểm tra chữ ký
    // mà không cần biết logic mã hóa cụ thể (HashSecret) là gì.
    public interface IVnPayValidationService
    {
        // Trả về true nếu chữ ký hợp lệ, false nếu sai.
        bool ValidateSignature(IQueryCollection queryData);
    }
}