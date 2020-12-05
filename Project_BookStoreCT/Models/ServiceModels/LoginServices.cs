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
                var roles = (from u in db.Users where u.email == email select new { u.role}).FirstOrDefault();
                var checkLogin = db.Users.Where(x => x.email == email && x.password == password && x.role== roles.role).FirstOrDefault();
                if (checkLogin != null)
                {
                   

                    if (checkRemember == true)
                    {
                        FormsAuthentication.SetAuthCookie(checkLogin.User_ID.ToString(), true);
                    }
                    SessionCheckingServices.Session(checkLogin.User_ID, checkLogin.avatar, checkLogin.userName, checkLogin.password,checkLogin.role);
                    return true;
                }
                return false;
            }
        }
    }
}