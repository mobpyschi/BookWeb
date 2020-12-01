using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ViewModels
{
    public class DetailBills_ViewModels
    {
        public string bookName { get; set; }
        public string image { get; set; }  
        public int ? quantity { get; set; }
        public string customerName { get; set; }
        public string phone { get; set; }
        public double total { get; set; }
        public double? price { get; set; }
        public int payment_method { get; set; }
        public double saleOffPrice { get; set; }
    }
}