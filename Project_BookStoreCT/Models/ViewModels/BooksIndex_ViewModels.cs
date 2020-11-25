using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ViewModels
{
    public class BooksIndex_ViewModels
    {
        public int book_id { get; set; }
        public string bookName { get; set; }
        public double ? price { get; set; }
        public int ? quantityExist { get; set; }
        public string image { get; set; }
        public int ? sellNumber { get; set; }
        public double ? priceSaleOff { get; set; }
        public bool ? statusSaleOff { get; set; }
        public string language { get; set; }
        public string categoryName { get; set; }
        public int ? star { get; set; }
        public string supplierName { get; set; }
    }
}