using System;
using System.Collections.Generic;

namespace GarageSales.API.Entities;

public partial class GarageSale
{
    public int Id { get; set; }

    public int SaleTypeId { get; set; }

    public int OwnerId { get; set; }

    public int AddressId { get; set; }

    public string? Description { get; set; }

    public bool Draft { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<FeaturedItem> FeaturedItems { get; set; } = new List<FeaturedItem>();

    public virtual ICollection<GarageSaleSchedule> GarageSaleSchedules { get; set; } = new List<GarageSaleSchedule>();

    public virtual User Owner { get; set; } = null!;

    public virtual GarageSaleType SaleType { get; set; } = null!;
}
