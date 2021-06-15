using FluentPOS.Shared.DTOs.Upload;

namespace FluentPOS.Shared.Application.Interfaces.Services
{
    public interface IUploadService
    {
        string UploadAsync(UploadRequest request);
    }
}