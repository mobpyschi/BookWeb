using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using Project_BookStoreCT.Models.DataModels;
using Project_BookStoreCT.Models.PostModels;
using Project_BookStoreCT.Models.ServiceModels;
using Project_BookStoreCT.Models.ViewModels;
using DataContext = Project_BookStoreCT.Models.DataModels.DataContext;

namespace Project_BookStoreCT.Controllers
{
    public class HomeController : Controller
    {
        //Trang chủ
        public ActionResult Index()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetAllBooks = (from b in db.Books select b).ToList();
                ViewBag.GetAllBooksSaleOff = (from b in db.Books where b.statusSaleOff == true select b).ToList();
                ViewBag.GetAllBooksHighlights = (from b in db.Books orderby b.sellNumber descending select b).Take(6).ToList();
            }
            return View();
        }
        //Lấy dữ liệu cho partial menu sách trong nước
        public PartialViewResult _PartialMenuSachTrongNuoc()
        {
            using (DataContext db = new DataContext())
            {

                List<GetThemeSachTrongNuoc> themes = new List<GetThemeSachTrongNuoc>();
                var chude = (from c in db.Themes select c).ToList();
                foreach (var cd in chude)
                {
                    GetThemeSachTrongNuoc theme = new GetThemeSachTrongNuoc();
                    theme.themeName = cd.themeName;
                    themes.Add(theme);
                }
                return PartialView("_PartialMenuSachTrongNuoc",themes);
            }
        }
        //Lấy dữ liệu cho partial menu sách nước ngoài
        public PartialViewResult _PartialMenuSachNuocNgoai()
        {
            using (DataContext db = new DataContext())
            {

                List<GetThemeSachNuocNgoai> themes = new List<GetThemeSachNuocNgoai>();
                var chude = (from c in db.ThemeForeigns select c).ToList();
                foreach (var cd in chude)
                {
                    GetThemeSachNuocNgoai theme = new GetThemeSachNuocNgoai();
                    theme.themeForeignName = cd.ThemeForeignName;
                    themes.Add(theme);
                }
                return PartialView("_PartialMenuSachNuocNgoai", themes);
            }
        }
        //Thêm vào giỏ hàng
        [HttpPost]
        public ActionResult Cart(int ? bid)
        {
            using(DataContext db =new DataContext())
            {
                if (bid != null)
                {
                    Book book = db.Books.Where(x => x.Book_ID == bid).FirstOrDefault();
                    if (book.statusSaleOff == true) 
                    {
                        AddToCart(book.Book_ID, book.bookName, book.saleOffPrice,book.image);
                    }
                    else
                    {
                        AddToCart(book.Book_ID, book.bookName, book.price,book.image);
                    }
                    return PartialView("_PartialCart");
                }
                else
                {
                    return PartialView("_Partial404NotFound");
                }
            }
        }
        public void AddToCart(int id, string bookname, double? price, string image)
        { 
            if (Session["Cart"] == null)
            {
                List<Cart_ViewModels> carts = new List<Cart_ViewModels>();
                Cart_ViewModels cart = new Cart_ViewModels();
                cart.book_id = id;
                cart.bookname = bookname;
                cart.image = image;
                cart.number = 1;
                cart.price = price;
                cart.total = Convert.ToDouble(cart.price * cart.number);
                carts.Add(cart);
                Session["cart"] = carts;
                Session["ThanhTien"] = cart.total;
            }
            else
            {
                int vitri = -1;
                var carts = (List<Cart_ViewModels>)Session["Cart"];
                for (int i = 0; i < carts.Count; i++)  
                {
                    if (carts[i].book_id == id)
                    {
                        vitri = i;
                    }
                }
                if (vitri == -1)
                {
                    Cart_ViewModels cart = new Cart_ViewModels();
                    cart.book_id = id;
                    cart.bookname = bookname;
                    cart.image = image;
                    cart.number = 1;
                    cart.price = price;
                    cart.total = Convert.ToDouble(cart.price * cart.number);
                    carts.Add(cart);
                }
                else
                {
                    carts[vitri].number++;
                    carts[vitri].total = Convert.ToDouble(carts[vitri].number * carts[vitri].price);
                }
                Session["Cart"] = carts;
                
            }
        }
        [HttpPost]
        public ActionResult RemoveItemCart(int ? bid)
        {
            if (bid != null)
            {
                var carts = (List<Cart_ViewModels>)Session["Cart"];
                for (int i = 0; i < carts.Count; i++) 
                {
                    if (carts[i].book_id == bid)
                    {
                        var item = carts[i];
                        carts.Remove(item);
                    }
                }
                Session["Cart"] = carts;
                return PartialView("_PartialCart");
            }
            else
            {
                return PartialView("_Partial404NotFound");
            }
        }
        public ActionResult ViewCart()
        {

            return View();
        }
        [HttpPost]
        public ActionResult UpdateCart(BooksPost quanti, FormCollection f)
        {
            using (DataContext db = new DataContext())
            {
                string[] quantity = f.GetValues("quantity");
                var carts = (List<Cart_ViewModels>)Session["Cart"];
                Book book = db.Books.Where(x => x.Book_ID == quanti.book_id).FirstOrDefault();

                for (int i = 0; i < carts.Count; i++)
                {
                    if (Convert.ToInt32(quantity[i]) >= quanti.quantityExist)
                    {
                        return Json(new { quan_check = 1 });
                    }
                    else
                    {
                        if (Convert.ToInt32(quantity[i]) <= 0)
                        {
                            carts.Remove(carts[i]);
                        }
                        else
                        {
                            carts[i].number = Convert.ToInt32(quantity[i]);
                            carts[i].total = Convert.ToDouble(carts[i].number * carts[i].price);
                        }
                    }

                }

                Session["Cart"] = carts;

                double total = 0;
                foreach (var item in (List<Cart_ViewModels>)Session["Cart"])
                {

                    total = total + item.total;
                }
                Session["ThanhTien"] = total;
                return View("ViewCart");
            }
        }

        

        //Work with paypal payment
        private Payment payment;
        public double TyGiaUSD = 23300;
        //Create payment wit APIcontext
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var listItem = new ItemList() { items = new List<Item>() };
            List < Cart_ViewModels > listCarts = (List<Cart_ViewModels>)Session["Cart"];
            foreach( var cart in listCarts)
            {
                listItem.items.Add(new Item()
                {
                    name = cart.bookname,
                    currency = "USD",
                    price = cart.price.ToString(),
                    quantity = cart.number.ToString(),
                    sku = "sku"
                }) ;
            }
            var payer = new Payer() { payment_method = "paypal" };
            //Cofiguration RedirectUrls
            var redirUrl = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            //create details
            
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = Session["ThanhTien"].ToString()
            };

            
            //Create amount object 
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(),
                details = details
            };

            //create transaction
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Test transaction decription",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = listItem
            });
            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrl
            };
            return payment.Create(apiContext);
        }

        // CREATE execute Payment method
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        //create Payment Whit Paypal method
        public ActionResult PaymentWithPaypal()
        {
            //getying context from the paypal bases on clientId and clientSecret
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/Home/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createPayment = CreatePayment(apiContext, baseUrl + "guid=" + guid);

                    //get links returned from paypal response to create cal function
                    var links = createPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if(link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid, createPayment.id);
                    return Redirect(paypalRedirectUrl);

                }
                else 
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if(executedPayment.state.ToLower() != "approved")
                    {
                        
                        return View("FailureView");
                    }
                    
                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                Session["Cart"] = null;
                return View("FailureView");
            }
            Session["Cart"] = null;
            return View("SuccessView");
        }


        public ActionResult Bill()
        {
            using (DataContext db = new DataContext())
            {
                var cus = (from c in db.Customers
                           join r in db.Roles on c.role equals r.Role_ID
                           where c.Customer_ID == SessionCheckingCustomes.customerID
                           select new
                           {
                               c.Customer_ID,
                               c.customerName,
                               c.customerAddress,
                               c.customerEmail,
                               c.customerPhone
                           }).ToList();
                List<CusIndex_ViewModels> customer = new List<CusIndex_ViewModels>();
                foreach (var c in cus)
                {
                    CusIndex_ViewModels ci = new CusIndex_ViewModels();
                    ci.cus_id = c.Customer_ID;
                    ci.cusName = c.customerName;
                    ci.cusAddress = c.customerAddress;
                    ci.cusEmail = c.customerEmail;
                    ci.cusPhone = c.customerPhone;
                    customer.Add(ci);
                }
                return View(customer);
            }
        }
           

        [HttpGet]
        public ActionResult BooksInCategory(int ? cid)
        {
            if (cid != null)
            {
                using(DataContext db=new DataContext())
                {
                    ViewBag.GetAllCategorys = (from c in db.Categories select c).ToList();
                    ViewBag.GetBookFromID = (from b in db.Books
                                             join c in db.Categories 
                                             on b.category_id equals c.Category_ID
                                             where b.category_id == cid select b).ToList();
                    return View();
                }
            }
            else
            {
                return PartialView("_Partial404NotFound");
            }
        }
        
        [HttpGet]
        public ActionResult BookDetail(int ? bid)
        {
            if (bid != null)
            {
                using(DataContext db=new DataContext())
                {
                    ViewBag.GetBook = (from b in db.Books where b.Book_ID == bid select b).ToList();
                    var get_id_Category = (from b in db.Books join c in db.Categories on b.category_id equals c.Category_ID where b.Book_ID == bid select c.Category_ID).FirstOrDefault();
                    ViewBag.GetBookCategory = (from b in db.Books join c in db.Categories on b.category_id equals c.Category_ID where c.Category_ID == get_id_Category select b).ToList();
                }        
                return View();
            }
            else
            {
                return PartialView("_Partial404NotFound");
            }
        }

    }
}
