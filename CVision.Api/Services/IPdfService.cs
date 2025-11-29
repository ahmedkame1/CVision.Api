// Services/IPdfService.cs
using CVision.Api.Models;

namespace CVision.Api.Services
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdf(CV cv); // التأكد من أن الاسم مطابق
    }
}