using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.payload;
using worklog_api.Service;
using worklog_api.helper;

namespace worklog_api.Controllers;


[Route("api/daily")]
[ApiController]
public class DailyController : ControllerBase
{
    public readonly ILogger<UserController> _logger;
    public readonly IDailyService _dailyService;

    public DailyController(IDailyService dailyService)
    {
        _dailyService = dailyService;
    }

    //[Authorize(Policy = "RequireGroupLeader")]
    [Route("form")]
    [HttpGet]
    
    public async Task<IActionResult> GetForm([FromQuery] string egi, [FromQuery] string codeNumber)
    {
        var user = JWT.GetUserInfo(HttpContext);

        return Ok(new ApiResponse<string>(
            StatusCodes.Status200OK,
            "login success",
            $"Test endpoint for group leader, User: {user.username}, Role: {user.role}, EGI: {egi}, CN: {codeNumber}"
        ));
    }

    //[Authorize(Policy = "RequireGroupLeader")]
    [Route("list-egi")]
    [HttpGet]
    public async Task<IActionResult> GetListEGI([FromQuery] string egi = "")
    {
        egi = egi.ToUpper();

        var listEGI = await _dailyService.GetEGI(egi);

        var response = new ApiResponse<object>
        {
            StatusCode = 200,
            Message = "Success get EGIs",
            Data = new
            {
                Egis = listEGI
            }
        };

        return Ok(response);

    }

    [Route("list-code-number")]
    [HttpGet]
    public async Task<IActionResult> GetListCodeNumber([FromQuery] Guid egiID, [FromQuery] string codeNumber = "")
    {
        codeNumber = codeNumber.ToUpper();

        var listCodeNumber = await _dailyService.GetCodeNumber(codeNumber, egiID);

        var response = new ApiResponse<object>
        {
            StatusCode = 200,
            Message = "Success get EGIs",
            Data = new
            {
                CodeNumber = listCodeNumber
            }
        };

        return Ok(response);
    }

}