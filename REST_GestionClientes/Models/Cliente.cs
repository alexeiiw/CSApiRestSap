using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    /// <summary>
    /// Objeto cliente
    /// </summary>
    public class Cliente
    {
        #region Atributos
        public string CardCode { get; set; }
        public string U_CodSIFCO { get; set; }
        public string CardName { get; set; }
        public int GroupCode { get; set; }
        public string Phone1 { get; set; }
        public string E_Mail { get; set; }
        public string AddId { get; set; }
        public string VatIdUnCmp { get; set; }
        public string U_FechaInicio { get; set; }
        public char U_AgenteRet { get; set; }
        public List<PersonaContacto> ContactPerson { get; set; }
        public List<Direccion> Addresses { get; set; }
        public Ive IVE { get; set; }
        #endregion        
    }
}