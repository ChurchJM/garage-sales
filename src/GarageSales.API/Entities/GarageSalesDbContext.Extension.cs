using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Modes;

namespace GarageSales.API.Entities;

public partial class GarageSalesDbContext : DbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GarageSale>()
            .Navigation(gs => gs.GarageSaleSchedules)
            .AutoInclude();
        
        modelBuilder.Entity<GarageSale>()
            .Navigation(gs => gs.SaleType)
            .AutoInclude();

        modelBuilder.Entity<GarageSale>()
            .Navigation(gs => gs.Address)
            .AutoInclude();

        modelBuilder.Entity<GarageSale>()
            .Navigation(gs => gs.FeaturedItems)
            .AutoInclude();

        modelBuilder.Entity<User>()
            .Navigation(u => u.Address)
            .AutoInclude();

        modelBuilder.Entity<FeaturedItem>()
            .Navigation(fi => fi.Category)
            .AutoInclude();
    }
}