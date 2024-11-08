using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.payload;
using worklog_api.Service;
using worklog_api.helper;
using worklog_api.Model;

namespace worklog_api.Controllers;


[Route("api/schedule")]
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

    [Route("get-schedule")]
    [HttpGet]
    public async Task<IActionResult> GetScheduleDetails([FromQuery] DateTime scheduleMonth, [FromQuery] Guid? egiId = null, [FromQuery] Guid? cnId = null)
    {
        try
        {
            var schedules = await _scheduleService.GetScheduleDetailsByMonth(scheduleMonth, egiId, cnId);

            var response = new ApiResponse<Schedule>
            {
                StatusCode = 200,
                Message = "Success",
                Data = schedules
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<string>
            {
                StatusCode = 500,
                Message = "An error occurred while retrieving schedule details.",
                Data = ex.Message
            };

            return StatusCode(500, response);
        }
    }

    [Route("update-schedule")]
    [HttpPut]
    public async Task<IActionResult> UpdateScheduleDetails([FromBody] Schedule updateScheduleRequest)
    {
        var user = JWT.GetUserInfo(HttpContext);

        var updatedDetails = new List<ScheduleDetail>();

        foreach (var detail in updateScheduleRequest.ScheduleDetails)
        {
            //if ID is not provided, it will be created as new
            if (detail.ID == Guid.Empty)
            {
                detail.CreatedAt = DateTime.Now;
                detail.CreatedBy = user.username;
            }

            updatedDetails.Add(new ScheduleDetail
            {
                ID = detail.ID,
                PlannedDate = detail.PlannedDate, // Assuming detail.PlannedDate is already a DateTime
                IsDone = detail.IsDone,
                UpdatedAt = DateTime.Now,
                CreatedAt = detail.CreatedAt,
                CreatedBy = detail.CreatedBy,
                UpdatedBy = user.username

          
            });
        }

        await _scheduleService.UpdateScheduleDetails(updateScheduleRequest.ID, updatedDetails);

        var response = new ApiResponse<object>
        {
            StatusCode = 200,
            Message = "Success update schedule",
            Data = new
            {
                User = user,
                Schedule = updateScheduleRequest
            }
        };

        return Ok(response);
    }


}