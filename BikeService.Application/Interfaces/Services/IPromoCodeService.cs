using BikeService.Application.DTOs.PromoCode;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IPromoCodeService
    {
        Task<Result<List<PromoCodeDto>>> GetAllAsync();
        Task<Result<PromoCodeDto>> GetByIdAsync(int id);
        Task<Result<List<PromoCodeDto>>> GetActiveAsync();
        Task<Result<PromoCodeDto>> ValidateCodeAsync(string code);
        Task<Result<int>> CreateAsync(PromoCodeFormDto dto);
        Task<Result<bool>> UpdateAsync(int id, PromoCodeFormDto dto);
        Task<Result<bool>> ToggleActiveAsync(int id);
        Task<Result<bool>> DeleteAsync(int id);
    }
}
