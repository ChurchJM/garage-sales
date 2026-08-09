using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GarageSalesAPI.Entities;

public partial class GarageSalesDbContext : DbContext
{
    public GarageSalesDbContext()
    {
    }

    public GarageSalesDbContext(DbContextOptions<GarageSalesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<FeaturedItem> FeaturedItems { get; set; }

    public virtual DbSet<GarageSale> GarageSales { get; set; }

    public virtual DbSet<GarageSaleSchedule> GarageSaleSchedules { get; set; }

    public virtual DbSet<GarageSaleType> GarageSaleTypes { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Secret> Secrets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("<YOUR CONNECTION STRING>", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Street).HasMaxLength(200);
            entity.Property(e => e.Zip).HasMaxLength(20);
        });

        modelBuilder.Entity<FeaturedItem>(entity =>
        {
            entity.HasOne(d => d.Category).WithMany(p => p.FeaturedItems)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FeaturedItemsCategories");

            entity.HasOne(d => d.GarageSale).WithMany(p => p.FeaturedItems)
                .HasForeignKey(d => d.GarageSaleId)
                .HasConstraintName("FK_FeaturedItemsGarageSales");
        });

        modelBuilder.Entity<GarageSale>(entity =>
        {
            entity.HasOne(d => d.Address).WithMany(p => p.GarageSales)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GarageSalesAddresses");

            entity.HasOne(d => d.Owner).WithMany(p => p.GarageSales)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GarageSalesUsers");

            entity.HasOne(d => d.SaleType).WithMany(p => p.GarageSales)
                .HasForeignKey(d => d.SaleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GarageSalesTypes");
        });

        modelBuilder.Entity<GarageSaleSchedule>(entity =>
        {
            entity.Property(e => e.From).HasColumnType("datetime");
            entity.Property(e => e.To).HasColumnType("datetime");

            entity.HasOne(d => d.GarageSale).WithMany(p => p.GarageSaleSchedules)
                .HasForeignKey(d => d.GarageSaleId)
                .HasConstraintName("FK_GarageSaleSchedulesSales");
        });

        modelBuilder.Entity<GarageSaleType>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationsUsers");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.UserName).HasMaxLength(100);

            entity.HasOne(d => d.Address).WithMany(p => p.Users)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsersAddresses");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
