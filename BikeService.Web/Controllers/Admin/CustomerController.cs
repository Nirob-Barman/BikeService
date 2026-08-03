using BikeService.Application.Features.Customers.Commands.BanCustomer;
using BikeService.Application.Features.Customers.Commands.UnbanCustomer;
using BikeService.Application.Features.Customers.Queries.GetCustomers;
using BikeService.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers.Admin
{
    [Authorize(Roles = AppRoles.Admin)]
    [Route("Admin/[controller]")]
    public class CustomerController : Controller
    {
        private readonly IMediator _mediator;

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetCustomersQuery());
            if (!result.Success)
            {
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to load customers.";
                return View(new List<BikeService.Application.DTOs.Customer.CustomerDto>());
            }
            return View(result.Data);
        }

        [HttpPost("Ban/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(string id)
        {
            var result = await _mediator.Send(new BanCustomerCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to ban customer.";
            else
                TempData["Success"] = "Customer has been banned.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Unban/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(string id)
        {
            var result = await _mediator.Send(new UnbanCustomerCommand(id));
            if (!result.Success)
                TempData["Error"] = result.Errors?.FirstOrDefault() ?? "Failed to unban customer.";
            else
                TempData["Success"] = "Customer has been unbanned.";

            return RedirectToAction(nameof(Index));
        }
    }
}
