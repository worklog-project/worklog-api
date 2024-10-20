namespace worklog_api.Model.dto
{
    public class MOLTrackingHistoryCreateDto
    {
        public Guid MOLID { get; set; }
        public string WRCode { get; set; }
        public string Status { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
