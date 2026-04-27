using System.Text.Json;
using BikeService.Application.DTOs.PaymentGateway;
using BikeService.Web.ViewModels.PaymentGateway;

namespace BikeService.Web.ViewModels.Mappers;

public static class PaymentGatewayViewModelMapper
{
    /// <summary>
    /// Reads the flat VM fields for the selected slug and builds the JSON config string.
    /// Blank/null values are omitted so the service merge logic keeps existing secrets.
    /// </summary>
    public static string BuildConfig(PaymentGatewayFormViewModel vm)
    {
        var dict = vm.Slug switch
        {
            "stripe_checkout" or "stripe_payment_intents" => new Dictionary<string, string?>
            {
                ["secret_key"]      = vm.Stripe_SecretKey,
                ["publishable_key"] = vm.Stripe_PublishableKey,
                ["webhook_secret"]  = vm.Stripe_WebhookSecret,
            },
            "sslcommerz_hosted" or "sslcommerz_easy" => new Dictionary<string, string?>
            {
                ["store_id"]   = vm.Ssl_StoreId,
                ["store_pass"] = vm.Ssl_StorePassword,
            },
            "bkash_checkout" or "bkash_tokenized" => new Dictionary<string, string?>
            {
                ["app_key"]    = vm.Bkash_AppKey,
                ["app_secret"] = vm.Bkash_AppSecret,
                ["username"]   = vm.Bkash_Username,
                ["password"]   = vm.Bkash_Password,
            },
            "bkash_webhook" => new Dictionary<string, string?>
            {
                ["app_key"]        = vm.Bkash_AppKey,
                ["app_secret"]     = vm.Bkash_AppSecret,
                ["username"]       = vm.Bkash_Username,
                ["password"]       = vm.Bkash_Password,
                ["webhook_secret"] = vm.Bkash_WebhookSecret,
            },
            "surjopay_checkout" or "surjopay_seamless" => new Dictionary<string, string?>
            {
                ["store_id"]   = vm.Surjo_StoreId,
                ["store_pass"] = vm.Surjo_StorePassword,
            },
            _ => new Dictionary<string, string?>()
        };

        // Drop blank entries — service will merge missing keys with existing encrypted values
        var clean = dict
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .ToDictionary(kv => kv.Key, kv => kv.Value!);

        return JsonSerializer.Serialize(clean);
    }

    /// <summary>
    /// Populates the flat ViewModel fields from a decrypted JSON config string.
    /// Used on the Edit GET to pre-fill non-secret fields.
    /// Secret fields are intentionally left blank (show placeholder "••••••••").
    /// </summary>
    public static void PopulateFields(PaymentGatewayFormViewModel vm, string decryptedJson)
    {
        if (string.IsNullOrWhiteSpace(decryptedJson)) return;

        Dictionary<string, string>? config = null;
        try { config = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedJson); }
        catch { return; }

        if (config is null) return;

        string? Get(string key) => config.TryGetValue(key, out var v) ? v : null;

        switch (vm.Slug)
        {
            case "stripe_checkout":
            case "stripe_payment_intents":
                vm.Stripe_PublishableKey = Get("publishable_key");
                break;

            case "sslcommerz_hosted":
            case "sslcommerz_easy":
                vm.Ssl_StoreId = Get("store_id");
                break;

            case "bkash_checkout":
            case "bkash_tokenized":
            case "bkash_webhook":
                vm.Bkash_AppKey  = Get("app_key");
                vm.Bkash_Username = Get("username");
                break;

            case "surjopay_checkout":
            case "surjopay_seamless":
                vm.Surjo_StoreId = Get("store_id");
                break;
        }
    }

    public static PaymentGatewayFormDto ToDto(PaymentGatewayFormViewModel vm) =>
        new()
        {
            Slug     = vm.Slug,
            Name     = vm.Name,
            Config   = BuildConfig(vm),
            IsActive = vm.IsActive,
            IsSandbox = vm.IsSandbox,
        };
}
