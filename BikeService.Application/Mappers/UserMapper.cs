using BikeService.Application.DTOs.Identity;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class UserMapper
    {
        public static AppUser ToEntity(RegisterDto dto) => new()
        {
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Address = dto.Address,
        };
    }
}
