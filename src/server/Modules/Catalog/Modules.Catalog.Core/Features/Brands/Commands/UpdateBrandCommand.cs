﻿using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Upload;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Brands.Commands
{
    public class UpdateBrandCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Detail { get; set; }
        public UploadRequest UploadRequest { get; set; }
    }
}