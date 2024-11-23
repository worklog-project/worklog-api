using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using worklog_api.Model;

namespace worklog_api.helper
{
    public class FormToPDF
    {
        static string excavatorHTMLTemplate = @"
        <!DOCTYPE html>
        <html lang=""en"">
          <head>
            <meta charset=""UTF-8"" />
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <title>Daily Check Sheet Excavator</title>
            <style>
              body {
                font-family: Arial, sans-serif;
                margin: 0;
                padding: 5mm;
                width: 210mm;
                height: 297mm;
                box-sizing: border-box;
              }
              .container {
                width: 100%;
                max-width: 100%;
                margin: auto;
              }
              table {
                width: 100%;
                border-collapse: collapse;
                margin-bottom: 10px;
              }
              table,
              th,
              td {
                border: 1px solid #ccc;
              }
              th,
              td {
                padding: 2px;
                text-align: left;
                font-size: 10px;
              }
              h1,
              h2 {
                text-align: center;
                font-size: 12px;
              }
              .status-column {
                width: 100px; /* Set a consistent width for the status column */
              }
              @media print {
                body {
                  margin: 0;
                  padding: 5mm;
                  width: auto;
                  height: auto;
                }
              }
            </style>
          </head>
          <body>
            <div class=""container"">
              <h1>{FormType}</h1>
              <h2>Unit Model: {EgiName}</h2>
              <table>
                <tr>
                  <th>Code Number</th>
                  <td>{CodeNumber}</td>
                </tr>
                <tr>
                  <th>Tanggal</th>
                  <td>{Tanggal}</td>
                </tr>
                <tr>
                  <th>Hour Meter</th>
                  <td>{HourMeter}</td>
                </tr>
                <tr>
                  <th>Start Time</th>
                  <td>{StartTime}</td>
                </tr>
                <tr>
                  <th>Finish Time</th>
                  <td>{FinishTime}</td>
                </tr>
              </table>
              <table>
                <colgroup>
                  <col />
                  <col />
                  <col />
                  <col class=""status-column"" />
                </colgroup>
                <tr>
                  <th>No</th>
                  <th>Activities</th>
                  <th>CRITICAL POINT</th>
                  <th>Status</th>
                </tr>
                {ActivityRows}
              </table>

              <h2>PENAMBAHAN CONSUMMABLE GOODS</h2>
              <table>
                <colgroup>
                  <col />
                  <col />
                  <col />
                  <col class=""status-column"" />
                </colgroup>
                <tr>
                  <th>No</th>
                  <th>Component</th>
                  <th>Type</th>
                  <th>Status</th>
                </tr>
                {ConsumableRows}
              </table>

              <h2 style=""font-size: 10px"">Legend</h2>
              <ul style=""font-size: 10px; margin-top: 3px; padding-left: 15px; color:red"">
                <li><strong>V</strong> : CHECK/VISUAL</li>
                <li><strong>X</strong> : RUSAK ( UNIT MASIH BISA DIOPERASIKAN ) </li>
                <li><span style=""display: inline-block; width: 10px; height: 10px; border: 1px solid black; border-radius: 50%; text-align: center; line-height: 11px; font-weight: bold"">X</span> : GANTI / DIPERBAIKI</li>
                <li style=""font-size: 10px"">
                  <span style=""display: inline-block; width: 10px; height: 10px; border: 1px solid black; border-radius: 50%; text-align: center; line-height: 11px; font-weight: bold"">V</span>
                  : IMPROVEMENT (FLUSHING)
                </li>
                <li><strong>NA</strong>: NOT APLICABLE</li>
              </ul>

              <table style=""width: auto; border-collapse: collapse; font-size: 10px;"">
                  <tr>
                    <th style=""text-align: left; padding: 4px; border: 1px solid #ccc;"">Group Leader</th>
                    <td style=""padding: 4px; border: 1px solid #ccc;"">{GroupLeader}</td>
                  </tr>
                  <tr>
                    <th style=""text-align: left; padding: 4px; border: 1px solid #ccc;"">Mechanic / Inspector</th>
                    <td style=""padding: 4px; border: 1px solid #ccc;"">{Mechanic}</td>
                  </tr>
              </table>
            </div>
          </body>
        </html>
        ";

