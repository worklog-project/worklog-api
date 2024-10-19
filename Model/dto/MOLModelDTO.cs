namespace worklog_api.Model.dto
{
    public class MOLCreateDTO
    {
        public string KodeNumber { get; set; }
        public DateTime Tanggal { get; set; }
        public string WorkOrder { get; set; }
        public int HourMeter { get; set; }
        public string KodeKomponen { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Categories { get; set; }
        public string Remark { get; set; }
        public string RequestBy { get; set; }
        public string Status { get; set; }
    }

    public class MOLUpdateDTO
    {
        public Guid ID { get; set; }
        public string KodeNumber { get; set; }
        public DateTime Tanggal { get; set; }
        public string WorkOrder { get; set; }
        public int HourMeter { get; set; }
        public string KodeKomponen { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Categories { get; set; }
        public string Remark { get; set; }
        public string RequestBy { get; set; }
        public string Status { get; set; }
        public int Version { get; set; }
    }

}
