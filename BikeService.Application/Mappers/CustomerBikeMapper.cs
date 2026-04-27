using BikeService.Application.DTOs.CustomerBike;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class CustomerBikeMapper
    {
        public static CustomerBikeDto ToDto(CustomerBike bike) => new()
        {
            Id = bike.Id,
            Make = bike.Make,
            Model = bike.Model,
            Year = bike.Year,
            RegistrationNo = bike.RegistrationNo,
            ImageUrl = bike.ImageUrl,
            CustomerId = bike.CustomerId,
            CreatedAt = bike.CreatedAt
        };

        public static CustomerBike ToEntity(CustomerBikeFormDto dto) => new()
        {
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            RegistrationNo = dto.RegistrationNo,
            ImageUrl = dto.ImageUrl
        };

        public static void UpdateEntity(CustomerBike bike, CustomerBikeFormDto dto)
        {
            bike.Make = dto.Make;
            bike.Model = dto.Model;
            bike.Year = dto.Year;
            bike.RegistrationNo = dto.RegistrationNo;
            bike.ImageUrl = dto.ImageUrl;
        }
    }
}
