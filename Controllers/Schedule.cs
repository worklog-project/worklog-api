using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.payload;
using worklog_api.Service;
using worklog_api.helper;

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

        await _scheduleService.Create(scheduleRequest);

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