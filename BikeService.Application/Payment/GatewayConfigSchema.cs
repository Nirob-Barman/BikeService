namespace BikeService.Application.Payment;

public class GatewayFieldDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool IsSecret { get; init; }
    public bool IsRequired { get; init; } = true;
    public string? Placeholder { get; init; }
}

public class GatewayDefinition
{
    public string Slug { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;   // full name: "bKash Checkout"
    public string VariantLabel { get; init; } = string.Empty;  // short label for variant dropdown: "Checkout"
    public List<GatewayFieldDefinition> Fields { get; init; } = new();
}

public class GatewayFamily
{
    public string Key { get; init; } = string.Empty;           // internal key: "bkash"
    public string DisplayName { get; init; } = string.Empty;   // shown in family dropdown: "bKash"
    public List<GatewayDefinition> Variants { get; init; } = new();
    public bool HasVariants => Variants.Count > 1;
}

public static class GatewayConfigSchema
{
    private static readonly List<GatewayFieldDefinition> BkashBaseFields = new()
    {
        new() { Key = "app_key",    Label = "App Key",    IsSecret = false },
        new() { Key = "app_secret", Label = "App Secret", IsSecret = true  },
        new() { Key = "username",   Label = "Username",   IsSecret = false },
        new() { Key = "password",   Label = "Password",   IsSecret = true  },
    };

    private static readonly List<GatewayFieldDefinition> StripeBaseFields = new()
    {
        new() { Key = "secret_key",      Label = "Secret Key",      IsSecret = true,  Placeholder = "sk_live_..." },
        new() { Key = "publishable_key", Label = "Publishable Key", IsSecret = false, Placeholder = "pk_live_..." },
        new() { Key = "webhook_secret",  Label = "Webhook Secret",  IsSecret = true,  IsRequired = false, Placeholder = "whsec_..." },
    };

    private static readonly List<GatewayFieldDefinition> SslBaseFields = new()
    {
        new() { Key = "store_id",   Label = "Store ID",       IsSecret = false },
        new() { Key = "store_pass", Label = "Store Password", IsSecret = true  },
    };

    private static readonly List<GatewayFieldDefinition> SurjoBaseFields = new()
    {
        new() { Key = "store_id",   Label = "Store ID",       IsSecret = false },
        new() { Key = "store_pass", Label = "Store Password", IsSecret = true  },
    };

    public static readonly List<GatewayFamily> Families = new()
    {
        new GatewayFamily
        {
            Key = "stripe", DisplayName = "Stripe",
            Variants = new()
            {
                new GatewayDefinition
                {
                    Slug = "stripe_checkout", DisplayName = "Stripe Checkout", VariantLabel = "Checkout",
                    Fields = new(StripeBaseFields)
                },
                new GatewayDefinition
                {
                    Slug = "stripe_payment_intents", DisplayName = "Stripe Payment Intents", VariantLabel = "Payment Intents",
                    Fields = new(StripeBaseFields)
                },
            }
        },
        new GatewayFamily
        {
            Key = "sslcommerz", DisplayName = "SSLCommerz",
            Variants = new()
            {
                new GatewayDefinition
                {
                    Slug = "sslcommerz_hosted", DisplayName = "SSLCommerz Hosted", VariantLabel = "Hosted Payment",
                    Fields = new(SslBaseFields)
                },
                new GatewayDefinition
                {
                    Slug = "sslcommerz_easy", DisplayName = "SSLCommerz Easy Checkout", VariantLabel = "Easy Checkout",
                    Fields = new(SslBaseFields)
                },
            }
        },
        new GatewayFamily
        {
            Key = "bkash", DisplayName = "bKash",
            Variants = new()
            {
                new GatewayDefinition
                {
                    Slug = "bkash_checkout", DisplayName = "bKash Checkout", VariantLabel = "Checkout",
                    Fields = new(BkashBaseFields)
                },
                new GatewayDefinition
                {
                    Slug = "bkash_tokenized", DisplayName = "bKash Tokenized", VariantLabel = "Tokenized",
                    Fields = new(BkashBaseFields)
                },
                new GatewayDefinition
                {
                    Slug = "bkash_webhook", DisplayName = "bKash Webhook", VariantLabel = "Webhook",
                    Fields = new(BkashBaseFields)
                    {
                        new() { Key = "webhook_secret", Label = "Webhook Secret", IsSecret = true, IsRequired = false }
                    }
                },
            }
        },
        new GatewayFamily
        {
            Key = "surjopay", DisplayName = "SurjoPay",
            Variants = new()
            {
                new GatewayDefinition
                {
                    Slug = "surjopay_checkout", DisplayName = "SurjoPay Checkout", VariantLabel = "Checkout",
                    Fields = new(SurjoBaseFields)
                },
                new GatewayDefinition
                {
                    Slug = "surjopay_seamless", DisplayName = "SurjoPay Seamless", VariantLabel = "Seamless",
                    Fields = new(SurjoBaseFields)
                },
            }
        },
        new GatewayFamily
        {
            Key = "mock", DisplayName = "Mock Gateway (Testing)",
            Variants = new()
            {
                new GatewayDefinition
                {
                    Slug = "mock", DisplayName = "Mock Gateway (Testing)", VariantLabel = "Mock Gateway (Testing)",
                    Fields = new()
                }
            }
        },
    };

    // Flat list of all definitions — keeps existing code using GatewayConfigSchema.All working
    public static IReadOnlyList<GatewayDefinition> All =>
        Families.SelectMany(f => f.Variants).ToList();

    public static GatewayDefinition? Get(string slug) =>
        All.FirstOrDefault(g => g.Slug == slug);

    public static GatewayFamily? GetFamily(string slug) =>
        Families.FirstOrDefault(f => f.Variants.Any(v => v.Slug == slug));
}
