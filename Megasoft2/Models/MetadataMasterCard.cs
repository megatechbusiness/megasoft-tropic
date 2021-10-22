using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class HeaderMetadata
    {
        
        public string ProductDescription { get; set; }
        public string PrintDescription { get; set; }
        [StringLength(15)]
        [Display(Name = "Customer")]
        public string Customer { get; set; }

        public string Contact { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public Nullable<decimal> Width { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<decimal> LeftGusset { get; set; }
        public Nullable<decimal> RightGusset { get; set; }
        public Nullable<decimal> TopGusset { get; set; }
        public Nullable<decimal> BottomGusset { get; set; }
        public Nullable<decimal> Lip { get; set; }
        public Nullable<decimal> Micron { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> Weight { get; set; }
        public bool Extrusion { get; set; }
        public bool Printing { get; set; }
        public bool Slitting { get; set; }
        public bool Bagging { get; set; }
        public bool Lamination { get; set; }
        public bool Other { get; set; }
        public Nullable<System.DateTime> DateSaved { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<int> Status { get; set; }
        public bool Wicketing { get; set; }
    }
}