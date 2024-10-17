using Microsoft.AspNetCore.Mvc;
using worklog_api.Service;
using worklog_api.Model;

namespace worklog_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MekanikController : ControllerBase
    {
        private readonly IMekanikService _mekanikService;

        public MekanikController(IMekanikService mekanikService)
        {
            _mekanikService = mekanikService;
        }

        [HttpGet("{id}")]
        public IActionResult GetMekanik(int id)
        {
            var mekanik = _mekanikService.GetMekanikById(id);
            if (mekanik == null)
            {
                return NotFound();
            }
            return Ok(mekanik);
        }

        [HttpPost]
        public IActionResult CreateMekanik([FromBody] MekanikModel mekanik)
        {
            if (mekanik == null)
            {
                return BadRequest();
            }

            _mekanikService.CreateMekanik(mekanik);
            return CreatedAtAction(nameof(GetMekanik), new { id = mekanik.Id }, mekanik);
        }
    }
}
