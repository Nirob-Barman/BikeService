using BikeService.Application.DTOs.Payroll;
using BikeService.Application.Features.Payroll.Commands.CreatePayrollRecord;
using BikeService.Application.Features.Payroll.Commands.DeletePayrollRecord;
using BikeService.Application.Features.Payroll.Commands.FinalizePayrollRecord;
using BikeService.Application.Features.Payroll.Commands.MarkPayrollRecordPaid;
using BikeService.Application.Features.Payroll.Commands.UpdatePayrollRecord;
using BikeService.Application.Features.Payroll.Queries.GetPayrollRecordById;
using BikeService.Application.Features.Payroll.Queries.GetPayrollRecords;
using BikeService.Application.Features.Mechanics.Queries.GetMechanics;
using BikeService.Domain.Constants;
using BikeService.Web.ViewModels.Payroll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class PayrollController : Controller
    {
        private readonly IMediator _mediator;

        public PayrollController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? year)
        {
            var result = await _mediator.Send(new GetPayrollRecordsQuery(year));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load payroll records.";
                return View(new List<PayrollRecordDto>());
            }

            ViewBag.Year        = year;
            ViewBag.CurrentYear = DateTime.Today.Year;
            return View(result.Data);
        }

        [HttpGet("Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var result = await _mediator.Send(new GetPayrollRecordByIdQuery(id));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Payroll record not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateMechanicsAsync();
            return View(new PayrollRecordFormViewModel { Year = DateTime.Today.Year, Month = DateTime.Today.Month });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PayrollRecordFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateMechanicsAsync();
                return View(vm);
            }

            var result = await _mediator.Send(new CreatePayrollRecordCommand(
                vm.MechanicId, vm.Month, vm.Year, vm.BaseSalary, vm.Bonus, vm.Deductions, vm.Notes));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to create payroll record.";
                await PopulateMechanicsAsync();
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id = result.Data });
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _mediator.Send(new GetPayrollRecordByIdQuery(id));
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

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PayrollRecordFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var r = await _mediator.Send(new GetPayrollRecordByIdQuery(id));
                ViewBag.Record = r.Data;
                await PopulateMechanicsAsync(vm.MechanicId);
                return View(vm);
            }

            var result = await _mediator.Send(new UpdatePayrollRecordCommand(
                id, vm.MechanicId, vm.Month, vm.Year, vm.BaseSalary, vm.Bonus, vm.Deductions, vm.Notes));
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to update payroll record.";
                var r = await _mediator.Send(new GetPayrollRecordByIdQuery(id));
                ViewBag.Record = r.Data;
                await PopulateMechanicsAsync(vm.MechanicId);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("Finalize/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(int id)
        {
            var result = await _mediator.Send(new FinalizePayrollRecordCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to finalize payroll record.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("MarkPaid/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var result = await _mediator.Send(new MarkPayrollRecordPaidCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to mark payroll as paid.";
            else
                TempData["Success"] = result.Message;

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeletePayrollRecordCommand(id));
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
            var result = await _mediator.Send(new GetMechanicsQuery());
            ViewBag.Mechanics = new SelectList(result.Data ?? [], "Id", "FullName", selectedId);
        }
    }
}
