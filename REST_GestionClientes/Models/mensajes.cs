using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace REST_GestionClientes.Models
{
    /// <summary>
    /// Objeto para gestión de mensajes a SIFCO
    /// Se envia codigo de exito o fracaso, asi mismo el mensaje
    /// </summary>
    public class mensajes
    {
        public List<Transacciones> transacciones = new List<Transacciones>();
    }
    /// <summary>
    /// Objeto de gestion de transacciones para envio de mensajes
    /// </summary>
    public class Transacciones
    {
        public string mensaje { get; set; }
        public string codigo { get; set; }
        public Transacciones() { }
        public Transacciones(string code, string message)
        {
            this.codigo = code;
            this.mensaje = message;
        }
    }
}