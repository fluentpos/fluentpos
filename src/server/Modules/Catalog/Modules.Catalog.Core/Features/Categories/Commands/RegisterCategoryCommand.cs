using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Upload;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Categories.Commands
{
    public partial class RegisterCategoryCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Detail { get; set; }
        public UploadRequest UploadRequest { get; set; }
    }
}