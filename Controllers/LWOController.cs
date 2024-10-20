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

            // Mapping LWO DTO to Model
            var lwo = new LWOModel
            {
                ID = Guid.NewGuid(),
                WONumber = lwoDto.WONumber,
                WODate = DateTime.Now,  // assuming WODate is the current date, you can modify this if needed
                WOType = lwoDto.WOType,
                Activity = lwoDto.Activity,
                HourMeter = lwoDto.HourMeter,
                TimeStart = lwoDto.TimeStart,
                TimeEnd = lwoDto.TimeEnd,
                PIC = lwoDto.PIC,
                LWOType = lwoDto.LWOType,
                Version = lwoDto.Version,
                Metadata = lwoDto.Metadata?.Select(metaDto => new LWOMetadataModel
                {
                    ID = Guid.NewGuid(),
                    LWOID = Guid.NewGuid(),  // this will be set when saving to the DB
                    Komponen = metaDto.Komponen,
                    Keterangan = metaDto.Keterangan,
                    KodeUnit = metaDto.KodeUnit,
                    Version = metaDto.Version,
                    Images = metaDto.LWOImages?.Select(imgDto => new LWOImageModel
                    {
                        ID = Guid.NewGuid(),
                        Path = imgDto.Path,
                        ImageName = imgDto.ImageName
                    }).ToList()
                }).ToList()
            };

            // Service call to create LWO along with metadata and images
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
