using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Project_BookStoreCT.Models.PostModels;
using Project_BookStoreCT.Models.ServiceModels;
using Project_BookStoreCT.Models.DataModels;
using Project_BookStoreCT.Models.ViewModels;
using System.IO;

namespace Project_BookStoreCT.Controllers
{
    public class CustomerController : Controller
    {
        // GET: Customer
        [HttpGet]
        public ActionResult Login()
        {
            if (Request.Cookies[".cus"] == null)
            {
                if (SessionCheckingCustomes.customerID == null)
                {
                    return View();
                }
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Login(LoginPost lgPost)
        {
            LoginCus loginCus = new LoginCus();
            if (loginCus.CheckCus(lgPost.email, Encode.CreateMD5(lgPost.password), lgPost.rememberMe) == true)
            {
                return Json(new { _mess = 1 });
            }
            else
            {
                return Json(new { _mess = 0 });
            }
        }


        [HttpGet]
        public ActionResult Registration()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetRoles = (from r in db.Roles select r).ToList();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Registration(CusPost cus)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetRoles = (from r in db.Roles select r).ToList();
                var checkEmailExist = (from u in db.Customers where u.customerEmail == cus.email select u).FirstOrDefault();
                
                if (checkEmailExist == null )
                {
                    
                        Customer u = new Customer();
                        u.customerName = cus.username;
                        u.customerEmail = cus.email;
                        u.customerPhone = cus.phone;
                        u.dayOfBirth = DateTime.Now;
                        u.role = 3;
                                //u.role = cus.role;
                        u.password = Encode.CreateMD5(cus.password);
                        u.sex = Convert.ToBoolean(cus.sex);
                    if (Convert.ToBoolean(cus.sex)==true)
                    {
                        u.avatar = "avt_men.jpg ";
                    }    
                    else
                    {
                        u.avatar = "avt_girl.jpg";
                    }    
                         u.customerAddress = cus.address;
                         db.Customers.Add(u);
                         db.SaveChanges();
                        

                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }

        }
        public ActionResult _PartialCustomerInformation()
        {
            if (Request.Cookies[".cus"] != null)
            {
                //Kiểm tra cookie lấy id của user để lấy thông tin user
                DataContext db = new DataContext();
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var getUserIDFromCookie = int.Parse(ticket.Name);
                var getCus = (from cus in db.Customers where cus.Customer_ID == getUserIDFromCookie select cus).FirstOrDefault();
                SessionCheckingCustomes.Session(getCus.Customer_ID, getCus.customerName, getCus.avatar);
                return PartialView("_PartialUserInformation");
            }
            return PartialView("_PartialCustomerInformation");
        }

        public ActionResult Logout ()
        {
            FormsAuthentication.SignOut();
            SessionCheckingCustomes.customerID = null;
            return Json(new { _mess = 1 });
        }

        public ActionResult Index()
        {
            using(DataContext db = new DataContext())
            {
                var cus = (from c in db.Customers
                           join r in db.Roles on c.role equals r.Role_ID where c.Customer_ID==SessionCheckingCustomes.customerID
                           select new
                           {
                               c.Customer_ID,
                               c.customerName,
                               c.customerAddress,
                               c.avatar,
                               c.customerEmail,
                               c.customerPhone,
                               c.sex,
                               r.roleName
                           }).ToList();
                List<CusIndex_ViewModels> customer = new List<CusIndex_ViewModels>();
                foreach (var c in cus)
                {
                    CusIndex_ViewModels ci = new CusIndex_ViewModels();
                    ci.cus_id = c.Customer_ID;
                    ci.cusName = c.customerName;
                    ci.cusAddress = c.customerAddress;
                    ci.avatar = c.avatar;
                    ci.cusEmail = c.customerEmail;
                    ci.cusPhone = c.customerPhone;
                    ci.sex = c.sex;
                    ci.role = c.roleName;
                    customer.Add(ci);
                }
                return View(customer);
            }
        }
        [HttpGet]
        public ActionResult UpdateCustomer(int ? cid)
        {
            using(DataContext db = new DataContext())
            {
                TempData["cid"] = cid;
                if (cid != null)
                {
                    ViewBag.GetInfoCus = (from c in db.Customers where c.Customer_ID == cid select c).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFound");
                }
            }    
        }

        [HttpPost]
        public ActionResult UpdateCustomer(CusPost cus)
        {
            using(DataContext db = new DataContext())
            {
                Customer c = db.Customers.Where(x => x.Customer_ID== cus.userid).FirstOrDefault();
                if (cus.username != null || cus.phone != null || cus.address != null )
                {
                    
                    c.customerName = cus.username;
                    c.customerPhone = cus.phone;
                    c.role = 3;
                    c.sex = Convert.ToBoolean(cus.sex);
                    if (Convert.ToBoolean(cus.sex) == true)
                    {
                        c.avatar = "avt_men.jpg ";
                    }
                    else
                    {
                        c.avatar = "avt_girl.jpg";
                    }
                    c.customerAddress = cus.address;
                    db.SaveChanges();
                    return Json(new { mess__ = 1 });
                }
                else
                {
                   
                    return Json(new { mess__ = 0 });
                }
            }
        }

        [HttpGet]
        public ActionResult ChangePass(int ? cid)
        {
            using (DataContext db = new DataContext())
            {
                TempData["cid"] = cid;
                if (cid != null)
                {
                    ViewBag.GetInfoCus = (from c in db.Customers where c.Customer_ID == cid select c).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFound");
                }
            }
        }
        [HttpPost]
        public ActionResult ChangePass(changePass cus)
        {
            using(DataContext db = new DataContext())
            {
                Customer c = db.Customers.Where(x => x.Customer_ID == cus.userid).FirstOrDefault();
                if (Encode.CreateMD5(cus.old_pass) == c.password)
                {
                    c.password = Encode.CreateMD5(cus.password);
                    db.SaveChanges();
                    return Json(new { mes_check = 1 });  
                }
                else
                {
                    return Json(new { mes_check = 0 });
                }
                

            }
        }
    }
}