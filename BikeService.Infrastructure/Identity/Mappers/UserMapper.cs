

using BikeService.Application.DTOs.Identity;

namespace BikeService.Infrastructure.Identity.Mappers
{
    public static class UserMapper
    {
        public static ApplicationUser ToEntity(ApplicationUserDto userDto)
        {
            return new ApplicationUser
            {
                Id = userDto.Id!,
                Email = userDto.Email,
                Address = userDto.Address
            };
        }
        public static ApplicationUserDto ToDto(ApplicationUser user)
        {
            return new ApplicationUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Address = user.Address
            };
        }
    }
}
