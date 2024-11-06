namespace worklog_api.Model
{
    public class ScheduleDetail
    {
        public Guid ID { get; set; }
        public Guid ScheduleID { get; set; }
        public DateTime PlannedDate { get; set; }
        public bool IsDone { get; set; }
        public Guid DailyID { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
