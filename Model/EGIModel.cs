using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace worklog_api.Model
{
    [Table("EGI")]
    public class EGIModel
    {
        [Key]
        public Guid ID { get; set; }
        public string EGI { get; set; }
    }

}