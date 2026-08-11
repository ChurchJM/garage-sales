using GarageSales.API.Entities;
using NetTopologySuite.Geometries;

public static class SpatialQueryExtensions
{
    public const double MetersPerMile = 1609.344;

    public static IQueryable<GarageSale> WithinRadiusOf(
        this IQueryable<GarageSale> query,
        Geometry origin,
        double radiusInMiles)
    {
        double radiusInMeters = radiusInMiles * MetersPerMile;

        return query.Where(gs =>
            gs.Address.Location != null &&
            gs.Address.Location.Distance(origin) <= radiusInMeters);
    }

    public static IQueryable<Notification> MatchingSaleLocation(
        this IQueryable<Notification> query,
        Geometry saleLocation)
    {
        return query.Where(n =>
            n.User.Address.Location != null &&
            n.User.Address.Location.Distance(saleLocation) <= (n.MaxRadius * MetersPerMile));
    }
}

public static class ValidationQueryExtensions
{
    public static IQueryable<GarageSale> ThisYearByUserName(this IQueryable<GarageSale> query, string userName)
    {
        var thisYear = DateTime.UtcNow.Year;

        // For simplicity, I am considering sales to be entirely in the year they started in. This would be an issue
        // if a sale began on Dec 31 and ended Jan 1, but that seems unlikely.
        return query.Where(gs => gs.Owner.UserName == userName 
            && gs.GarageSaleSchedules.Any(s => s.From.Year == thisYear));
    }
}