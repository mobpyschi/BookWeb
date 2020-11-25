using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class CategorysPost
    {
        public int category_id { get; set; }
        public string categoryTV { get; set; }
        public string categoryTA { get; set; }
        public int theme_id { get; set; }
    }
}