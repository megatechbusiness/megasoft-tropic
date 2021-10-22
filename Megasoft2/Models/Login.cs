using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Megasoft2.Models
{
    public class Login
    {
        [Required]
        public string Username { get; set; }

        //[Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Company { get; set; }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public string ResetPassword { get; set; }
        public string SmartId { get; set; }

    }
}