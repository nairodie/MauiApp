namespace MauiApp2
{
    public static class DateValidationHelper
    {
        public static bool IsValidDateRange(DateTime? start, DateTime? end)
        {
            return start.HasValue && end.HasValue && start.Value <= end.Value;
        }

        public static (string? startError, string? endError) GetValidationErrors(DateTime? start, DateTime? end)
        {
            string? startError = null;
            string? endError = null;

            if (!start.HasValue)
                startError = "Start date is required.";

            if (!end.HasValue)
                endError = "End date is required.";
            else if (start.HasValue && start.Value > end.Value)
                startError = "Start date must be before or equal to end date.";
            return (startError, endError);
        }

        public static bool CorrectInvalidRange(ref DateTime start, ref DateTime end)
        {
            if (end < start)
            {
                end = start;
                return true;
            }
            return false;               
        }
    }
}
