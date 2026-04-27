using System.Text.Json;
using BikeService.Application.DTOs.BulkImport;
using BikeService.Application.Interfaces;
using BikeService.Application.Interfaces.Persistence;
using BikeService.Application.Interfaces.Services;
using BikeService.Application.Wrappers;
using BikeService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BikeService.Application.Services
{
    public class BulkImportService : IBulkImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<BulkImportService> _logger;

        public BulkImportService(
            IUnitOfWork unitOfWork,
            IAuditLogService auditLogService,
            IUserContextService userContextService,
            ILogger<BulkImportService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
            _userContextService = userContextService;
            _logger = logger;
        }

        public async Task<Result<BulkImportResultDto>> ImportPartsAsync(Stream csvStream, string fileName)
        {
            var result = new BulkImportResultDto();
            var partsToAdd = new List<Part>();

            // Load all existing SKUs once for duplicate checking
            var existingSkus = (await _unitOfWork.Repository<Part>().GetAllAsync())
                .Select(p => p.SKU.ToLowerInvariant())
                .ToHashSet();

            using var reader = new StreamReader(csvStream);

            var headerLine = await reader.ReadLineAsync();
            if (headerLine is null)
                return Result<BulkImportResultDto>.Fail("CSV file is empty.");

            // Validate header
            var headers = headerLine.Split(',').Select(h => h.Trim().ToLowerInvariant()).ToArray();
            var expectedHeaders = new[] { "name", "sku", "unitprice", "stockquantity", "lowstockthreshold" };
            if (!expectedHeaders.All(h => headers.Contains(h)))
                return Result<BulkImportResultDto>.Fail($"Invalid CSV header. Expected columns: {string.Join(", ", expectedHeaders)}.");

            var nameIdx = Array.IndexOf(headers, "name");
            var skuIdx = Array.IndexOf(headers, "sku");
            var priceIdx = Array.IndexOf(headers, "unitprice");
            var stockIdx = Array.IndexOf(headers, "stockquantity");
            var thresholdIdx = Array.IndexOf(headers, "lowstockthreshold");

            int rowNumber = 1;
            string? line;

            while ((line = await reader.ReadLineAsync()) is not null)
            {
                rowNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                result.TotalRows++;

                var columns = line.Split(',');

                if (columns.Length < 5)
                {
                    result.Errors.Add($"Row {rowNumber}: Insufficient columns (expected 5, got {columns.Length}).");
                    result.FailedCount++;
                    continue;
                }

                var name = columns[nameIdx].Trim();
                var sku = columns[skuIdx].Trim();
                var unitPriceRaw = columns[priceIdx].Trim();
                var stockQtyRaw = columns[stockIdx].Trim();
                var thresholdRaw = columns[thresholdIdx].Trim();

                var rowErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(name))
                    rowErrors.Add("Name is required.");

                if (string.IsNullOrWhiteSpace(sku))
                    rowErrors.Add("SKU is required.");

                if (!decimal.TryParse(unitPriceRaw, out var unitPrice) || unitPrice <= 0)
                    rowErrors.Add($"Invalid UnitPrice '{unitPriceRaw}': must be a positive number.");

                if (!int.TryParse(stockQtyRaw, out var stockQty) || stockQty < 0)
                    rowErrors.Add($"Invalid StockQuantity '{stockQtyRaw}': must be a non-negative integer.");

                if (!int.TryParse(thresholdRaw, out var threshold) || threshold < 0)
                    rowErrors.Add($"Invalid LowStockThreshold '{thresholdRaw}': must be a non-negative integer.");

                if (rowErrors.Count > 0)
                {
                    result.Errors.Add($"Row {rowNumber}: {string.Join(" ", rowErrors)}");
                    result.FailedCount++;
                    continue;
                }

                // Check for duplicate SKU against DB and within the current import batch
                var skuLower = sku.ToLowerInvariant();
                if (existingSkus.Contains(skuLower))
                {
                    result.Errors.Add($"Row {rowNumber}: SKU '{sku}' already exists.");
                    result.FailedCount++;
                    continue;
                }

                // Track within-batch duplicates
                existingSkus.Add(skuLower);

                partsToAdd.Add(new Part
                {
                    Name = name,
                    SKU = sku,
                    UnitPrice = unitPrice,
                    StockQuantity = stockQty,
                    LowStockThreshold = threshold,
                    CreatedBy = _userContextService.UserId
                });

                result.SuccessCount++;
            }

            if (partsToAdd.Count > 0)
            {
                await _unitOfWork.Repository<Part>().AddRangeAsync(partsToAdd);
                await _unitOfWork.SaveChangesAsync();

                await _auditLogService.LogAsync(
                    "Part", "BulkImport",
                    _userContextService.UserId, _userContextService.Email,
                    $"Imported {result.SuccessCount} parts from file '{fileName}'",
                    ipAddress: _userContextService.IpAddress,
                    userAgent: _userContextService.UserAgent,
                    newValues: JsonSerializer.Serialize(new { result.SuccessCount, result.FailedCount, FileName = fileName }));
            }

            return Result<BulkImportResultDto>.Ok(result, $"Import complete. {result.SuccessCount} parts imported, {result.FailedCount} rows failed.");
        }
    }
}
