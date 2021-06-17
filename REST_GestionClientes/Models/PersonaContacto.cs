using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    public class PersonaContacto
    {
        public char U_TipoRef { get; set; }
        public string Name { get; set; }
        public string Tel1 { get; set; }
        public string Address { get; set; }
    }
}