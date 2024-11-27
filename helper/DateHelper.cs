namespace worklog_api.helper;

public class DateHelper
{
    public DateHelper()
    {
    }

    public List<DateTime> GetDaysByMonthAndYear(DateTime date)
    {
        // Get first day of the month
        DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

        // Get number of days in the month
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

        // List to store all dates
        List<DateTime> allDates = new List<DateTime>();

        // Iterate through each day of the month
        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime currentDate = new DateTime(date.Year, date.Month, day);
            allDates.Add(currentDate);
        }

        return allDates;
    }
    
}