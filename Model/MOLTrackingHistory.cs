using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worklog_api.Model
{
    [Table("mol_tracking_history")]
    public class MOLTrackingHistoryModel
    {
        [Key]
        public Guid ID { get; set; }
        public Guid MOLID { get; set; }
        public string WRCode { get; set; }
        public string Status { get; set; }
        public string AdditionalInfo { get; set; }

        // Navigation Property
        public MOLModel MOL { get; set; }
    }

}
