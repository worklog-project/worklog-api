namespace worklog_api.Model
{
    public class BacklogImageModel
    {
        public Guid ID { get; set; }
        public Guid BacklogId { get; set; }
        public string FormatType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
