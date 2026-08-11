using System;
using System.Collections.Generic;

namespace GarageSales.API.Entities;

public partial class User
{
    public int Id { get; set; }

    public int AddressId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<GarageSale> GarageSales { get; set; } = new List<GarageSale>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