        static string hd7857HTMLTemplate = @"
         <!DOCTYPE html>
         <html lang=""""en"""">
           <head>
             <meta charset=""""UTF-8"""" />
             <meta name=""""viewport"""" content=""""width=device-width, initial-scale=1.0"""" />
             <title>Daily Check Sheet HD785-7</title>
             <style>
               body {
                 font-family: Arial, sans-serif;
                 margin: 0;
                 padding: 5mm;
                 width: 210mm;
                 height: 297mm;
                 box-sizing: border-box;
               }
               .container {
                 width: 100%;
                 max-width: 100%;
                 margin: auto;
               }
               table {
                 width: 100%;
                 border-collapse: collapse;
                 margin-bottom: 10px;
               }
               table,
               th,
               td {

                 border: 1px solid #ccc;
               }
               th,
               td {
                 padding: 2px;
                 text-align: left;
                 font-size: 10px;
               }
               h1,
               h2 {
                 text-align: center;
                 font-size: 12px;
               }
               .status-column {
                 width: 100px; /* Set a consistent width for the status column */
               }
               @media print {
                 body {
                   margin: 0;
                   padding: 5mm;
                   width: auto;
                   height: auto;
                 }
               }
             </style>
           </head>
           <body>
             <div class=""""container"""">
               <h1>{FormType}</h1>
               <h2>Unit Model: {EgiName}</h2>
               <table>
                 <tr>
                   <th>Code Number</th>
                   <td>{CodeNumber}</td>
                 </tr>
                 <tr>
                   <th>Hour Meter</th>
                   <td>{HourMeter}</td>
                 </tr>
                 <tr>
                   <th>Tanggal</th>
                   <td>{Tanggal}</td>
                 </tr>
               </table>
               <table>
                 <colgroup>
                   <col />
                   <col />
                   <col />
                   <col class=""status-column"" />
                 </colgroup>
                 <tr>
                   <th>No</th>
                   <th>DESCRIPTION</th>
                   <th>EXPLANATION</th>
                   <th>ACTIVITY</th>
                   <th>VALUE</th>
                   <th>STD TIME</th>
                   <th>AREA</th>
                   <th>POSISI</th>
                   <th>CHECK/CONDITION</th>
                 </tr>
                 {InspectionRows}
               </table>

               <h2 style=""font-size: 10px"">Legend</h2>
               <ul style=""font-size: 10px; margin-top: 3px; padding-left: 15px; color:red"">
                 <li><strong>V</strong> : CHECK/VISUAL</li>
                 <li><strong>X</strong> : RUSAK ( UNIT MASIH BISA DIOPERASIKAN ) </li>
                 <li><span style=""display: inline-block; width: 10px; height: 10px; border: 1px solid black; border-radius: 50%; text-align: center; line-height: 11px; font-weight: bold"">X</span> : GANTI / DIPERBAIKI</li>
                 <li style=""font-size: 10px"">
                   <span style=""display: inline-block; width: 10px; height: 10px; border: 1px solid black; border-radius: 50%; text-align: center; line-height: 11px; font-weight: bold"">V</span>
                   : IMPROVEMENT (FLUSHING)
                 </li>
                 <li><strong>NA</strong>: NOT APLICABLE</li>
               </ul>

               <table style=""width: auto; border-collapse: collapse; font-size: 10px;"">
                   <tr>
                     <th style=""text-align: left; padding: 4px; border: 1px solid #ccc;"">Group Leader</th>
                     <td style=""padding: 4px; border: 1px solid #ccc;"">{GroupLeader}</td>
                   </tr>
                   <tr>
                     <th style=""text-align: left; padding: 4px; border: 1px solid #ccc;"">Mechanic / Inspector</th>
                     <td style=""padding: 4px; border: 1px solid #ccc;"">{Mechanic}</td>
                   </tr>
               </table>
             </div>
           </body>
         </html>
        ";

