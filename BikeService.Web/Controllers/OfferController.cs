using BikeService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeService.Web.Controllers
{
    public class OfferController : Controller
    {
        private readonly IPromoCodeService _promoCodeService;

        public OfferController(IPromoCodeService promoCodeService)
        {
            _promoCodeService = promoCodeService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _promoCodeService.GetActiveAsync();
            return View(result.Data ?? new());
        }
    }
}
