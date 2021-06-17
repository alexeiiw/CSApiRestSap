using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    /// <summary>
    /// Objeto de Gestión IVE
    /// </summary>
    public class Ive
    {
        #region Atributos
        
        public string Code { get; set; }
        public string U_Nombre1 { get; set; }
        public string U_Nombre2 { get; set; }
        public string U_Apellido1 { get; set; }
        public string U_Apellido2 { get; set; }
        public string U_ApellidoC { get; set; }
        public string U_FechaNac { get; set; }
        public string U_CodPais { get; set; }
        public char U_Sexo { get; set; }
        public char U_TipoP { get; set; }
        public string U_RazonSocial { get; set; }
        public char U_TipoId { get; set; }
        public string Name { get; set; }
        #endregion
    }
}