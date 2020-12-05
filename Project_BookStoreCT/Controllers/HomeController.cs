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
        public ActionResult UpdateCart(BooksPost bo, FormCollection f)
        {
            using (DataContext db = new DataContext())
            {
                int quantities = bo.number;
                int[] quantity = new int[] { quantities };
                var carts = (List<Cart_ViewModels>)Session["Cart"];
                Book book = db.Books.Where(x => x.Book_ID == bo.book_id).FirstOrDefault();

                for (int i = 0; i < carts.Count; i++)
                {
                    if (quantity[i] > book.quantityExists)
                    {
                        return Json(new { _mess__ = 0 });
                    } 
                    else
                    {
                        if (quantity[i] <= 0)
                        {
                            carts.Remove(carts[i]);
                        }
                        else
                        {
                            carts[i].number = quantity[i];
                            carts[i].total = Convert.ToDouble(carts[i].number * carts[i].price);
                        }
                        
                    }

                }

                Session["Cart"] = carts;

                double total = 0;
                foreach (var item in (List<Cart_ViewModels>)Session["Cart"])
                {

                    total = total + Math.Round(item.total,2);
                }
                Session["ThanhTien"] = total;
                return Json(new { _mess__ = 1 });
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
            foreach(Cart_ViewModels cart in listCarts)
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
            return RedirectToAction("SuccessView");
        }
        [HttpGet]
        public ActionResult Bill()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetCus = (from b in db.Customers where b.Customer_ID == SessionCheckingCustomes.customerID select b).ToList();
            }
            return View();
        }

       [HttpPost]
        public ActionResult Bill(BillsPost bi, FormCollection f)
        {
            using (DataContext db = new DataContext())
            {
                
                if (f["payment"] !=null)
                {
                    if (Convert.ToInt32(f["payment"]) == 1)
                    {
                        Bill bills = new Bill();
                        bills.customerName = f["txtKhachHang"];
                        bills.phoneNumber = f["txtSoDienThoai"];
                        bills.date_set = DateTime.Now;
                        bills.customerAddress = f["txtDiaChi"];
                        bills.total = (double?)Session["ThanhTien"];
                        bills.payment_method = Convert.ToInt32(f["payment"]);
                        bills.payment_status = false;
                        bills.delivered_status = false;
                        db.Bills.Add(bills);
                        db.SaveChanges();
                        var bill_id_max = db.Bills.Max(x => x.Bill_ID);
                        foreach (var item in (List<Cart_ViewModels>)Session["Cart"])
                        {
                            DetailBill detailBill = new DetailBill();
                            detailBill.Bill_ID = bill_id_max;
                            detailBill.Book_ID = item.book_id;
                            detailBill.quantity = item.number;
                            db.DetailBills.Add(detailBill);
                            db.SaveChanges();
                        }
                       
                        Session["Cart"] = null;
                        return RedirectToAction("SuccessView");
                    }
                    else
                    {
                        Bill bills = new Bill();
                        bills.customerName = f["txtKhachHang"];
                        bills.phoneNumber = f["txtSoDienThoai"];
                        bills.date_set = DateTime.Now;
                        bills.customerAddress = f["txtDiaChi"];
                        bills.total = (double?)Session["ThanhTien"];
                        bills.payment_method = Convert.ToInt32(f["payment"]);
                        bills.payment_status = false;
                        bills.delivered_status = false;
                        db.Bills.Add(bills);
                        db.SaveChanges();
                        var bill_id_max = db.Bills.Max(x => x.Bill_ID);
                        foreach (var item in (List<Cart_ViewModels>)Session["Cart"])
                        {
                            DetailBill detailBill = new DetailBill();
                            detailBill.Bill_ID = bill_id_max;
                            detailBill.Book_ID = item.book_id;
                            detailBill.quantity = item.number;
                            db.DetailBills.Add(detailBill);
                            db.SaveChanges();
                        }
                        
                        return RedirectToAction("PaymentWithPaypal");
                    }
                    
                }
                else
                {
                    return Json(new { _mess__ = 0 });
                }
                
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

        public ActionResult SuccessView()
        {
            using (DataContext db = new DataContext())
            {

                var bill_id_max = db.Bills.Max(x => x.Bill_ID);
                var bookbill = (from b in db.Books
                           join d in db.DetailBills on b.Book_ID equals d.Book_ID
                           join bi in db.Bills on d.Bill_ID equals bi.Bill_ID
                           where d.Book_ID == b.Book_ID && d.Bill_ID == bill_id_max
                                select new
                           {
                               b.statusSaleOff,
                               b.saleOffPrice,
                               b.bookName,
                               b.image,
                               d.quantity,
                               b.price,
                               bi.customerName,
                               bi.phoneNumber,
                               bi.total,
                               bi.payment_method
                           }).ToList();
               
                List<DetailBills_ViewModels> detailsBill = new List<DetailBills_ViewModels>();
                foreach (var b in bookbill)
                {
                    DetailBills_ViewModels detailBills = new DetailBills_ViewModels();
                    detailBills.bookName = b.bookName;
                    detailBills.image = b.image;
                    detailBills.quantity = b.quantity;
                    detailBills.price = b.price;
                    detailBills.customerName = b.customerName;
                    detailBills.phone = b.phoneNumber;
                    detailBills.total = (double)b.total;
                    detailBills.payment_method = (int)b.payment_method;
                    detailBills.saleOffPrice = b.saleOffPrice;
                    detailBills.statusSaleOff = b.statusSaleOff;
                    detailsBill.Add(detailBills);
                }
               
                return View(detailsBill);
            }
            
        }

    }
}
