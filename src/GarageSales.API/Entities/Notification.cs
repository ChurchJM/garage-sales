using System;
using System.Collections.Generic;

namespace GarageSales.API.Entities;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public double MaxRadius { get; set; }

    public virtual User User { get; set; } = null!;
}
