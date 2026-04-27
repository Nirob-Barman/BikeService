using System.ComponentModel.DataAnnotations;

namespace BikeService.Web.ViewModels.PaymentGateway;

public class PaymentGatewayFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a gateway.")]
    public string Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsSandbox { get; set; } = true;

    // ── Stripe ───────────────────────────────────────────────────────────────
    public string? Stripe_SecretKey { get; set; }
    public string? Stripe_PublishableKey { get; set; }
    public string? Stripe_WebhookSecret { get; set; }

    // ── SSLCommerz ───────────────────────────────────────────────────────────
    public string? Ssl_StoreId { get; set; }
    public string? Ssl_StorePassword { get; set; }

    // ── BKash (all variants share same credentials; webhook adds secret) ──────
    public string? Bkash_AppKey { get; set; }
    public string? Bkash_AppSecret { get; set; }
    public string? Bkash_Username { get; set; }
    public string? Bkash_Password { get; set; }
    public string? Bkash_WebhookSecret { get; set; }

    // ── SurjoPay ─────────────────────────────────────────────────────────────
    public string? Surjo_StoreId { get; set; }
    public string? Surjo_StorePassword { get; set; }
}
