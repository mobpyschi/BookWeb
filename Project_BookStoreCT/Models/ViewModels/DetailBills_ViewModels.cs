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
    }
}