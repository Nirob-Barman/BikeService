using BikeService.Application.DTOs.Mechanic;
using BikeService.Domain.Entities;

namespace BikeService.Application.Mappers
{
    public static class MechanicMapper
    {
        public static MechanicDto ToDto(Mechanic e) => new()
        {
            Id = e.Id,
            FullName = e.FullName,
            Specialty = e.Specialty,
            IsAvailable = e.IsAvailable,
            UserId = e.UserId,
            CreatedAt = e.CreatedAt
        };

        public static Mechanic ToEntity(MechanicFormDto dto) => new()
        {
            FullName = dto.FullName,
            Specialty = dto.Specialty,
            IsAvailable = dto.IsAvailable,
        };

        public static void UpdateEntity(Mechanic e, MechanicFormDto dto)
        {
            e.FullName = dto.FullName;
            e.Specialty = dto.Specialty;
            e.IsAvailable = dto.IsAvailable;
            // UserId is not editable after creation — never overwrite
        }
    }
}
