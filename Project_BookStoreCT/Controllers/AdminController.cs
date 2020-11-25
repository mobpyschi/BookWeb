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
    public class AdminController : Controller
    {
        // GET: Admin
        [HttpGet]
        public ActionResult Login()
        {
            if (Request.Cookies[".ad"] == null)
            {
                if(SessionCheckingServices.userID==null)
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
            LoginServices loginServices = new LoginServices();
            if (loginServices.CheckLogin(lgPost.email, Encode.CreateMD5( lgPost.password) ,lgPost.rememberMe) == true)
            {
                return Json(new { _mess = 1 });
            }
            else
            {
                return Json(new { _mess = 0 });
            }
        }

        public ActionResult Logout()
        {
            //if (Request.Cookies[".ASPXAUTH"] != null)
            //{
            //    var cookie = new HttpCookie(".ASPXAUTH");
            //    cookie.Expires = DateTime.Now.AddDays(-1);
            //    Response.Cookies.Add(cookie);
            //}
            FormsAuthentication.SignOut();
            SessionCheckingServices.userID = null;
            return Json(new { _mess = 1 });
        }
        public ActionResult _PartialUserInformation()
        {
            if (Request.Cookies[".ad"] != null)
            {
                //Kiểm tra cookie lấy id của user để lấy thông tin user
                DataContext db = new DataContext();
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var getUserIDFromCookie = int.Parse(ticket.Name);
                var getUser = (from user in db.Users where user.User_ID == getUserIDFromCookie select user).FirstOrDefault();
                SessionCheckingServices.Session(getUser.User_ID, getUser.avatar,getUser.userName,getUser.password);
                return PartialView("_PartialUserInformation");
            }
            return PartialView("_PartialUserInformation");
        }

        public ActionResult Index()
        {
            if (Request.Cookies[".ad"] == null) 
            {
                if(SessionCheckingServices.userID == null)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                //Kiểm tra cookie lấy id của user để lấy thông tin user
                DataContext db = new DataContext();
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var getUserIDFromCookie = int.Parse(ticket.Name);
                var getUser = (from user in db.Users where user.User_ID == getUserIDFromCookie select user).FirstOrDefault();
                SessionCheckingServices.Session(getUser.User_ID, getUser.avatar,getUser.userName,getUser.password);
                return View();
            }
        }
        public ActionResult UsersIndex()
        {
            using (DataContext db = new DataContext())
            {
                var user = (from u in db.Users join r in db.Roles on u.role equals r.Role_ID select new
                           {
                            u.User_ID, u.userName, u.address, u.avatar, u.email, u.phoneNumber, u.sex, r.roleName
                           }).ToList();
                List<UserIndex_ViewModels> users = new List<UserIndex_ViewModels>();
                foreach(var u in user)
                {
                    UserIndex_ViewModels ui = new UserIndex_ViewModels();
                    ui.user_id = u.User_ID;
                    ui.username = u.userName;
                    ui.address = u.address;
                    ui.avatar = u.avatar;
                    ui.email = u.email;
                    ui.phone = u.phoneNumber;
                    ui.sex = u.sex;
                    ui.role = u.roleName;
                    users.Add(ui);
                }
                return View(users);
            }
        }
        [HttpGet]
        public ActionResult ChangePass(int? uid)
        {
            using (DataContext db = new DataContext())
            {
                TempData["uid"] = uid;
                if (uid != null)
                {
                    ViewBag.GetInfoUser = (from c in db.Users where c.User_ID == uid select c).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }
        }
        [HttpPost]
        public ActionResult ChangePass(changePass user)
        {
            using (DataContext db = new DataContext())
            {
                User c = db.Users.Where(x => x.User_ID == user.userid).FirstOrDefault();
                if (Encode.CreateMD5(user.old_pass) == c.password)
                {
                    c.password = Encode.CreateMD5(user.password);
                    db.SaveChanges();
                    return Json(new { mes_check = 1 });
                }
                else
                {
                    return Json(new { mes_check = 0 });
                }


            }
        }
            public ActionResult CustomersIndex()
        {
            using (DataContext db = new DataContext())
            {
                var cus = (from c in db.Customers
                            join r in db.Roles on c.role equals r.Role_ID
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
        public ActionResult AddNewUsers()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetRoles = (from r in db.Roles select r).ToList();
            }  
            return View();
        }
        [HttpPost]
        public ActionResult AddNewUsers(UsersPost user)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetRoles = (from r in db.Roles select r).ToList();
                var checkEmailExist = (from u in db.Users where u.email == user.email select u).FirstOrDefault();
                if (checkEmailExist == null) 
                {
                    if (user.avatar != null)
                    {
                        //save ảnh vào thư mục và lấy tên ảnh
                        var filename = Path.GetFileName(user.avatar.FileName);
                        var path = Path.Combine(Server.MapPath("../img/avatar"), filename);
                        user.avatar.SaveAs(path);

                        User u = new User();
                        u.userName = user.username;
                        u.email = user.email;
                        u.phoneNumber = user.phone;
                        u.role = user.role;
                        u.avatar = filename;
                        u.password = Encode.CreateMD5(user.password);
                        u.sex = Convert.ToBoolean(user.sex);
                        u.address = user.address;
                        u.dayOfBirth = DateTime.Now;
                        db.Users.Add(u);
                        db.SaveChanges();
                        return Json(new { mess_ = 1 });
                    }
                    else
                    {
                        User u = new User();
                        u.userName = user.username;
                        u.email = user.email;
                        u.phoneNumber = user.phone;
                        u.role = user.role;
                        u.avatar = "default-avatar.png";
                        u.password = Encode.CreateMD5(user.password);
                        u.sex = Convert.ToBoolean(user.sex);
                        u.address = user.address;
                        u.dayOfBirth = DateTime.Now;
                        db.Users.Add(u);
                        db.SaveChanges();
                        return Json(new { mess_ = 1 });
                    }
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
               
            }
        }
        [HttpGet]
        public ActionResult UpdateUsers(int ? uid)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetRoles = (from r in db.Roles select r).ToList();
                TempData["userid"] = uid;
                if (uid != null)
                {
                    ViewBag.GetInfoUser = (from u in db.Users where u.User_ID == uid select u).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }
        }
        [HttpPost]
        public ActionResult UpdateUsers(UsersPost user)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetRoles = (from r in db.Roles select r).ToList();
                User u = db.Users.Where(x => x.User_ID == user.userid).FirstOrDefault();
                if (user.avatar != null)
                {
                    //save ảnh vào thư mục và lấy tên ảnh
                    var filename = Path.GetFileName(user.avatar.FileName);
                    var path = Path.Combine(Server.MapPath("../img/avatar"), filename);
                    user.avatar.SaveAs(path);

                    u.userName = user.username;
                    u.phoneNumber = user.phone;
                    u.role = user.role;
                    u.avatar = filename;
                    u.sex = Convert.ToBoolean(user.sex);
                    u.address = user.address;
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    u.userName = user.username;
                    u.phoneNumber = user.phone;
                    u.role = user.role;       
                    u.sex = Convert.ToBoolean(user.sex);
                    u.address = user.address;
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
            }
        }
        public ActionResult DeleteUsers(int ? uid)
        {
            using (DataContext db = new DataContext())
            {
                if (uid != null)
                {
                    User u = db.Users.Find(uid);
                    db.Users.Remove(u);
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }
        }
        public ActionResult BooksIndex()
        {
            using(DataContext db =new DataContext())
            {
                List<BooksIndex_ViewModels> books = new List<BooksIndex_ViewModels>();
                var listBooks = (from b in db.Books join c in db.Categories 
                                 on b.category_id equals c.Category_ID
                                 join s in db.Suppliers 
                                 on b.supplier_id equals s.Supplier_ID
                                 join l in db.Languages 
                                 on b.language_id equals l.Language_ID
                                 select new {
                                     b.Book_ID,
                                     b.bookName,
                                     b.price,
                                     b.image,
                                     b.saleOffPrice,
                                     b.statusSaleOff,
                                     b.star,
                                     b.quantityExists,
                                     b.sellNumber,
                                     c.categoryName,
                                     s.supplierName,
                                     l.languageName }).ToList();
                foreach(var b in listBooks)
                {
                    BooksIndex_ViewModels book = new BooksIndex_ViewModels();
                    book.book_id = b.Book_ID;
                    book.bookName = b.bookName;
                    book.price = b.price;
                    book.image = b.image;
                    book.priceSaleOff = b.saleOffPrice;
                    book.statusSaleOff = b.statusSaleOff;
                    book.star = b.star;
                    book.quantityExist = b.quantityExists;
                    book.sellNumber = b.sellNumber;
                    book.categoryName = b.categoryName;
                    book.supplierName = b.supplierName;
                    book.language = b.languageName;
                    books.Add(book);
                }
                return View(books);
            }
        }
        public ActionResult ThemesIndex()
        {
            using(DataContext db =new DataContext())
            {
                ViewBag.GetThemes = (from t in db.Themes select t).ToList();
                ViewBag.check = (from t in db.Themes join c in db.Categories on t.Theme_ID equals c.theme_id select c).ToList();
                return View();
            }
        }
        public ActionResult AddThemes(ThemesPost themes)
        {
            using (DataContext db = new DataContext())
            {
                Theme theme = new Theme();
                theme.themeName = themes.tenTV;
                theme.description = themes.description;
                db.Themes.Add(theme);
                db.SaveChanges();
                return Json(new { mess_ = 1 });
            }
        }
        public ActionResult DeleteThemes(int ? tid)
        {
            using (DataContext db = new DataContext())
            {
                if (tid != null)
                {
                    Theme theme = db.Themes.Find(tid);
                    db.Themes.Remove(theme);
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        [HttpGet]
        public ActionResult UpdateThemes(int ? tid)
        {
            using (DataContext db = new DataContext())
            {
                if (tid != null)
                {
                    ViewBag.getTheme = (from t in db.Themes where t.Theme_ID == tid select t).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }    
        }
        [HttpPost]
        public ActionResult UpdateThemes(ThemesPost themes)
        {
            using (DataContext db = new DataContext())
            {
                Theme theme = db.Themes.Where(x => x.Theme_ID == themes.theme_id).FirstOrDefault();
                if (theme != null)
                {
                    theme.themeName = themes.tenTV;
                    theme.description = themes.description;
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                return Json(new { mess_ = 0 });
            }
        }
        [HttpGet]
        public ActionResult AddBooks()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetCategory = (from c in db.Categories select c).ToList();
                ViewBag.GetSupplier = (from s in db.Suppliers select s).ToList();
                ViewBag.GetLanguage = (from l in db.Languages select l).ToList();
                return View();
            }
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult AddBooks(BooksPost book)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetCategory = (from c in db.Categories select c).ToList();
                ViewBag.GetSupplier = (from s in db.Suppliers select s).ToList();
                ViewBag.GetLanguage = (from l in db.Languages select l).ToList();
                if (ModelState.IsValid)
                {
                    Book b = new Book();

                    var filename = Path.GetFileName(book.image.FileName);
                    var path = Path.Combine(Server.MapPath("../imgs/"), filename);
                    book.image.SaveAs(path);

                    b.bookName = book.bookname;
                    b.description = book.description;
                    b.price = book.price;
                    b.quantityExists = book.number;
                    b.category_id = book.category;
                    b.supplier_id = book.supplier;
                    b.language_id = book.language;
                    b.image = filename;
                    b.statusSaleOff = false;
                    db.Books.Add(b);
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        [HttpGet]
        public ActionResult UpdateBooks(int ? bid)
        {
            if (bid != null)
            {
                using (DataContext db = new DataContext())
                {
                    ViewBag.GetCategory = (from c in db.Categories select c).ToList();
                    ViewBag.GetSupplier = (from s in db.Suppliers select s).ToList();
                    ViewBag.GetLanguage = (from l in db.Languages select l).ToList();
                    ViewBag.GetBooks = (from b in db.Books where b.Book_ID == bid select b).ToList();
                    return View();
                }
            }
            else
            {
                return PartialView("_Partial404NotFoundAdmin");
            }
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateBooks(BooksPost book)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetCategory = (from c in db.Categories select c).ToList();
                ViewBag.GetSupplier = (from s in db.Suppliers select s).ToList();
                ViewBag.GetLanguage = (from l in db.Languages select l).ToList();
                if (ModelState.IsValid)
                {
                    Book b = db.Books.Where(x => x.Book_ID == book.book_id).FirstOrDefault();
                    if (b != null)
                    {
                        if (book.image == null)
                        {
                            b.bookName = book.bookname;
                            b.price = book.price;
                            b.quantityExists = book.number;
                            b.language_id = book.language;
                            b.category_id = book.category;
                            b.supplier_id = book.supplier;
                            b.description = book.description;
                            db.SaveChanges();
                            return Json(new { mess_ = 1 });
                        }
                        else
                        {
                            var filename = Path.GetFileName(book.image.FileName);
                            var path = Path.Combine(Server.MapPath("../imgs/"), filename);
                            book.image.SaveAs(path);

                            b.bookName = book.bookname;
                            b.price = book.price;
                            b.quantityExists = book.number;
                            b.language_id = book.language;
                            b.category_id = book.category;
                            b.supplier_id = book.supplier;
                            b.description = book.description;
                            b.image = filename;
                            db.SaveChanges();
                            return Json(new { mess_ = 1 });

                        }
                    }
                    else
                    {
                        return Json(new { mess_ = 0 });
                    }              
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        public ActionResult DeleteBooks(int ? bid)
        {
            using (DataContext db = new DataContext())
            {
                if (bid != null)
                {
                    Book book = db.Books.Find(bid);
                    db.Books.Remove(book);
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        [HttpGet]
        public ActionResult SaleOffBooks(int ? bid)
        {
            using (DataContext db = new DataContext())
            {
                if (bid != null)
                {
                    ViewBag.GetBook = (from b in db.Books where b.Book_ID ==  bid select b).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }
        }
        [HttpPost]
        public ActionResult SaleOffBooks(SaleOffPost sale)
        {
            using (DataContext db = new DataContext())
            {
                if (sale.book_id != null)
                {
                    Book b = db.Books.Where(x => x.Book_ID == sale.book_id).FirstOrDefault();
                    if (b != null)
                    {
                        b.statusSaleOff = Convert.ToBoolean(sale.statusSaleOff);
                        b.saleOffPrice = sale.priceSaleOff;
                        db.SaveChanges();
                        return Json(new { mess_ = 1 });
                    }
                    return Json(new { mess_ = 0 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        public ActionResult CategoriesIndex()
        {
            using (DataContext db = new DataContext())
            {
                List<CategorysIndex_ViewModels> Categories = new List<CategorysIndex_ViewModels>();
                var getcates = (from c in db.Categories join t in db.Themes 
                                on c.theme_id equals t.Theme_ID
                                select new {
                                    c.Category_ID,
                                    c.categoryName,
                                    c.categoryNameForeign,
                                    t.themeName }).ToList();
                foreach(var item in getcates)
                {
                    CategorysIndex_ViewModels category = new CategorysIndex_ViewModels();
                    category.category_id = item.Category_ID;
                    category.category_nameTV = item.categoryName;
                    category.category_nameTA = item.categoryNameForeign;
                    category.theme_name = item.themeName;
                    Categories.Add(category);
                }
                return View(Categories);
            }
        }
        [HttpGet]
        public ActionResult AddCategorys()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetThemes = (from t in db.Themes select t).ToList();
                return View();
            }
        }
        [HttpPost]
        public ActionResult AddCategorys(CategorysPost category)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetThemes = (from t in db.Themes select t).ToList();
                if (ModelState.IsValid)
                {
                    Category c = new Category();
                    c.theme_id = category.theme_id;
                    c.categoryName = category.categoryTV;
                    c.categoryNameForeign = category.categoryTA;
                    db.Categories.Add(c);
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        [HttpGet]
        public ActionResult UpdateCategorys(int ? cid)
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetThemes = (from t in db.Themes select t).ToList();
                if (cid != null)
                {
                    ViewBag.getCategory = (from c in db.Categories where c.Category_ID == cid select c).ToList();
                    return View();
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }
        }
        [HttpPost]
        public ActionResult UpdateCategorys(CategorysPost category)
        {
            using (DataContext db = new DataContext())
            {
                Category c = db.Categories.Where(x => x.Category_ID == category.category_id).FirstOrDefault();
                if (c != null)
                {
                    c.categoryName = category.categoryTV;
                    c.categoryNameForeign = category.categoryTA;
                    c.theme_id = category.theme_id;               
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                return Json(new { mess_ = 0 });
            }
        }
        public ActionResult DeleteCategorys(int ? cid)
        {
            using (DataContext db = new DataContext())
            {
                if (cid != null)
                {
                    Category category = db.Categories.Find(cid);
                    db.Categories.Remove(category);
                    db.SaveChanges();
                    return Json(new { mess_ = 1 });
                }
                else
                {
                    return Json(new { mess_ = 0 });
                }
            }
        }
        public ActionResult BillsIndex()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.Bills = (from b in db.Bills select b).ToList();
                return View();
            }
        }
        public ActionResult DeleteBills(int ? bid)
        {
            using (DataContext db = new DataContext())
            {
                var item = (from d in db.DetailBills select d).ToList();
                foreach(var detail in item)
                {
                    if (detail.Book_ID == bid)
                    {
                        db.DetailBills.Remove(detail);
                        db.SaveChanges();
                    }  
                }
                Bill bill = db.Bills.Find(bid);
                db.Bills.Remove(bill);
                db.SaveChanges();
                return Json(new { mess_ = 1 });
            }
        }
        public ActionResult DetailBills(int ? bid)
        {
            using (DataContext db = new DataContext())
            {              
                if (bid != null)
                {
                    var detailBill = (from d in db.DetailBills
                                      join b in db.Books
                                      on d.Book_ID equals b.Book_ID
                                      where d.Bill_ID == bid
                                      select new { d.quantity ,b.price , b.bookName, b.image }).ToList();
                    List<DetailBills_ViewModels> details = new List<DetailBills_ViewModels>();
                    foreach(var d in detailBill)
                    {
                        DetailBills_ViewModels detail = new DetailBills_ViewModels();
                        detail.bookName = d.bookName;
                        detail.image = d.image;                
                        detail.quantity = d.quantity;
                        details.Add(detail);
                    }
                    return View(details);
                }
                else
                {
                    return PartialView("_Partial404NotFoundAdmin");
                }
            }
        }
        [HttpGet]
        public ActionResult ViewRevenue()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ViewRevenue(DateTime from_date, DateTime to_date)
        {
            using(DataContext db=new DataContext())
            {
                ViewBag.GetBills = (from b in db.Bills where b.date_set >= from_date && b.date_set <= to_date && b.isPayment == true select b).ToList();
                ViewBag.GetQuantityOrder = (from b in db.Bills where b.date_set >= from_date && b.date_set <= to_date && b.isPayment == true select b.Bill_ID).Count();
                ViewBag.SumToTal = (from b in db.Bills where b.date_set >= from_date && b.date_set <= to_date && b.isPayment == true select b.total).Sum();
                return View();
            }       
        }
    }
}