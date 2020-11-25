using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class ThemesPost
    {
        public int theme_id { get; set; }
        public string tenTV { get; set; }
        public string nameEnglish { get; set; }
        public string description { get; set; }
    }
}