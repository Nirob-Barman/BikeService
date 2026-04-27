using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Interfaces.Services;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Payroll;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class PayrollController : Controller
    {
        private readonly IPayrollService  _payrollService;
        private readonly IMechanicService _mechanicService;

        public PayrollController(IPayrollService payrollService, IMechanicService mechanicService)
        {
            _payrollService  = payrollService;
            _mechanicService = mechanicService;
        }

        public async Task<IActionResult> Index(int? year)
        {
            var result = await _payrollService.GetAllAsync(year);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load payroll records.";
                return View(new List<PayrollRecordDto>());
            }

            ViewBag.Year        = year;
            ViewBag.CurrentYear = DateTime.Today.Year;
            return View(result.Data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var result = await _payrollService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Payroll record not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateMechanicsAsync();
            return View(new PayrollRecordFormViewModel { Year = DateTime.Today.Year, Month = DateTime.Today.Month });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollRecordFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMechanicsAsync();
                return View(vm);
            }

            var dto = new PayrollRecordFormDto
            {
                MechanicId = vm.MechanicId,
                Month      = vm.Month,
                Year       = vm.Year,
                BaseSalary = vm.BaseSalary,
                Bonus      = vm.Bonus,
                Deductions = vm.Deductions,
                Notes      = vm.Notes,
            };

            var result = await _payrollService.CreateAsync(dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create payroll record.";
                await PopulateMechanicsAsync();
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id = result.Data });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _payrollService.GetByIdAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Payroll record not found.";
                return RedirectToAction(nameof(Index));
            }

            var dto = result.Data!;
            var vm = new PayrollRecordFormViewModel
            {
                MechanicId = dto.MechanicId,
                Month      = dto.Month,
                Year       = dto.Year,
                BaseSalary = dto.BaseSalary,
                Bonus      = dto.Bonus,
                Deductions = dto.Deductions,
                Notes      = dto.Notes,
            };

            ViewBag.Record = dto;
            await PopulateMechanicsAsync(dto.MechanicId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PayrollRecordFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var r = await _payrollService.GetByIdAsync(id);
                ViewBag.Record = r.Data;
                await PopulateMechanicsAsync(vm.MechanicId);
                return View(vm);
            }

            var dto = new PayrollRecordFormDto
            {
                MechanicId = vm.MechanicId,
                Month      = vm.Month,
                Year       = vm.Year,
                BaseSalary = vm.BaseSalary,
                Bonus      = vm.Bonus,
                Deductions = vm.Deductions,
                Notes      = vm.Notes,
            };

            var result = await _payrollService.UpdateAsync(id, dto);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update payroll record.";
                var r = await _payrollService.GetByIdAsync(id);
                ViewBag.Record = r.Data;
                await PopulateMechanicsAsync(vm.MechanicId);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(int id)
        {
            var result = await _payrollService.FinalizeAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to finalize payroll record.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var result = await _payrollService.MarkPaidAsync(id);
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to mark payroll as paid.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _payrollService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to delete payroll record.";
                return RedirectToAction(nameof(Detail), new { id });
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateMechanicsAsync(int? selectedId = null)
        {
            var result = await _mechanicService.GetAllAsync();
            ViewBag.Mechanics = new SelectList(result.Data ?? [], "Id", "FullName", selectedId);
        }
    }
}
