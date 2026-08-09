using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GarageSalesAPI.Entities;

public partial class Address
{
    public int Id { get; set; }

    public string Street { get; set; } = null!;

    public string City { get; set; } = null!;

    public string State { get; set; } = null!;

    public string Zip { get; set; } = null!;

    public double? Lat { get; set; }

    public double? Lon { get; set; }

    public Geometry? Location { get; set; }

    public virtual ICollection<GarageSale> GarageSales { get; set; } = new List<GarageSale>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
