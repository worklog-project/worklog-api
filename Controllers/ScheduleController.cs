using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.payload;
using worklog_api.Service;
using worklog_api.helper;
using worklog_api.Model;

namespace worklog_api.Controllers;


[Route("api/daily")]
[ApiController]
public class ScheduleController : ControllerBase
{
    public readonly ILogger<UserController> _logger;
    public readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [Route("create-schedule")]
    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] ScheduleDTO scheduleRequest)
    {
        var user = JWT.GetUserInfo(HttpContext);

        var schedule = new Schedule
        {
            EGIID = scheduleRequest.EGIID,
            CNID = scheduleRequest.CNID,
            ScheduleMonth = new DateTime(scheduleRequest.ScheduleMonth.Year, scheduleRequest.ScheduleMonth.Month, 1),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            CreatedBy = user.username,
            UpdatedBy = user.username,
            ScheduleDetails = new List<ScheduleDetail>()
        };

        foreach (var detail in scheduleRequest.ScheduleDetails)
        {
            if (DateTime.TryParseExact(detail.PlannedDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var plannedDate))
            {
                schedule.ScheduleDetails.Add(new ScheduleDetail
                {
                    PlannedDate = plannedDate,
                    IsDone = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CreatedBy = user.username,
                    UpdatedBy = user.username
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "Invalid date format in PlannedDate. Expected format: yyyy-MM-dd."
                });
            }
        }

        await _scheduleService.Create(schedule);

        var response = new ApiResponse<object>
        {
            StatusCode = 200,
            Message = "Success create schedule",
            Data = new
            {
                User = user,
                Schedule = scheduleRequest
            }
        };

        return Ok(response);
    }



}