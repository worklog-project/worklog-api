using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using worklog_api.helper;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Service;

namespace worklog_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/lwo")]
    public class LWOController : ControllerBase
    {
        private readonly ILWOService _lwoService;

        public LWOController(ILWOService lwoService)
        {
            _lwoService = lwoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lwos = await _lwoService.GetAllLWOs();
            return Ok(new
            {
                StatusCode = 200,
                Message = "Success get LWOs",
                Data = lwos
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lwo = await _lwoService.GetLWOById(id);
            if (lwo == null)
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "LWO not found"
                });

            return Ok(new
            {
                StatusCode = 200,
                Message = "Success get LWO",
                Data = lwo
            });
        }

        [Route("create-lwo")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string lwoJson, [FromForm] IFormFileCollection images)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(lwoJson))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "LWO data is required"
                });
            }

            var user = JWT.GetUserInfo(HttpContext);

            // Forward to service layer with raw data
            try
            {
                var lwoDto = JsonConvert.DeserializeObject<LWOCreateDto>(lwoJson);
                lwoDto.CreatedBy = user.username;
                lwoDto.UpdatedBy = user.username;

                var createdLwo = await _lwoService.CreateLWO(lwoDto, images);

                return CreatedAtAction(nameof(GetById), new { id = createdLwo.ID }, new
                {
                    StatusCode = 201,
                    Message = "LWO created successfully",
                    Data = createdLwo
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Validation failed",
                    Errors = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while creating LWO",
                    Error = ex.Message
                });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LWOCreateDto lwoDto)
        {
            var existingLwo = await _lwoService.GetLWOById(id);
            if (existingLwo == null)
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "LWO not found"
                });

            existingLwo.WONumber = lwoDto.WONumber;
            existingLwo.WOType = lwoDto.WOType;
            existingLwo.Activity = lwoDto.Activity;
            existingLwo.HourMeter = lwoDto.HourMeter;
            existingLwo.TimeStart = lwoDto.TimeStart;
            existingLwo.TimeEnd = lwoDto.TimeEnd;
            existingLwo.PIC = lwoDto.PIC;
            existingLwo.LWOType = lwoDto.LWOType;
            existingLwo.Version = lwoDto.Version;

            await _lwoService.UpdateLWO(existingLwo);

            var updatedLwo = await _lwoService.GetLWOById(id);

            return Ok(new
            {
                StatusCode = 200,
                Message = "LWO updated successfully",
                Data = updatedLwo
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingLwo = await _lwoService.GetLWOById(id);
            if (existingLwo == null)
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "LWO not found"
                });

            await _lwoService.DeleteLWO(id);
            return Ok(new
            {
                StatusCode = 200,
                Message = "LWO deleted successfully",
                Data = existingLwo
            });
        }
    }
}
