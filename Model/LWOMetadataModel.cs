using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace worklog_api.Model
{
    [Table("LWO_Metadata")]
    public class LWOMetadataModel
    {
        [Key]
        public Guid ID { get; set; }

        [ForeignKey("LWOModel")]
        public Guid LWOID { get; set; }

        public string Komponen { get; set; }

        public string Keterangan { get; set; }

        public string KodeUnit { get; set; }

        public int Version { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public LWOModel LWO { get; set; }

        public ICollection<LWOImageModel>? Images { get; set; }
    }
}
