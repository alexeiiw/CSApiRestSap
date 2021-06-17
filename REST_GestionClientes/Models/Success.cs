using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    public class Success
    {
        public string message { get; set; }
        public string customerCode { get; set; }

        public Success(string customerCode)
        {
            this.message = "success";
            this.customerCode = customerCode;
        }
    }
}