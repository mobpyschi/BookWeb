using Project_BookStoreCT.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Project_BookStoreCT.Models.ServiceModels
{
    public class LoginCus
    {
        public bool CheckCus(string email, string password, bool checkRemember)
        {
            using (DataContext db = new DataContext())
            {
                var checkCus = db.Customers.Where(x => x.customerEmail == email && x.password == password).FirstOrDefault();
                if (checkCus != null)
                {
                    if (checkRemember == true)
                    {
                        FormsAuthentication.SetAuthCookie(checkCus.Customer_ID.ToString(), true);
                    }
                    SessionCheckingCustomes.Session(checkCus.Customer_ID, checkCus.customerName, checkCus.avatar);
                    return true;
                }
                return false;
            }
        }

        
    }
}