using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    /// <summary>
    /// Objeto de Gestión de Dirección
    /// </summary>
    public class Direccion
    {
        public char AddresType { get; set; }
        public string Address { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Municipio { get; set; }
        public char U_TipoVivienda { get; set; }
        public decimal U_TiempoLab { get; set; }
        public decimal U_Salario { get; set; }        
        public string U_NombreTrabajo { get; set; }
        public string U_TelefonoTrabajo { get; set; }
    }
}