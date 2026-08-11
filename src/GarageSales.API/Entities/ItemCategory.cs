using System;
using System.Collections.Generic;

namespace GarageSales.API.Entities;

public partial class ItemCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<FeaturedItem> FeaturedItems { get; set; } = new List<FeaturedItem>();
}
