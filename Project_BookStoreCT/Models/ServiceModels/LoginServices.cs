using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Project_BookStoreCT.Models.DataModels;

namespace Project_BookStoreCT.Models.ServiceModels
{
    public class LoginServices
    {
        public bool CheckLogin(string email ,string password, bool checkRemember)
        {
            using(DataContext db =new DataContext())
            {
                var checkLogin = db.Users.Where(x => x.email == email && x.password == password).FirstOrDefault();
                if (checkLogin != null)
                {
                    if (checkRemember == true)
                    {
                        FormsAuthentication.SetAuthCookie(checkLogin.User_ID.ToString(), true);
                    }
                    SessionCheckingServices.Session(checkLogin.User_ID, checkLogin.avatar, checkLogin.userName, checkLogin.password);
                    return true;
                }
                return false;
            }
        }
    }
}