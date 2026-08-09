using System;
using System.Collections.Generic;

namespace GarageSalesAPI.Entities;

public partial class GarageSaleSchedule
{
    public int Id { get; set; }

    public int GarageSaleId { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public virtual GarageSale GarageSale { get; set; } = null!;
}
