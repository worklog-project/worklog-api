using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worklog_api.Model
{
    [Table("Status_History")]
    public class StatusHistoryModel
    {
        [Key]
        public Guid ID { get; set; }
        public Guid MOLID { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        // Navigation Property
        public MOLModel MOL { get; set; }
    }

}
