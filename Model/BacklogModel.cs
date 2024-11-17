using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace worklog_api.Model
{
    public class BacklogModel
    {
 
        public Guid ID { get; set; }
        public DateTime? DateInspection { get; set; }
        public Guid CNID { get; set; }
        public string? ProblemDescription { get; set; }
        public string? Component { get; set; }
        public string? PartNumberRequired { get; set; }
        public string? Description { get; set; }
        public string? NoIndex { get; set; }
        public string? NoFigure { get; set; }
        public int? Quantity { get; set; }
        public string? PlanRepair { get; set; }
        public double? EstimateRepairHour { get; set; }
        public string? Status { get; set; }
        public Guid DailyDetailId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        // list image
        public List<BacklogImageModel> BacklogImages { get; set; }
    }
}
