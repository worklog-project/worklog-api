using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worklog_api.Model
{
    [Table("LWO")]
    public class LWOModel
    {
        [Key]
        public Guid ID { get; set; }
        public string WONumber { get; set; }
        public DateTime WODate { get; set; }
        public string WOType { get; set; }
        public string Activity { get; set; }
        public int HourMeter { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string PIC { get; set; }
        public string LWOType { get; set; }
        public int Version { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<LWOMetadataModel>? Metadata { get; set; }
    }
}
