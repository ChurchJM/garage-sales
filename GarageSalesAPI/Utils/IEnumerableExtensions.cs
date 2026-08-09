public static class ValidationExtensions
{
    // All GarageSaleSchedules associated with a GarageSale must be within 2 consecutive days.
    public static (bool IsValid, string ErrorMessage) ValidateScheduleSpan(
        this IEnumerable<GarageSaleScheduleDTO>? schedules)
    {
        if (schedules == null || !schedules.Any())
        {
            return (false, "At least one scheduled date is required.");
        }

        var earliestFrom = schedules.Min(s => s.From);
        var latestTo = schedules.Max(s => s.To);

        var saleStart = DateOnly.FromDateTime(earliestFrom);
        var saleEnd = DateOnly.FromDateTime(latestTo);

        if (saleEnd.DayNumber - saleStart.DayNumber > 1)
        {
            return (false, "Garage sales may only span two consecutive days.");
        }

        return (true, string.Empty);
    }
}