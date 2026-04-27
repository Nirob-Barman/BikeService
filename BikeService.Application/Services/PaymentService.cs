using System.Text.Json;
using BikeService.Application.DTOs.Payment;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using BikeService.Domain.Enums;

namespace BikeService.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentProcessorFactory _processorFactory;
        private readonly IPaymentGatewayService _gatewayService;
        private readonly IPromoCodeService _promoCodeService;
        private readonly IUserContextService _userContextService;
        private readonly INotificationService _notificationService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IPaymentProcessorFactory processorFactory,
            IPaymentGatewayService gatewayService,
            IPromoCodeService promoCodeService,
            IUserContextService userContextService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _processorFactory = processorFactory;
            _gatewayService = gatewayService;
            _promoCodeService = promoCodeService;
            _userContextService = userContextService;
            _notificationService = notificationService;
        }

        public async Task<Result<CheckoutInfoDto>> GetCheckoutInfoAsync(int invoiceId, string? promoCode)
        {
            var userId = _userContextService.UserId;

            var invoices = await _unitOfWork.Repository<Invoice>()
                .GetAllWithIncludesAsync<Invoice>(
                    i => i.Id == invoiceId,
                    i => i,
                    i => i.ServiceTicket,
                    i => i.PromoCode);

            var invoice = invoices.FirstOrDefault();
            if (invoice == null)
                return Result<CheckoutInfoDto>.Fail("Invoice not found.");

            if (invoice.ServiceTicket?.Bike == null)
                invoice.ServiceTicket!.Bike = await _unitOfWork.Repository<CustomerBike>()
                    .GetByIdAsync(invoice.ServiceTicket.BikeId);

            if (invoice.ServiceTicket?.Bike?.CustomerId != userId)
                return Result<CheckoutInfoDto>.Fail("Access denied.");

            if (invoice.Status == InvoiceStatus.Paid)
                return Result<CheckoutInfoDto>.Fail("Invoice is already paid.");
            if (invoice.Status == InvoiceStatus.Void)
                return Result<CheckoutInfoDto>.Fail("Invoice is void.");

            var gatewaysResult = await _gatewayService.GetAllAsync();
            var gateways = (gatewaysResult.Data ?? new()).Where(g => g.IsActive).ToList();

            decimal discountAmount = 0;
            decimal discountPercent = 0;
            string? appliedCode = null;

            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                var promoResult = await _promoCodeService.ValidateCodeAsync(promoCode);
                if (promoResult.Success && promoResult.Data != null)
                {
                    discountPercent = promoResult.Data.DiscountPercent;
                    discountAmount = Math.Round(invoice.TotalAmount * (discountPercent / 100m), 2);
                    appliedCode = promoResult.Data.Code;
                }
            }

            var bike = invoice.ServiceTicket?.Bike;
            var bikeSummary = bike == null ? "Bike Service" : $"{bike.Year} {bike.Make} {bike.Model}";
            var finalAmount = Math.Max(0, invoice.TotalAmount + invoice.TaxAmount - discountAmount);

            return Result<CheckoutInfoDto>.Ok(new CheckoutInfoDto
            {
                InvoiceId = invoice.Id,
                BikeSummary = bikeSummary,
                TotalAmount = invoice.TotalAmount,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                AppliedPromoCode = appliedCode,
                PromoDiscountPercent = discountPercent,
                Gateways = gateways
            });
        }

        public async Task<Result<string>> InitiateAsync(int invoiceId, int gatewayId, string? promoCode)
        {
            var userId = _userContextService.UserId;

            var invoices = await _unitOfWork.Repository<Invoice>()
                .GetAllWithIncludesAsync<Invoice>(
                    i => i.Id == invoiceId,
                    i => i,
                    i => i.ServiceTicket);

            var invoice = invoices.FirstOrDefault();
            if (invoice == null)
                return Result<string>.Fail("Invoice not found.");

            if (invoice.ServiceTicket?.Bike == null)
                invoice.ServiceTicket!.Bike = await _unitOfWork.Repository<CustomerBike>()
                    .GetByIdAsync(invoice.ServiceTicket.BikeId);

            if (invoice.ServiceTicket?.Bike?.CustomerId != userId)
                return Result<string>.Fail("Access denied.");

            if (invoice.Status == InvoiceStatus.Paid)
                return Result<string>.Fail("Invoice is already paid.");
            if (invoice.Status != InvoiceStatus.Issued)
                return Result<string>.Fail("Invoice must be in Issued status before payment.");

            var gateway = await _unitOfWork.Repository<PaymentGateway>().GetByIdAsync(gatewayId);
            if (gateway == null || !gateway.IsActive)
                return Result<string>.Fail("Payment gateway not available.");

            var processor = _processorFactory.GetProcessor(gateway.Slug);
            if (processor == null)
                return Result<string>.Fail($"Payment processor '{gateway.Slug}' is not configured.");

            // Apply promo code if provided
            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                var promoResult = await _promoCodeService.ValidateCodeAsync(promoCode);
                if (promoResult.Success && promoResult.Data != null)
                {
                    var discountAmt = Math.Round(invoice.TotalAmount * (promoResult.Data.DiscountPercent / 100m), 2);
                    invoice.DiscountAmount = discountAmt;
                    invoice.PromoCodeId = promoResult.Data.Id;
                    invoice.FinalAmount = Math.Max(0, invoice.TotalAmount + invoice.TaxAmount - discountAmt);
                    invoice.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<Invoice>().Update(invoice);
                }
            }

            // Create pending transaction
            var tx = new PaymentTransaction
            {
                InvoiceId = invoice.Id,
                GatewayId = gatewayId,
                Amount = invoice.FinalAmount,
                Status = PaymentTransactionStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            await _unitOfWork.Repository<PaymentTransaction>().AddAsync(tx);
            await _unitOfWork.SaveChangesAsync();

            // Get decrypted config
            var configResult = await _gatewayService.GetDecryptedConfigAsync(gatewayId);
            if (!configResult.Success)
                return Result<string>.Fail("Failed to load gateway configuration.");

            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data!)
                         ?? new Dictionary<string, string>();

            var baseUrl = _userContextService.GetBaseUrl();
            var successUrl = $"{baseUrl}/Payment/Success?txId={tx.Id}&gateway={gateway.Slug}";
            var cancelUrl = $"{baseUrl}/Payment/Cancel?txId={tx.Id}";

            var initResult = await processor.InitiateAsync(config, invoice.FinalAmount, tx.Id, successUrl, cancelUrl);
            if (!initResult.Success)
            {
                tx.Status = PaymentTransactionStatus.Failed;
                _unitOfWork.Repository<PaymentTransaction>().Update(tx);
                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Fail(initResult.Error ?? "Payment initiation failed.");
            }

            return Result<string>.Ok(initResult.RedirectUrl!);
        }

        public async Task<Result<bool>> HandleSuccessAsync(int txId, Dictionary<string, string> callbackParams)
        {
            var transactions = await _unitOfWork.Repository<PaymentTransaction>()
                .GetAllWithIncludesAsync<PaymentTransaction>(
                    t => t.Id == txId,
                    t => t,
                    t => t.Invoice,
                    t => t.Gateway);

            var tx = transactions.FirstOrDefault();
            if (tx == null)
                return Result<bool>.Fail("Transaction not found.");

            // Idempotent — already processed
            if (tx.Status == PaymentTransactionStatus.Success)
                return Result<bool>.Ok(true, "Payment already processed.");

            var processor = _processorFactory.GetProcessor(tx.Gateway.Slug);
            if (processor == null)
                return Result<bool>.Fail("Processor not found.");

            var configResult = await _gatewayService.GetDecryptedConfigAsync(tx.GatewayId);
            if (!configResult.Success)
                return Result<bool>.Fail("Failed to load gateway configuration.");

            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configResult.Data!)
                         ?? new Dictionary<string, string>();

            var verified = await processor.VerifyAsync(config, callbackParams);
            if (!verified)
            {
                tx.Status = PaymentTransactionStatus.Failed;
                tx.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<PaymentTransaction>().Update(tx);
                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Fail("Payment verification failed.");
            }

            tx.Status = PaymentTransactionStatus.Success;
            tx.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<PaymentTransaction>().Update(tx);

            var invoice = await _unitOfWork.Repository<Invoice>().GetByIdAsync(tx.InvoiceId);
            if (invoice != null)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Invoice>().Update(invoice);

                if (invoice.PromoCodeId.HasValue)
                {
                    var promo = await _unitOfWork.Repository<PromoCode>().GetByIdAsync(invoice.PromoCodeId.Value);
                    if (promo != null)
                    {
                        promo.UsageCount++;
                        promo.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Repository<PromoCode>().Update(promo);
                    }
                }

                var ticket = await _unitOfWork.Repository<ServiceTicket>().GetByIdAsync(invoice.ServiceTicketId);
                if (ticket != null && ticket.Status == ServiceTicketStatus.ReadyForPickup)
                {
                    ticket.Status = ServiceTicketStatus.Delivered;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Repository<ServiceTicket>().Update(ticket);

                    var bike = await _unitOfWork.Repository<CustomerBike>().GetByIdAsync(ticket.BikeId);
                    if (bike != null)
                    {
                        await _notificationService.CreateNotificationAsync(
                            bike.CustomerId,
                            "Payment Confirmed",
                            "Your payment has been confirmed. Thank you for using BikeService!",
                            $"/Invoice/Detail/{invoice.Id}");
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return Result<bool>.Ok(true, "Payment processed successfully.");
        }

        public async Task<Result<bool>> HandleCancelAsync(int txId)
        {
            var tx = await _unitOfWork.Repository<PaymentTransaction>().GetByIdAsync(txId);
            if (tx == null)
                return Result<bool>.Fail("Transaction not found.");

            if (tx.Status == PaymentTransactionStatus.Pending)
            {
                tx.Status = PaymentTransactionStatus.Failed;
                tx.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<PaymentTransaction>().Update(tx);
                await _unitOfWork.SaveChangesAsync();
            }

            return Result<bool>.Ok(true);
        }
    }
}
