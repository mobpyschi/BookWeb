using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class BooksPost
    {
        public HttpPostedFileBase image { get; set; }
        public string description { get; set; }
        public string bookname { get; set; }
        public int price { get; set; }
        public int number { get; set; }
        public int category { get; set; }
        public int supplier { get; set; }
        public int language { get; set; }
        public int book_id { get; set; }
        public int quantityExist { get; set; }
    }
}