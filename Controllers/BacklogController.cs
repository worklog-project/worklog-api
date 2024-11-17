﻿using Microsoft.AspNetCore.Mvc;
using worklog_api.Service;
using worklog_api.Model;
using worklog_api.Model.dto;
using System.Text.Json;
using worklog_api.payload;

namespace worklog_api.Controllers
{

    [Route("api/backlog")]
    [ApiController]
    public class BacklogController : ControllerBase
    {
        public readonly ILogger<UserController> _logger;
        public readonly IBacklogService _backlogService;

        public BacklogController(IBacklogService backlogService)
        {
            _backlogService = backlogService;
        }
        [Route("create-backlog")]
        [HttpPost]
        public async Task<IActionResult> InsertBacklogAsync([FromForm] string backlogJson, [FromForm] BacklogImageDTO imageDTO)
        {
            try
            {
                if (imageDTO?.ImageFile == null || imageDTO.ImageFile.Length == 0)
                    return BadRequest("Image is required");

                // Deserialize backlog JSON
                var backlogDTO = JsonSerializer.Deserialize<BacklogDTO>(backlogJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (backlogDTO == null)
                    return BadRequest("Invalid backlog data");

                var result = await _backlogService.InsertBacklogAsync(backlogDTO, imageDTO);
                var response = new ApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "Success create Backlog",
                    Data = result
                };

                return Ok(response);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing backlog data");
                return BadRequest(new { error = "Invalid JSON format for backlog data" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InsertBacklogAsync");
                return BadRequest(new { error = "Failed to process the request" });
            }
        }

        [Route("detail-backlog")]
        [HttpGet]
        public async Task<IActionResult> GetBacklogDetail([FromQuery] Guid backlogId)
        {
            var backlog = await _backlogService.GetByIDAsync(backlogId);

            if (backlog == null)
                return NotFound("Backlog not found");

            var response = new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Success get Backlog detail",
                Data = backlog
            };

            return Ok(response);
        }
    }
}
