using System;
using System.Collections.Generic;

namespace GarageSales.API.Entities;

public partial class FeaturedItem
{
    public int Id { get; set; }

    public int GarageSaleId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public virtual ItemCategory Category { get; set; } = null!;

    public virtual GarageSale GarageSale { get; set; } = null!;
}
