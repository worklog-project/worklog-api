using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Service;

namespace worklog_api.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LWOCreateDto lwoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid data",
                    Errors = ModelState
                });

            var lwo = new LWOModel
            {
                ID = Guid.NewGuid(),
                WONumber = lwoDto.WONumber,
                WODate = DateTime.Now,
                WOType = lwoDto.WOType,
                Activity = lwoDto.Activity,
                HourMeter = lwoDto.HourMeter,
                TimeStart = lwoDto.TimeStart,
                TimeEnd = lwoDto.TimeEnd,
                PIC = lwoDto.PIC,
                LWOType = lwoDto.LWOType,
                Version = 1
            };

            await _lwoService.CreateLWO(lwo);

            return CreatedAtAction(nameof(GetById), new { id = lwo.ID }, new
            {
                StatusCode = 201,
                Message = "LWO created successfully",
                Data = lwo
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LWOCreateDto lwoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid data",
                    Errors = ModelState
                });

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
