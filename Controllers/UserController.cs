using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.payload;
using worklog_api.Service;

namespace worklog_api.Controllers;


[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    public readonly ILogger<UserController> _logger;
    public readonly IUserService _userService;

    public UserController(ILogger<UserController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LoginRequest  loginRequest)
    {
        var loginResponse = await _userService.Login(loginRequest);
        return Ok(new ApiResponse<LoginResponse>(
               StatusCodes.Status200OK,
               "login success",
               loginResponse));
    }
    
    [Authorize(Policy = "RequireGroupLeader")]
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        return Ok(new ApiResponse<String>(
            StatusCodes.Status200OK,
            "login success",
            "test endpoint group leader"));
    }
    
}