using BikeService.Application.DTOs.LeaveRequest;
using BikeService.Web.ViewModels.LeaveRequest;

namespace BikeService.Web.ViewModels.Mappers
{
    public static class LeaveRequestViewModelMapper
    {
        public static LeaveRequestFormDto ToDto(LeaveRequestFormViewModel vm) => new()
        {
            FromDate = vm.FromDate,
            ToDate   = vm.ToDate,
            Type     = vm.Type,
            Reason   = vm.Reason,
        };
    }
}
