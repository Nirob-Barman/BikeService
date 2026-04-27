using BikeService.Application.DTOs.Mechanic;
using BikeService.Web.ViewModels.Mechanic;

namespace BikeService.Web.ViewModels.Mappers
{
    public static class MechanicViewModelMapper
    {
        public static MechanicFormViewModel ToViewModel(MechanicDto dto) => new()
        {
            Id            = dto.Id,
            FullName      = dto.FullName,
            Specialty     = dto.Specialty,
            IsAvailable   = dto.IsAvailable,
            LinkedEmail   = dto.LinkedEmail,
            IsLoginActive = dto.IsLoginActive,
        };

        public static MechanicFormDto ToDto(MechanicFormViewModel vm) => new()
        {
            FullName    = vm.FullName,
            Specialty   = vm.Specialty,
            IsAvailable = vm.IsAvailable,
            Email       = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
            Password    = string.IsNullOrWhiteSpace(vm.Password) ? null : vm.Password,
        };
    }
}
