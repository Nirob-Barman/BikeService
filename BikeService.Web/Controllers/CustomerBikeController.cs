using BikeService.Application.DTOs.CustomerBike;
using BikeService.Application.Interfaces.FileStorage;
using BikeService.Application.Interfaces.Services;
using BikeService.Web.ViewModels.CustomerBike;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerBikeController : Controller
    {
        private readonly ICustomerBikeService _bikeService;
        private readonly IFileStorage _fileStorage;

        public CustomerBikeController(ICustomerBikeService bikeService, IFileStorage fileStorage)
        {
            _bikeService = bikeService;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _bikeService.GetMyBikesAsync();
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load bikes.";
                return View(new List<CustomerBikeDto>());
            }
            return View(result.Data);
        }

        [HttpGet]
        public IActionResult Create() => View(new CustomerBikeFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerBikeFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var imageUrl = await UploadImageAsync(vm.Image);

            var dto = new CustomerBikeFormDto
            {
                Make = vm.Make,
                Model = vm.Model,
                Year = vm.Year,
                RegistrationNo = vm.RegistrationNo,
                ImageUrl = imageUrl
            };

            var result = await _bikeService.CreateAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Failed to register bike.");
                return View(vm);
            }

            TempData["Success"] = "Bike registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _bikeService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = "Bike not found.";
                return RedirectToAction(nameof(Index));
            }

            var bike = result.Data!;
            var vm = new CustomerBikeFormViewModel
            {
                Make = bike.Make,
                Model = bike.Model,
                Year = bike.Year,
                RegistrationNo = bike.RegistrationNo,
                ExistingImageUrl = bike.ImageUrl
            };

            ViewBag.BikeId = id;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerBikeFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BikeId = id;
                return View(vm);
            }

            var imageUrl = vm.Image != null
                ? await UploadImageAsync(vm.Image)
                : vm.ExistingImageUrl;

            var dto = new CustomerBikeFormDto
            {
                Make = vm.Make,
                Model = vm.Model,
                Year = vm.Year,
                RegistrationNo = vm.RegistrationNo,
                ImageUrl = imageUrl
            };

            var result = await _bikeService.UpdateAsync(id, dto);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Errors?.FirstOrDefault() ?? "Failed to update bike.");
                ViewBag.BikeId = id;
                return View(vm);
            }

            TempData["Success"] = "Bike updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bikeService.DeleteAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete bike.";
            else
                TempData["Success"] = "Bike deleted.";

            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> UploadImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            using var stream = file.OpenReadStream();
            return await _fileStorage.UploadFileAsync(stream, fileName, "bikes");
        }
    }
}
