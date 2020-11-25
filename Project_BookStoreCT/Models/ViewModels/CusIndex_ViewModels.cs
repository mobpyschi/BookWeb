using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ViewModels
{
    public class CusIndex_ViewModels
    {
        public int cus_id { get; set; }
        public string cusName { get; set; }
        public string avatar { get; set; }
        public string cusEmail { get; set; }
        public string cusPhone { get; set; }
        public string cusAddress { get; set; }
        public string role { get; set; }
        public bool? sex { get; set; }
    }
}