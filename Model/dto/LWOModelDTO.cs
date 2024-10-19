namespace worklog_api.Model.dto
{
    public class LWOCreateDto
    {
        public string WONumber { get; set; }
        public string WOType { get; set; }
        public string Activity { get; set; }
        public int HourMeter { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string PIC { get; set; }
        public string LWOType { get; set; }
        public int Version { get; set; }
    }

}
