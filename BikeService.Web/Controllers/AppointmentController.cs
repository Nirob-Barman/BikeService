using BikeService.Application.DTOs.Appointment;
using BikeService.Application.Interfaces.Services;
using BikeService.Web.ViewModels.Appointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICustomerBikeService _bikeService;

        public AppointmentController(IAppointmentService appointmentService, ICustomerBikeService bikeService)
        {
            _appointmentService = appointmentService;
            _bikeService = bikeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _appointmentService.GetMyAppointmentsAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load appointments.";
                return View(new List<AppointmentDto>());
            }
            return View(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var bikesResult = await _bikeService.GetMyBikesAsync();
            var bikes = bikesResult.Data ?? new();

            if (!bikes.Any())
            {
                TempData["Error"] = "You need to register a bike before booking an appointment.";
                return RedirectToAction("Create", "CustomerBike");
            }

            return View(new AppointmentCreateViewModel { Bikes = bikes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var bikesResult = await _bikeService.GetMyBikesAsync();
                vm.Bikes = bikesResult.Data ?? new();
                return View(vm);
            }

            var dto = new AppointmentFormDto
            {
                BikeId = vm.BikeId,
                AppointmentDate = vm.AppointmentDate,
                Notes = vm.Notes
            };

            var result = await _appointmentService.CreateAsync(dto);
            if (!result.Success)
            {
                foreach (var err in result.Errors ?? new())
                    ModelState.AddModelError(string.Empty, err);

                if (result.FieldErrors != null)
                    foreach (var fe in result.FieldErrors)
                        ModelState.AddModelError(fe.Key, fe.Value);

                var bikesResult = await _bikeService.GetMyBikesAsync();
                vm.Bikes = bikesResult.Data ?? new();
                return View(vm);
            }

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _appointmentService.CancelAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to cancel appointment.";
            else
                TempData["Success"] = "Appointment cancelled.";

            return RedirectToAction(nameof(Index));
        }
    }
}
