using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class changePass
    {
        [Required(ErrorMessage = "Không được bỏ trống")]
        public int userid { get; set; }
        [EmailAddress(ErrorMessage = "Không đúng định dạng email")]
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50), MinLength(4)]
        public string email { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50), MinLength(2)]
        public string old_pass { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50), MinLength(2)]
        public string password { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        public string nhaplaipassword { get; set; }
        
     
    }
}