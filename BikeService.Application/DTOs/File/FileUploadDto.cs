namespace BikeService.Application.DTOs.File
{
    public class FileUploadDto
    {
        public Stream? Content { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long Size { get; set; }
    }
}
