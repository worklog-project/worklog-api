using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Service;

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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MOLTrackingHistoryCreateDto dto)
        {
            // Validate the incoming request
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid data",
                    Errors = ModelState
                });
            }

            // Map the DTO to the Model inside the controller
            var trackingHistory = new MOLTrackingHistoryModel
            {
                ID = Guid.NewGuid(),
                MOLID = dto.MOLID,
                WRCode = dto.WRCode,
                Status = dto.Status,
                AdditionalInfo = dto.AdditionalInfo
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
