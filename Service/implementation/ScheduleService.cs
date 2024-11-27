using System;
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
            var scheduleByMonth = await _scheduleRepository.GetScheduleByMonth(dateTime);
            var scheduleDetailsById = await _scheduleRepository.GetScheduleDetailsById(scheduleByMonth);


            var scheduleDetails = scheduleDetailsById.ToList();

            var dayByMonthAndYear = _dateHelper.GetDaysByMonthAndYear(DateTime.Parse(scheduleMonth));
            
            using (var workbook = new XLWorkbook())
            {
                // Add a worksheet
                var worksheet = workbook.Worksheets.Add("AGUSTUS 2023");
                // Title row with merged cells
                worksheet.Range("A1:BH1").Merge().Value = "ACHIEVEMENT PROGRAM DAILY CHECK WHEEL AGUSTUS 2023";

                worksheet.Cell("A2").Value = "NO";
                worksheet.Range("A2:A3").Merge();
                worksheet.Cell("B2").Value = "EGI";
                worksheet.Range("B2:B3").Merge();
                worksheet.Cell("C2").Value = "CN";
                worksheet.Range("C2:C3").Merge();
                

                
                // Loop through the cells, starting at column index 6
                int cellIndex = 4; // Initialize outside the loop to use later
                for (int dayIndex = 0; dayIndex < dayByMonthAndYear.Count; cellIndex += 2, dayIndex++)
                {
                    // Get the current day from the list
                    string currentDay = dayByMonthAndYear[dayIndex].ToString("dddd");
                    // Map currentDay to the cell `cellIndex`
                    worksheet.Cell(2, cellIndex).Value = currentDay;
                    worksheet.Cell(3, cellIndex).Value = (dayIndex + 1).ToString();
                }

                List<string> constantColumns = new List<string> { "TOTAL P", "TOTAL A", "ACH" };
                foreach (string constant in constantColumns)
                {
                    worksheet.Cell(2, cellIndex).Value = constant;
                    worksheet.Cell(2, cellIndex).Value = constant;
                    worksheet.Cell(2, cellIndex).Value = constant;
                    cellIndex += 1;
                }
                
                var end =scheduleDetailsById.Count() + 3;
                for (int row = 4; row <= end ; row++)
                {
                    if (row == end)
                    {
                        worksheet.Cell(end+1, 3).Value = "Daily";
                        worksheet.Cell(end+2, 3).Value = "Weekly";
                    }
                    worksheet.Cell(row, 1).Value = row - 3; // NO column
                    var scheduleDetail = scheduleDetails[row-4];
                    worksheet.Cell(row, 2).Value = scheduleDetail.ID.ToString(); // EGI column example
                    worksheet.Cell(row, 3).Value = scheduleDetail.ID.ToString();
                    
                    int totalP = 0;
                    int totalA = 0;
                    int innerCellIndex = 4; // Initialize outside the loop to use later
                    for (int dayIndex = 0; dayIndex < dayByMonthAndYear.Count; innerCellIndex += 2, dayIndex++)
                    {
                        // Get the current day from the list
                        DateTime currentDay = dayByMonthAndYear[dayIndex];
                        
                        
                        // if planned date days == current day, mark that as P
                        var planedDay = scheduleDetail.PlannedDate;
                        if ( planedDay == currentDay)
                        {
                            worksheet.Cell(row, innerCellIndex).Value = "P";
                            worksheet.Cell(row, innerCellIndex).Style.Fill.BackgroundColor = XLColor.FromHtml("#03bafc");
                            totalP++;
                            
                            // checked if details.Is_Done == true. 
                            if (scheduleDetail.IsDone)
                            {
                                worksheet.Cell(row, innerCellIndex+1).Value = "A";
                                worksheet.Cell(row, innerCellIndex +1).Style.Fill.BackgroundColor = XLColor.FromHtml("#fc03d7");
                                totalA++;
                            }
                        }
                    }
                    // untuk ph, th, ach
                    worksheet.Cell(row, innerCellIndex).Value = totalP.ToString();
                    worksheet.Cell(row, ++innerCellIndex).Value = totalA.ToString();
                    worksheet.Cell(row, innerCellIndex+1).Value = (totalA / totalP) * 100;
                    
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
                                   
                    if (dailyP > 0)
                    {
                        // Only calculate ratio if dailyP is not zero
                        mergedCell.Value = (double)dailyA/dailyP * 100;
                    }
                    else 
                    {
                        // Handle the case where dailyP is 0
                        mergedCell.Value = 0;
                    }
                }
                
                int weeklyCellIndex = 4; // Starting index
                int weeklyRow = end + 2; // Row where weekly averages will go
                int daysInWeek = 7;
                int columnsPerWeek = daysInWeek * 2;
                // Loop through the data in chunks of 7 days
                for (int weekStart = 0; weekStart < dayByMonthAndYear.Count; weekStart += daysInWeek)
                {
                    double weeklySum = 0;
                    int validDays = 0;
                    
                    // Calculate how many days are left in this week
                    int remainingDays = Math.Min(daysInWeek, dayByMonthAndYear.Count - weekStart);
                    int columnsToMerge = remainingDays * 2; // Each day takes 2 columns

                    // Sum up 7 days worth of values (or less if we're at the end)
                    for (int day = 0; day < daysInWeek && (weekStart + day) < dayByMonthAndYear.Count; day++)
                    {
                        int currentCellIndex = weeklyCellIndex + (day * 2);
        
                        // Get the value from the daily calculation row (end + 1)
                        var dailyValue = worksheet.Cell(end + 1, currentCellIndex).Value;
        
                        if (!dailyValue.IsBlank  && double.TryParse(dailyValue.ToString(), out double value))
                        {
                            weeklySum += value;
                            validDays++;
                        }
                    }

                    // Calculate weekly average
                    var mergedCell = worksheet.Range(worksheet.Cell(weeklyRow, weeklyCellIndex), 
                        worksheet.Cell(weeklyRow, weeklyCellIndex + columnsToMerge -1)).Merge();

                    if (validDays > 0)
                    {
                        mergedCell.Value = weeklySum / validDays; // Average of valid days
                    }
                    else
                    {
                        mergedCell.Value = 0;
                    }

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


                // Calculate ACH (sum of Total A / sum of Total P * 100)
                double ach = sumTotalP > 0 ? (sumTotalA / sumTotalP) * 100 : 0;
                
                worksheet.Range(
                    worksheet.Cell(end + 1, calcCellIndex+2), 
                    worksheet.Cell(end + 2, calcCellIndex+2)
                ).Merge().Value = Math.Round(ach, 2);
                
                // Adjust column widths for better appearance
                worksheet.Columns().AdjustToContents();
                // Save the workbook
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
            
        }
    }
}
