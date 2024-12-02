using System;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using ClosedXML.Excel;
using worklog_api.error;
using worklog_api.helper;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Repository;
using worklog_api.Repository.implementation;

namespace worklog_api.Service.implementation
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly DateHelper _dateHelper;

        public ScheduleService(IScheduleRepository scheduleRepository, DateHelper dateHelper)
        {
            _scheduleRepository = scheduleRepository;
            _dateHelper = dateHelper;
        }

        public async Task Create(Schedule schedule)
        {
            // check if schedule with cn id, egi id, and schedule month already exist
            var egiId = schedule.EGIID;
            var cnId = schedule.CNID;

            var existingSchedule = await _scheduleRepository.GetScheduleDetailsByMonth(schedule.ScheduleMonth, egiId, cnId);

            // If an existing schedule is found, throw an error
            if (existingSchedule != null)
            {
                throw new InternalServerError("A schedule with the same EGI, CN, and month already exists.");
            }


            try
            {
                await _scheduleRepository.Create(schedule); 

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError(e.Message);
            }
        }

        public async Task<Schedule> GetScheduleDetailsByMonth(DateTime scheduleMonth, Guid? egiId = null, Guid? cnId = null)
        {
            try
            {
                return await _scheduleRepository.GetScheduleDetailsByMonth(scheduleMonth, egiId, cnId);
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError("Error retrieving schedule details");
            }
        }

        public async Task UpdateScheduleDetails(Guid scheduleId, List<ScheduleDetail> updatedDetails)
        {
            try
            {
                await _scheduleRepository.UpdateScheduleDetails(scheduleId, updatedDetails);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new InternalServerError("Error updating schedule details");
            }
        }

        public async Task<Byte[]> GetScheduleByMonth(string scheduleMonth)
        {
            var dateTime = DateTime.Parse(scheduleMonth);
            var culture = new CultureInfo("id-ID"); // Indonesian culture info
            var monthName = dateTime.ToString("MMMM", culture); // Get the full month name
            var year = dateTime.Year;
            
            var scheduleByMonth = await _scheduleRepository.GetScheduleByMonth(dateTime);
            var scheduleDetails = scheduleByMonth.ToList();

            var dayByMonthAndYear = _dateHelper.GetDaysByMonthAndYear(DateTime.Parse(scheduleMonth));

            var calculateOverallWeeklyAchievement = CalculateWeeklyAchievementsForMonth(scheduleDetails, dateTime);

            using (var workbook = new XLWorkbook())
            {
                // Construct the worksheet name and title
                var worksheetName = $"{monthName.ToUpper()} {year}";
                var title = $"ACHIEVEMENT PROGRAM DAILY CHECK WHEEL {worksheetName}";
                
                // Add a worksheet
                var worksheet = workbook.Worksheets.Add(worksheetName);
                // Title row with merged cells
                worksheet.Range("A1:C1").Merge();
                
                // Merge cells and set value
                worksheet.Range("D1:BH1").Merge();
                worksheet.Range("D1:BH1").Value = title;
                
                // Make the merged cell bold
                worksheet.Range("D1:BH1").Style.Font.Bold = true;
                
                // Increase row height (adjust the value as needed)
                worksheet.Row(1).Height = 30; // Set row height to 30 points
                
                
                worksheet.Cell("A2").Value = "NO";
                worksheet.Range("A2:A3").Merge();
                worksheet.Range("A2:A3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                worksheet.Cell("B2").Value = "EGI";
                worksheet.Range("B2:B3").Merge();
                worksheet.Range("B2:B3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                
                worksheet.Cell("C2").Value = "CN";
                worksheet.Range("C2:C3").Merge();
                worksheet.Range("C2:C3").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                

                worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                
                worksheet.SheetView.FreezeColumns(3);
                worksheet.SheetView.FreezeRows(3);
                
                
                // Loop through the cells, starting at column index 6
                int cellIndex = 4; // Initialize outside the loop to use later
                for (int dayIndex = 0; dayIndex < dayByMonthAndYear.Count; cellIndex += 2, dayIndex++)
                {
                    CultureInfo indonesianCulture = new CultureInfo("id-ID");
                    // Get the current day from the list
                    string currentDay = dayByMonthAndYear[dayIndex].ToString("dddd", indonesianCulture);
                    // Map currentDay to the cell `cellIndex`
                    worksheet.Cell(2, cellIndex).Value = currentDay;
                    worksheet.Cell(2, cellIndex).Style.Font.Bold = true;
                    worksheet.Range(worksheet.Cell(2, cellIndex), worksheet.Cell(2, cellIndex+1)).Merge();
                    worksheet.Range(worksheet.Cell(2, cellIndex), worksheet.Cell(2, cellIndex+1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    if (currentDay == "Minggu")
                    {
                        worksheet.Range(worksheet.Cell(2, cellIndex), worksheet.Cell(2, cellIndex+1)).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255,0,0);
                    }
                    else
                    {
                        worksheet.Range(worksheet.Cell(2, cellIndex), worksheet.Cell(2, cellIndex+1)).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 0,176,80);
                    }

                    
                    worksheet.Cell(3, cellIndex).Value = (dayIndex + 1).ToString();
                    worksheet.Cell(3, cellIndex).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 0,176,80);
                    worksheet.Range(worksheet.Cell(3, cellIndex), worksheet.Cell(3, cellIndex+1)).Merge();
                    worksheet.Range(worksheet.Cell(3, cellIndex), worksheet.Cell(3, cellIndex+1)).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 0,176,80);
                    worksheet.Range(worksheet.Cell(3, cellIndex), worksheet.Cell(3, cellIndex+1)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                List<string> constantColumns = new List<string> { "TOTAL P", "TOTAL A", "ACH" };
                foreach (string constant in constantColumns)
                {
                    
                    worksheet.Cell(2, cellIndex).Value = constant;
                    worksheet.Cell(2, cellIndex).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(worksheet.Cell(2, cellIndex), worksheet.Cell(3, cellIndex)).Merge();

                    if (constant == "TOTAL P")
                    {
                        worksheet.Cell(2, cellIndex).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 0,176,240);
                    }else if (constant == "TOTAL A")
                    {
                        worksheet.Cell(2, cellIndex).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255,255,0);

                    }else if (constant == "ACH")
                    {
                        worksheet.Cell(2, cellIndex).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 146,208,80);

                    }
                    
                    cellIndex += 1;
                }
                
                var end = scheduleByMonth.Count() + 3;
                for (int row = 4; row <= end ; row++)
                {
                    if (row == end)
                    {
                        worksheet.Cell(end+1, 3).Value = "Daily";
                        worksheet.Cell(end+1, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(end+1, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(end+1, 2).Style.Border.TopBorder = XLBorderStyleValues.Thin;

                        worksheet.Cell(end+2, 3).Value = "Weekly";
                        worksheet.Cell(end+2, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    worksheet.Cell(row, 1).Value = row - 3; // NO column
                    var scheduleDetail = scheduleDetails[row-4];
                    worksheet.Cell(row, 2).Value = scheduleDetail.Egi; // EGI column example
                    worksheet.Column(2).Width = 15; 
                    worksheet.Cell(row, 3).Value = scheduleDetail.CodeNumber;
                    worksheet.Column(3).Width = 15; 
                    
                    int totalP = 0;
                    int totalA = 0;
                    int innerCellIndex = 4; // Initialize outside the loop to use later
                    for (int dayIndex = 0; dayIndex < dayByMonthAndYear.Count; innerCellIndex += 2, dayIndex++)
                    {
                        // Get the current day from the list
                        DateTime currentDay = dayByMonthAndYear[dayIndex];
                        
                        worksheet.Column(innerCellIndex).Width = 5; // Set a fixed width
                        worksheet.Column(innerCellIndex+1).Width = 5; // Ensure both columns have same width
                        
                        worksheet.Cell(row, innerCellIndex).Style.Border.OutsideBorder = XLBorderStyleValues.Thin; 
                        worksheet.Cell(row, innerCellIndex+1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin; 

                        
                        foreach (var sch in scheduleDetail.ScheduleDetails)
                        {
                            
                            // if planned date days == current day, mark that as P
                            var planedDay = sch.PlannedDate;
                            if ( planedDay == currentDay)
                            {
                                worksheet.Cell(row, innerCellIndex).Value = "P";
                                worksheet.Cell(row, innerCellIndex).Style.Fill.BackgroundColor = XLColor.FromHtml("#03bafc");
                                totalP++;
                            
                                // checked if details.Is_Done == true. 
                                if (sch.IsDone == true)
                                {
                                    worksheet.Cell(row, innerCellIndex+1).Value = "A";
                                    worksheet.Cell(row, innerCellIndex +1).Style.Fill.BackgroundColor = XLColor.FromHtml("#fc03d7");
                                    totalA++;
                                }
                            }
                        }
                    }
                    // untuk ph, th, ach
                    worksheet.Cell(row, innerCellIndex).Value = totalP.ToString();
                    worksheet.Cell(row, innerCellIndex).Style.Border.OutsideBorder = XLBorderStyleValues.Thin ;
                    
                    worksheet.Cell(row, ++innerCellIndex).Value = totalA.ToString();
                    worksheet.Cell(row, innerCellIndex).Style.Border.OutsideBorder = XLBorderStyleValues.Thin ;

                    var ac = ((double)totalA / totalP) * 100;
                    worksheet.Cell(row, innerCellIndex+1).Value = (totalP == 0) ? 0 : Math.Round(ac, 2).ToString() + " %";
                    worksheet.Cell(row, innerCellIndex+1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin ;
                }
                
                int outerCellIndex = 4; // Initialize outside the loop to use later
                for (int dayIndex = 0; dayIndex < dayByMonthAndYear.Count; outerCellIndex += 2, dayIndex++)
                {
                    int dailyA = 0;
                    int dailyP = 0;
                    for (int row = 4; row <= end; row++)
                    {
                        if (worksheet.Cell(row, outerCellIndex).Value.ToString() == "P")
                        {
                            dailyP++;
                        }
                        if (worksheet.Cell(row, outerCellIndex+1).Value.ToString() == "A")
                        {
                            dailyA++;
                        }
                    }
                    var mergedCell = worksheet.Range(worksheet.Cell(end+1, outerCellIndex), 
                        worksheet.Cell(end+1, outerCellIndex+1)).Merge();
                    mergedCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin; 

                    if (dailyP > 0)
                    {
                        // Only calculate ratio if dailyP is not zero
                        var a = (double)dailyA/dailyP * 100;
                        mergedCell.Value = Math.Round(a, 2).ToString() + " %";
                    }
                    else 
                    {
                        // Handle the case where dailyP is 0
                        mergedCell.Value = "0 %";
                    }
                }
                
                int weeklyCellIndex = 4; // Starting index
                int weeklyRow = end + 2; // Row where weekly averages will go
                int daysInWeek = 7;
                
                foreach (var weeklyAchievement in calculateOverallWeeklyAchievement)
                {
                    // Calculate how many days are in this week
                    int remainingDays = (weeklyAchievement.WeekEnd - weeklyAchievement.WeekStart).Days + 1;
                    int columnsToMerge = remainingDays * 2; // Each day takes 2 columns

                    // Merge cells for this week
                    var mergedCell = worksheet.Range(
                        worksheet.Cell(weeklyRow, weeklyCellIndex), 
                        worksheet.Cell(weeklyRow, weeklyCellIndex + columnsToMerge - 1)
                    ).Merge();

                    mergedCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin; 

                    // Set the achievement percentage
                    mergedCell.Value = weeklyAchievement.OverallAchievementPercentage.ToString() + " %";

                    // Move to next week's starting column
                    weeklyCellIndex += daysInWeek * 2; // Multiply by 2 because each day uses 2 columns
                }
                
                // Loop for total P, total A, and ACH calculations
                int calcCellIndex = dayByMonthAndYear.Count * 2 + 4; // Start at the PH column

                // Calculate total P
                double sumTotalP = 0;
                for (int row = 4; row <= end; row++)
                {
                    double totalP;
                    if (double.TryParse(worksheet.Cell(row, calcCellIndex).Value.ToString(), out totalP))
                    {
                        sumTotalP += totalP;
                    }
                }
                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex), 
                    worksheet.Cell(end + 2, calcCellIndex)
                ).Merge().Value = sumTotalP;

                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex), 
                    worksheet.Cell(end + 2, calcCellIndex)
                ).Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                
                // Calculate total A
                double sumTotalA = 0;
                for (int row = 4; row <= end; row++)
                {
                    double totalA;
                    if (double.TryParse(worksheet.Cell(row, calcCellIndex + 1).Value.ToString(), out totalA))
                    {
                        sumTotalA += totalA;
                    }
                }
                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex+1), 
                    worksheet.Cell(end + 2, calcCellIndex+1)
                ).Merge().Value = sumTotalA;

                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex+1), 
                    worksheet.Cell(end + 2, calcCellIndex+1)
                ).Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // Calculate ACH (sum of Total A / sum of Total P * 100)
                double ach = sumTotalP > 0 ? (sumTotalA / sumTotalP) * 100 : 0;
                
                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex+2), 
                    worksheet.Cell(end + 2, calcCellIndex+2)
                ).Merge().Value = Math.Round(ach, 2).ToString() + " %";
                
                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex+2), 
                    worksheet.Cell(end + 2, calcCellIndex+2)
                ).Merge().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                
                // Save the workbook
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
            
        }
        
        public class WeeklyAchievement
        {
            public DateTime WeekStart { get; set; }
            public DateTime WeekEnd { get; set; }
            public int TotalPlannedTasks { get; set; }
            public int TotalAchievedTasks { get; set; }
            public double OverallAchievementPercentage { get; set; }
            public List<EgiTaskDetails> EgiDetails { get; set; }
        }

        public class EgiTaskDetails
        {
            public string EgiName { get; set; }
            public int PlannedTasks { get; set; }
            public int AchievedTasks { get; set; }
            public double AchievementPercentage { get; set; }
        }
        
        public List<WeeklyAchievement> CalculateWeeklyAchievementsForMonth(List<Schedule> scheduleByMonth, DateTime monthYear)
{
    var weeklyAchievements = new List<WeeklyAchievement>();

    // Get the first and last day of the month
    var firstDayOfMonth = new DateTime(monthYear.Year, monthYear.Month, 1);
    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

    // Iterate through weeks in the month
    var currentWeekStart = firstDayOfMonth;
    while (currentWeekStart <= lastDayOfMonth)
    {
        var currentWeekEnd = currentWeekStart.AddDays(6);

        // Ensure we don't go beyond the last day of the month
        if (currentWeekEnd > lastDayOfMonth)
        {
            currentWeekEnd = lastDayOfMonth;
        }

        // Filter tasks for this specific week
        var weekTasks = scheduleByMonth
            .SelectMany(s => s.ScheduleDetails
                .Where(sd => sd.PlannedDate >= currentWeekStart && sd.PlannedDate <= currentWeekEnd)
                .Select(sd => new 
                {
                    EgiName = s.Egi,
                    IsDone = sd.IsDone
                }))
            .ToList();

        // Calculate EGI-specific details
        var egiDetails = weekTasks
            .GroupBy(t => t.EgiName)
            .Select(g => new EgiTaskDetails
            {
                EgiName = g.Key,
                PlannedTasks = g.Count(),
                AchievedTasks = g.Count(t => t.IsDone),
                AchievementPercentage = g.Count() > 0 
                    ? Math.Round((double)g.Count(t => t.IsDone) / g.Count() * 100, 2) 
                    : 0
            })
            .ToList();

        // Create weekly achievement (even if no tasks)
        weeklyAchievements.Add(new WeeklyAchievement
        {
            WeekStart = currentWeekStart,
            WeekEnd = currentWeekEnd,
            TotalPlannedTasks = weekTasks.Count(),
            TotalAchievedTasks = weekTasks.Count(t => t.IsDone),
            OverallAchievementPercentage = weekTasks.Count() > 0
                ? Math.Round((double)weekTasks.Count(t => t.IsDone) / weekTasks.Count() * 100, 2)
                : 0,
            EgiDetails = egiDetails
        });

        // Move to next week
        currentWeekStart = currentWeekEnd.AddDays(1);
    }

    return weeklyAchievements;
}
    }
}