        public static Task<string> ExcavatorFormToPDF(DailyModel dailyForm)
        {
            var activityRows = new StringBuilder();
            var consumableRows = new StringBuilder();
            var _sheetDetail = dailyForm._sheetDetail;

            // Check if _sheetDetail contains the "activities" key and if it's a JsonElement
            if (_sheetDetail.TryGetValue("activities", out var activitiesObj))
            {
                if (activitiesObj is JsonElement activitiesElement)
                {
                    // Check if the element is an array
                    if (activitiesElement.ValueKind == JsonValueKind.Array)
                    {
                        // Deserialize the JsonArray into a List<Dictionary<string, object>>
                        var activities = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(activitiesElement.GetRawText());

                        int activityCount = 1;

                        // Iterate over each activity in the activities list
                        foreach (var activity in activities)
                        {
                            // Check if each activity contains the expected "bagian" and "isiBagian"
                            if (activity.TryGetValue("bagian", out var bagian) &&
                                activity.TryGetValue("isiBagian", out var isiBagianObj))
                            {

                                activityRows.AppendLine($@"
                                    <tr class=""activities"">
                                      <td></td>
                                      <td style=""font-weight: bold; font-size: 14px;"">{activity.GetValueOrDefault("bagian")}</td>
                                      <td></td>                                  
                                      <td></td>
                                    </tr>");
                                // Deserialize isiBagian array
                                var isiBagian = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(isiBagianObj.ToString());

                                // Iterate over each item in the "isiBagian" array
                                foreach (var item in isiBagian)
                                {
                                    // Generate the HTML row for each item in "isiBagian"
                                    activityRows.AppendLine($@"
                                    <tr class=""activities"">
                                      <td>{activityCount}</td>
                                      <td>{item.GetValueOrDefault("namaIsiBagian")}</td>
                                      <td>{item.GetValueOrDefault("criticalPoint")}</td>
                                      <td>{item.GetValueOrDefault("condition")}</td>
                                    </tr>");
                                    activityCount++; // Increment the counter for each row
                                }
                            }
                        }
                    }
                }
            }

            // Build consumable goods rows from "consumeAbleGoods" data
            if (_sheetDetail.TryGetValue("consumeAbleGoods", out var consumeAbleGoodsObj))
            {
                var consumables = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(consumeAbleGoodsObj.ToString());
                int consumableCount = 1;
                foreach (var consumable in consumables)
                {
                    consumableRows.AppendLine($@"
                    <tr class=""consumable_goods"">
                      <td>{consumableCount}</td>
                      <td>{consumable.GetValueOrDefault("component")}</td>
                      <td>{consumable.GetValueOrDefault("type")}</td>
                      <td>{consumable.GetValueOrDefault("condition")}</td>
                    </tr>");
                    consumableCount++;
                }
            }

            // Replace placeholders in the HTML template
            var filledHtml = excavatorHTMLTemplate
                .Replace("{CodeNumber}", _sheetDetail.GetValueOrDefault("CodeNumber", dailyForm._cnName).ToString())
                .Replace("{Tanggal}", _sheetDetail.GetValueOrDefault("Tanggal", dailyForm._date).ToString())
                .Replace("{HourMeter}", _sheetDetail.GetValueOrDefault("HourMeter", dailyForm._hourmeter).ToString())
                .Replace("{StartTime}", _sheetDetail.GetValueOrDefault("StartTime", dailyForm._startTime).ToString())
                .Replace("{FinishTime}", _sheetDetail.GetValueOrDefault("FinishTime", dailyForm._endTime).ToString())
                .Replace("{FormType}", _sheetDetail.GetValueOrDefault("FormType", dailyForm._formType).ToString())
                .Replace("{EgiName}", _sheetDetail.GetValueOrDefault("EgiName", dailyForm._egiName).ToString())
                .Replace("{ActivityRows}", activityRows.ToString())
                .Replace("{ConsumableRows}", consumableRows.ToString())
                .Replace("{GroupLeader}", _sheetDetail.GetValueOrDefault("GroupLeader", dailyForm._groupLeader).ToString())
                .Replace("{Mechanic}", _sheetDetail.GetValueOrDefault("Mechanic", dailyForm._mechanic).ToString());

            return Task.FromResult(filledHtml);
        }

        public static Task<string> HD785FormToPDF(DailyModel dailyForm)
        {
            var inspectionRows = new StringBuilder();
            var _sheetDetail = dailyForm._sheetDetail;

            // Check if _sheetDetail contains the "inspection" key and if it's a JsonElement
            if (_sheetDetail.TryGetValue("inspection", out var inspectionObj))
            {
                if (inspectionObj is JsonElement inspectionElement)
                {
                    // Check if the element is an array
                    if (inspectionElement.ValueKind == JsonValueKind.Array)
                    {
                        // Deserialize the JsonArray into a List<Dictionary<string, object>>
                        var inspections = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(inspectionElement.GetRawText());

                        int inspectionCount = 1;

                        // Iterate over each inspection in the inspections list
                        foreach (var inspection in inspections)
                        {
                            Console.WriteLine(inspection);
                            // Check if each inspection contains the expected keys
                                // Generate the HTML row for each inspection
                                inspectionRows.AppendLine($@"
                                    <tr class=""inspections"">
                                      <td>{inspectionCount}</td>
                                      <td>{inspection.GetValueOrDefault("description")}</td>
                                      <td>{inspection.GetValueOrDefault("explanation")}</td>
                                      <td>{inspection.GetValueOrDefault("activity")}</td>
                                      <td>{inspection.GetValueOrDefault("value")}</td>
                                      <td>{inspection.GetValueOrDefault("stdTime")}</td>
                                      <td>{inspection.GetValueOrDefault("area")}</td>
                                      <td>{inspection.GetValueOrDefault("posisi")}</td>
                                      <td>{inspection.GetValueOrDefault("condition")}</td>
                                    </tr>");
                                inspectionCount++; // Increment the counter for each row
                        }
                    }
                }
            }

            // Replace placeholders in the HTML template
            var filledHtml = hd7857HTMLTemplate
                .Replace("{CodeNumber}", _sheetDetail.GetValueOrDefault("CodeNumber", dailyForm._cnName).ToString())
                .Replace("{HourMeter}", _sheetDetail.GetValueOrDefault("HourMeter", dailyForm._hourmeter).ToString())
                .Replace("{Tanggal}", _sheetDetail.GetValueOrDefault("Tanggal", dailyForm._date).ToString())
                .Replace("{EgiName}", _sheetDetail.GetValueOrDefault("EgiName", dailyForm._egiName).ToString())
                .Replace("{InspectionRows}", inspectionRows.ToString())
                .Replace("{GroupLeader}", _sheetDetail.GetValueOrDefault("GroupLeader", dailyForm._groupLeader).ToString())
                .Replace("{Mechanic}", _sheetDetail.GetValueOrDefault("Mechanic", dailyForm._mechanic).ToString())
                .Replace("{FormType}", _sheetDetail.GetValueOrDefault("FormType", dailyForm._formType).ToString());


            return Task.FromResult(filledHtml);
        }
    }
}
