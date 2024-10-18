using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.Service;

namespace worklog_api.Controllers
{
    [Route("api/mol")]
    [ApiController]
    public class MOLController : ControllerBase
    {
        private readonly IMOLService _molService;

        public MOLController(IMOLService molService)
        {
            _molService = molService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var mols = await _molService.GetAllMOLs();
            return Ok(mols);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var mol = await _molService.GetMOLById(id);
            if (mol == null)
                return NotFound();

            return Ok(mol);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MOLCreateDTO molDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map the DTO to the MOLModel
            var mol = new MOLModel
            {
                ID = Guid.NewGuid(),
                KodeNumber = molDto.KodeNumber,
                Tanggal = molDto.Tanggal,
                WorkOrder = molDto.WorkOrder,
                HourMeter = molDto.HourMeter,
                KodeKomponen = molDto.KodeKomponen,
                PartNumber = molDto.PartNumber,
                Description = molDto.Description,
                Quantity = molDto.Quantity,
                Categories = molDto.Categories,
                Remark = molDto.Remark,
                RequestBy = molDto.RequestBy,
                Status = molDto.Status,
                StatusHistories = new List<StatusHistoryModel>(),
                TrackingHistories = new List<MOLTrackingHistoryModel>()
            };

            await _molService.CreateMOL(mol);
            return CreatedAtAction(nameof(GetById), new { id = mol.ID }, mol);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MOLModel mol)
        {
            if (id != mol.ID)
                return BadRequest();

            await _molService.UpdateMOL(mol);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _molService.DeleteMOL(id);
            return NoContent();
        }
    }
}
