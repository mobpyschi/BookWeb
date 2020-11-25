using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ViewModels
{
    public class UserIndex_ViewModels
    {
        public int user_id { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string role { get; set; }
        public bool ? sex { get; set; }
        
    }
}