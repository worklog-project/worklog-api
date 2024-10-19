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

        // Navigation properties
        public LWOMetadataModel LWOMetadata { get; set; }
    }
}
