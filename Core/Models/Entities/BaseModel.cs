using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models.Entities
{
    public class BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClusterID { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Timestamp]
        public byte[] RowGuid { get; set; }

        public string ModPerson { get; set; }

        [Display(Name = "Data dodania")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh\\:mm}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "DateTime2")]
        public DateTime? CreationTime { get; set; }

        [Display(Name = "Data modyfikacji")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd hh\\:mm}", ApplyFormatInEditMode = true)]
        [Column(TypeName = "DateTime2")]
        public DateTime? ModifiedTime { get; set; }
    }
}
