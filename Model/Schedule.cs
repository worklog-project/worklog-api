namespace worklog_api.Model
{
    public class Schedule
    {
        public Guid ID { get; set; }
        public Guid EGIID { get; set; }
        public string Egi { get; set; }
        public Guid CNID { get; set; }
        public string CodeNumber { get; set; }
        public DateTime ScheduleMonth { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ScheduleDetail> ScheduleDetails { get; set; }
    }
}
