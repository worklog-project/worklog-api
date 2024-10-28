using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Service;
using worklog_api.helper;

namespace worklog_api.Controllers
{
    [ApiController]
    [Route("api/mol/tracking")]
    public class MOLTrackingHistoryController : ControllerBase
    {
        private readonly IMOLTrackingHistoryService _molTrackingHistoryService;

        public MOLTrackingHistoryController(IMOLTrackingHistoryService molTrackingHistoryService)
        {
            _molTrackingHistoryService = molTrackingHistoryService;
        }

        [Authorize(Policy = "RequireDataPlanner")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MOLTrackingHistoryCreateDto dto)
        {
            var user = JWT.GetUserInfo(HttpContext);

            // Map the DTO to the Model inside the controller
            var trackingHistory = new MOLTrackingHistoryModel
            {
                ID = Guid.NewGuid(),
                MOLID = dto.MOLID,
                WRCode = dto.WRCode,
                Status = dto.Status,
                AdditionalInfo = dto.AdditionalInfo,
                CreatedBy = user.username,
                CreatedAt = DateTime.Now,
                UpdatedBy = user.username,
                UpdatedAt = DateTime.Now,
            };

            // Call the service to handle the business logic
            await _molTrackingHistoryService.Create(trackingHistory);

            return CreatedAtAction(nameof(Create), new { id = trackingHistory.ID }, new
            {
                StatusCode = 201,
                Message = "MOL tracking history created successfully",
                Data = trackingHistory
            });
        }
    }
}
