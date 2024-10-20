using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using worklog_api.Model;
using worklog_api.Model.dto;
using worklog_api.payload;
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
            var mols = (await _molService.GetAllMOLs()).ToList();
            var response = new ApiResponse<List<MOLModel>>
            {
                StatusCode = 200,
                Message = "Success get MOLs",
                Data = mols
            };
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var mol = await _molService.GetMOLById(id);
            if (mol == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "MOL not found",
                    Data = null
                });
            }

            var response = new ApiResponse<MOLModel>
            {
                StatusCode = 200,
                Message = "Success get MOL",
                Data = mol
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MOLCreateDTO molDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "Invalid data",
                    Data = ModelState
                });
            }

            var mol = new MOLModel
            {
                ID = Guid.NewGuid(),
                KodeNumber = molDto.KodeNumber,
                Tanggal = DateTime.Now,
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
                Version = 1,
                StatusHistories = new List<StatusHistoryModel>(),
                TrackingHistories = new List<MOLTrackingHistoryModel>()
            };

            await _molService.CreateMOL(mol);

            var response = new ApiResponse<MOLModel>
            {
                StatusCode = 201,
                Message = "MOL created successfully",
                Data = mol
            };

            return CreatedAtAction(nameof(GetById), new { id = mol.ID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MOLModel mol)
        {
            if (id != mol.ID)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "ID mismatch",
                    Data = null
                });
            }

            var existingMol = await _molService.GetMOLById(id);
            if (existingMol == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "MOL not found",
                    Data = null
                });
            }

            await _molService.UpdateMOL(mol);
            var updatedMol = await _molService.GetMOLById(id);

            var response = new ApiResponse<MOLModel>
            {
                StatusCode = 200,
                Message = "MOL updated successfully",
                Data = updatedMol
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existingMol = await _molService.GetMOLById(id);
            if (existingMol == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "MOL not found",
                    Data = null
                });
            }

            await _molService.DeleteMOL(id);

            var response = new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "MOL deleted successfully",
                Data = existingMol
            };

            return Ok(response);
        }
    }
}
