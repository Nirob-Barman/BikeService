using BikeService.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace BikeService.Infrastructure.Services;

public class ConfigEncryptor : IConfigEncryptor
{
    private readonly IDataProtector _protector;

    public ConfigEncryptor(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("BikeService.PaymentGateway.Config");
    }

    public string Encrypt(string plainText) => _protector.Protect(plainText);
    public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
}
