//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace test.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class stok
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public stok()
        {
            this.adminkontrol = new HashSet<adminkontrol>();
        }

        public int id { get; set; }
        [Required(ErrorMessage = "Bo� B�rakmay�n")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> tarih { get; set; }
        public Nullable<int> kul_id { get; set; }
        public Nullable<int> malzeme_id { get; set; }
        public Nullable<int> tur_id { get; set; }
        public Nullable<int> tedarik_id { get; set; }
        public Nullable<int> birim_id { get; set; }
        [Required(ErrorMessage = "Bo� B�rakmay�n")]
        public Nullable<double> miktar { get; set; }
        public Nullable<int> isdisable { get; set; }
        public Nullable<double> kalanmiktar { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<adminkontrol> adminkontrol { get; set; }
        public virtual birim birim { get; set; }
        public virtual login login { get; set; }
        public virtual malzemelist malzemelist { get; set; }
        public virtual tedarikci tedarikci { get; set; }
        public virtual tur tur { get; set; }
    }
}