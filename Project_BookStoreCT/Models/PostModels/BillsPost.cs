using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class BillsPost
    {
        public int Bill_ID { get; set; }
        public int Customer_ID { get; set; }
        public string customerName { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public int payment_method { get; set; }
        public int total { get; set; }
    }
}