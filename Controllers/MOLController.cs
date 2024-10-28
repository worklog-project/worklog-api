using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using worklog_api.Model.dto;
using worklog_api.Model;
using worklog_api.payload;
using worklog_api.Service;
using worklog_api.error;
using worklog_api.helper;
using Humanizer;

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

        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "Request_By",
            [FromQuery] string sortDirection = "ASC",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string requestBy = null,
            [FromQuery] string status = null)  // New status parameter
        {
            var (mols, totalCount) = await _molService.GetAllMOLs(pageNumber, pageSize, sortBy, sortDirection, startDate, endDate, requestBy, status);

            var response = new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Success get MOLs",
                Data = new
                {
                    TotalCount = totalCount,
                    Mols = mols
                }
            };

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var mol = await _molService.GetMOLById(id);
            if (mol == null)
            {
                return NotFound(new NotFoundException("Mol Not Found"));
            }

            var response = new ApiResponse<MOLModel>
            {
                StatusCode = 200,
                Message = "Success get MOL",
                Data = mol
            };
            return Ok(response);
        }

        [Authorize(Policy = "RequireMekanik")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MOLCreateDTO molDto)
        {
            var user = JWT.GetUserInfo(HttpContext);

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
                RequestBy = user.username,
                Status = molDto.Status,
                Version = 1,
                CreatedAt = DateTime.Now,
                CreatedBy = user.username,
                UpdatedAt = DateTime.Now,
                UpdatedBy = user.username,
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MOLModel mol)
        {
            var user = JWT.GetUserInfo(HttpContext);

            if (id != mol.ID)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "ID mismatch",
                    Data = null
                });
            }

            mol.UpdatedAt = DateTime.Now;
            mol.UpdatedBy = user.username;

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

        [Authorize]
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

        [Authorize]
        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] StatusHistoryDTO StatusHistory)
        {

            var user = JWT.GetUserInfo(HttpContext);

            var quantityApproved = StatusHistory.QuantityApproved;

            var status = new StatusHistoryModel
            {
                ID = Guid.NewGuid(),
                MOLID = StatusHistory.MOLID,
                Remark = StatusHistory.Remark,
                Version = 1,
                CreatedAt = DateTime.Now,
                CreatedBy = user.username,
                UpdatedAt = DateTime.Now,
                UpdatedBy = user.username
            };

            await _molService.ApproveMOL(status, user, quantityApproved);

            var response = new ApiResponse<string>
            {
                StatusCode = 200,
                Message = "MOL approved successfully",
                Data = "Approved"
            };

            return Ok(response);
        }
    }
}
