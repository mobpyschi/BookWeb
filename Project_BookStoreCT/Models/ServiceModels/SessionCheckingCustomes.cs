using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ServiceModels
{
    public class SessionCheckingCustomes
    {
        //Thông tin cần lấy của user khi đăng nhập 
        public static int? customerID { get; set; }
        public static string customerName { get; set; }
        public static string avatar { get; set; }
        public static void Session(int customerID, string customerName, string avatar)
        {
            SessionCheckingCustomes.customerID = customerID;
            SessionCheckingCustomes.customerName = customerName;
            SessionCheckingCustomes.avatar = avatar;
        }
    }
}