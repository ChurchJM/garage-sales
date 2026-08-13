public static class ValidationExtensions
{
    // .NET EST time zone automatically handles EST vs EDT
    private static TimeZoneInfo EST = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
    
    // All GarageSaleSchedules associated with a GarageSale must be within 2 consecutive days.
    public static (bool IsValid, string ErrorMessage) ValidateScheduleSpan(
        this IEnumerable<GarageSaleScheduleDTO>? schedules)
    {
        if (schedules == null || !schedules.Any())
        {
            return (false, "At least one scheduled date is required.");
        }

        var earliestFromUtc = schedules.Min(s => s.From);
        var latestToUtc = schedules.Max(s => s.To);

        var earliestFromEST = TimeZoneInfo.ConvertTimeFromUtc(earliestFromUtc, EST);
        var latestToEST = TimeZoneInfo.ConvertTimeFromUtc(latestToUtc, EST);

        var saleStart = DateOnly.FromDateTime(earliestFromEST);
        var saleEnd = DateOnly.FromDateTime(latestToEST);

        if (saleEnd.DayNumber - saleStart.DayNumber > 1)
        {
            return (false, "Garage sales may only span two consecutive days.");
        }

        return (true, string.Empty);
    }
}