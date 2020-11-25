using Project_BookStoreCT.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.ServiceModels
{
    public class SessionCheckingServices
    {
        //Thông tin cần lấy của user khi đăng nhập 
        public static int ? userID { get; set; }
        public static string userName { get; set; }
        public static string avatar { get; set; }
        public static string password { get; set; }
        public static void Session(int userID, string avatar, string username, string password)
        {
            SessionCheckingServices.userID = userID;
            SessionCheckingServices.avatar = avatar;
            SessionCheckingServices.userName = username;
            SessionCheckingServices.password = password;
        }
    }
     

}