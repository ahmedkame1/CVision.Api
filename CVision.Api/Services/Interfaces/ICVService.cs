// Services/Interfaces/ICVService.cs
using CVision.Api.DTOs.CV;
using CVision.Api.Models;

namespace CVision.Api.Services.Interfaces
{
    public interface ICVService
    {
        Task<CV> CreateCVAsync(int userId, CreateCvDto createCvDto);
        Task<CV> GetCVByIdAsync(int cvId);
        Task<List<CV>> GetUserCVsAsync(int userId);
        Task<bool> UpdateCVAsync(int cvId, CreateCvDto updateCvDto);
        Task<bool> DeleteCVAsync(int cvId);
        Task<bool> SetPrimaryCVAsync(int userId, int cvId);
        Task<byte[]> GenerateCvPdfAsync(int cvId); // تغيير من string إلى byte[]
    }
}