using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worklog_api.Model
{
    [Table("LWO_Image")]
    public class LWOImageModel
    {
        [Key]
        public Guid ID { get; set; }

        [ForeignKey("LWOMetadataModel")]
        public Guid LWOMetadataID { get; set; }

        public string Path { get; set; }

        public string ImageName { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public LWOMetadataModel LWOMetadata { get; set; }
    }
}
