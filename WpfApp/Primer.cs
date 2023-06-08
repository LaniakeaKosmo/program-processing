namespace WpfApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Primer")]
    public partial class Primer
    {
        [Key]
        [StringLength(255)]
        public string Fio { get; set; }

        [StringLength(255)]
        public string Fio_d { get; set; }

        public int? Numberib { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date_gosp { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Date_vipis { get; set; }

        [StringLength(10)]
        public string Otdel { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public int? Type_gosp { get; set; }

        [StringLength(255)]
        public string Polic { get; set; }

        [StringLength(255)]
        public string Type_pay { get; set; }
    }
}
