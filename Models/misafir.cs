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

    public partial class misafir
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public misafir()
        {
            this.guvenlikkontrol = new HashSet<guvenlikkontrol>();
        }

        public int id { get; set; }
        public string username { get; set; }
        public string ad { get; set; }
        public string soyad { get; set; }
        public string tc { get; set; }
        public string telefon { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy HH:mm}")]
        public Nullable<System.DateTime> girissaat { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy HH:mm}")]
        public Nullable<System.DateTime> cikissaat { get; set; }
        public string sebep { get; set; }
        public Nullable<double> sicaklik { get; set; }
        public string plaka { get; set; }
        public Nullable<int> isdisable { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<guvenlikkontrol> guvenlikkontrol { get; set; }
    }
}
