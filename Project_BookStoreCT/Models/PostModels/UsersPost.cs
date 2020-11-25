using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class UsersPost
    {
        public int userid { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string username { get; set; }
        [MaxLength(50), MinLength(2)]
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string email { get; set; }
        [EmailAddress(ErrorMessage = "Không đúng định dạng email")]
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50),MinLength(2)]
        public string password { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50), MinLength(2)]
        public string nhaplaipassword { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public int sex { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string phone { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string address { get; set; }
        public HttpPostedFileBase avatar { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public int role { get; set; }
    }
}