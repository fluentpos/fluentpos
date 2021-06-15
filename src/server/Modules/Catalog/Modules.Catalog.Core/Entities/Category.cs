using FluentPOS.Shared.Application.Domain;

namespace FluentPOS.Modules.Catalog.Core.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Detail { get; set; }
    }
}