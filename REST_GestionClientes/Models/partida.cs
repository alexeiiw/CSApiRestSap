using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    public class partida
    {
        public string codigoCliente { get; set; }
        public string fechaContabilizacion { get; set; }
        public string fechaVencimiento { get; set; }
        public string numeroCreditoSIFCO { get; set; }
        public string codigoOrigenCredito { get; set; }
        public string observaciones { get; set; }
        public int tipo { get; set; }
        public DetallePartida detallePartida { get; set; }

    }

    public class MediosDePago
    {
        public string esCheque { get; set; }
        public string esTransferencia { get; set; }
        public string esEfectivo { get; set; }
        public string esDeposito { get; set; }
        public string codigoCuentaMayor { get; set; }
        public string moneda { get; set; }
        public string tipoDeposito { get; set; }
        public string fecha { get; set; }
        public string nombreBanco { get; set; }
        public string numeroDocumento { get; set; }
        public string endosoCheque { get; set; }
        public string credito { get; set; }
        public string debito { get; set; }
        public string descripcion { get; set; }
    }

    public class Rubros
    {
        public string esCapital { get; set; }
        public string codigo { get; set; }
        public string credito { get; set; }
        public string debito { get; set; }
        public string facturable { get; set; }
        public string esAfectoAIva { get; set; }
       
    }

    public class DetallePartida
    {
        public List<MediosDePago> mediosDePago { get; set; }
        public List<Rubros> rubros { get; set; }
    }
}