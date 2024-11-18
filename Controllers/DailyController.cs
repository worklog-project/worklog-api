using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.payload;
using worklog_api.Service;
using worklog_api.helper;
using worklog_api.Model.form;
using worklog_api.Repository.implementation;

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

    [Route("form")]
    [HttpPost]
    public async Task<IActionResult> AddDaily([FromBody] DailyRequest dailyRequest)
    {
        var response =  await _dailyService.InsertDaily(dailyRequest);
        return Ok(new ApiResponse<string>()
        {
            StatusCode = 201,
            Message = "success created daily",
            Data = response
        });
    }
    
    [Route("form-all")]
    [HttpGet]
    public async Task<IActionResult> GetAllDaily([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response =  await _dailyService.GetAllDaily(page, pageSize);
        return Ok(new ApiResponse<IEnumerable<AllDailyWorkLogDTO>>()
        {
            StatusCode = 200,
            Message = "success get daily",
            Data = response
        });
    }
    
    [Route("form-detail")]
    [HttpGet]
    public async Task<IActionResult> GetDailyDetail([FromQuery] string id)
    {
        var response =  await _dailyService.GetDailyDetailByID(id);
        return Ok(new ApiResponse<DailyWorklogDetailResponse>()
        {
            StatusCode = 200,
            Message = "success get daily detail",
            Data = response
        });
    }
    [Route("form")]
    [HttpDelete]
    public async Task<IActionResult> DeleteAllDaily([FromQuery] string id)
    {
        var response =  await _dailyService.DeleteAllDaily(id);
        return Ok(new ApiResponse<bool>()
        {
            StatusCode = 200,
            Message = "success get daily detail",
            Data = response
        });
    }
    [Route("form-detail")]
    [HttpDelete]
    public async Task<IActionResult> DeleteFormDaily([FromQuery] string id)
    {
        var response =  await _dailyService.DeleteFormDaily(id);
        return Ok(new ApiResponse<bool>()
        {
            StatusCode = 200,
            Message = "success get daily detail",
            Data = response
        });
    }
    
    
    
}