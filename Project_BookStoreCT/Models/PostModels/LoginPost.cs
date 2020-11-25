using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Project_BookStoreCT.Models.PostModels
{
    public class LoginPost
    {

        
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50), MinLength(2)]
        public string email { get; set; }
        [Required(ErrorMessage = "Không được bỏ trống")]
        [MaxLength(50)]
        public string password { get; set; }
        public bool rememberMe { get; set; }
    }
}