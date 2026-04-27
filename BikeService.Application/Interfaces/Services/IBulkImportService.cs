using BikeService.Application.DTOs.BulkImport;
using BikeService.Application.Wrappers;

namespace BikeService.Application.Interfaces.Services
{
    public interface IBulkImportService
    {
        Task<Result<BulkImportResultDto>> ImportPartsAsync(Stream csvStream, string fileName);
    }
}
