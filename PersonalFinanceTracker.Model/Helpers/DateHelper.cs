namespace PersonalFinanceTracker.Model.Helpers
{
    public static class DateHelper
    {
        public static (DateTime, DateTime) GetDefaultStartEndTimeOfMonth()
        {
            var currentDate = DateTime.UtcNow;
            var startDate = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0);
            var endDate = startDate.AddMonths(1).AddSeconds(-1);
            return (startDate, endDate);
        }
    }
}
