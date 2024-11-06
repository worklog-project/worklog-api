namespace worklog_api.Model.dto
{
    public class ScheduleDTO
    {
        public Guid EGIID { get; set; }
        public Guid CNID { get; set; }
        public DateTime ScheduleMonth { get; set; }
        public List<ScheduleDetailDTO> ScheduleDetails { get; set; }
    }
    public class ScheduleDetailDTO
    {
        public String PlannedDate { get; set; }
    }
}
