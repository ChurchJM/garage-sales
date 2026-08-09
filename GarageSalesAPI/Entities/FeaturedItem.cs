using System;
using System.Collections.Generic;

namespace GarageSalesAPI.Entities;

public partial class FeaturedItem
{
    public int Id { get; set; }

    public int GarageSaleId { get; set; }

    public int CategoryId { get; set; }

    public string Description { get; set; } = null!;

    public virtual ItemCategory Category { get; set; } = null!;

    public virtual GarageSale GarageSale { get; set; } = null!;
}
