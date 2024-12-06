namespace worklog_api.Model.dto
{
    public class LWOImageCreateDto
    {
        public string Path { get; set; }
        public string ImageName { get; set; }
    }

    public class LWOMetadataCreateDto
    {
        public string Komponen { get; set; }
        public string Keterangan { get; set; }
        public string KodeUnit { get; set; }
        public List<String> ImagesName { get; set; }
        public int Version { get; set; }
        public List<LWOImageCreateDto> LWOImages { get; set; }
    }

    public class LWOCreateDto
    {
        public string WONumber { get; set; }
        public DateTime WODate { get; set; }
        public string WOType { get; set; }
        public string Activity { get; set; }
        public int HourMeter { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string PIC { get; set; }
        public string LWOType { get; set; }
        public string KodeUnit { get; set; }
        public int Version { get; set; }
        public List<LWOMetadataCreateDto> Metadatas { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

    }
}
