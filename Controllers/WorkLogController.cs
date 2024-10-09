using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using worklog_api.model;


namespace worklog_api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("v1/api/worklog")]
    public class WorkLogController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WorkLogController> _logger;

        public WorkLogController(ILogger<WorkLogController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetListWorkLog")]
        public IEnumerable<WorkLogModel> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WorkLogModel
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
