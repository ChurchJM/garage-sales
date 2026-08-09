using System;
using System.Collections.Generic;

namespace GarageSalesAPI.Entities;

public partial class GarageSaleType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<GarageSale> GarageSales { get; set; } = new List<GarageSale>();
}
