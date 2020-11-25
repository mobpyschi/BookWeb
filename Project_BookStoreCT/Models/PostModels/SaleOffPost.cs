using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class SaleOffPost
    {
        public int ? book_id { get; set; }
        public int statusSaleOff { get; set; }
        public int priceSaleOff { get; set; }
    }
}