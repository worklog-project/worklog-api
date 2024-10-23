using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worklog_api.Model
{
    [Table("MOL_Tracking_History")]
    public class MOLTrackingHistoryModel
    {
        [Key]
        public Guid ID { get; set; }
        public Guid MOLID { get; set; }
        public string? WRCode { get; set; }
        public string Status { get; set; }
        public string? AdditionalInfo { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation Property
        public MOLModel MOL { get; set; }
    }

}
