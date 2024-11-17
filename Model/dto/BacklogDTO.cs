namespace worklog_api.Model.dto
{
    public class BacklogDTO
    {
        public DateTime DateOfInspection { get; set; }
        public Guid CodeNumber { get; set; }
        public string? ProblemDescription { get; set; }
        public string? Component { get; set; }
        public string? PartNumberRequired { get; set; }
        public string? Description { get; set; }
        public string? NoFigure { get; set; }
        public string? NoIndex { get; set; }
        public Int32? Qty { get; set; }
        public string? PlanRepair { get; set; }
        public double? EstimateRepairHour { get; set; }
        public string? Status { get; set; }
        public Guid DailyDetailId { get; set; }
        public string? CreatedBy { get; set; }

    }
}
