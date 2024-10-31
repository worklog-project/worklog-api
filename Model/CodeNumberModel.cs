using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace worklog_api.Model
{
    [Table("EGI_Code_Number")]
    public class CodeNumberModel
    {
        [Key]
        public Guid ID { get; set; }
        public Guid EGIID { get; set; }
        public string CodeNumber { get; set; }

    }

}