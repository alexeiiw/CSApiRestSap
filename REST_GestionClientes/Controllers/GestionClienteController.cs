using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using REST_GestionClientes.Models;

namespace REST_GestionClientes.Controllers
{
    /// <summary>
    /// Controlador de Gestión de cliente
    /// Gestión de peticiones REST
    /// </summary>
    public class GestionClienteController : ApiController
    {
        
        // GET: api/GestionCliente
        public IEnumerable<string> Get()
        {
            return new string[] { "Prueba ws", "Prueba de funcionamiento WS" };
        }
        /*
        // GET: api/GestionCliente/5
        public string Get(int id)
        {
            return "Alguna persona";
        }
        */
        // POST: api/GestionCliente
        /// <summary>
        /// Establece un nuevo Cliente en SAP B1
        /// </summary>
        /// <param name="cliente">JSON con estructura de cliente</param>
        /// <returns></returns>
        public Transacciones Post([FromBody]Cliente cliente)
        {
            BusinessPartner bp = new BusinessPartner();
            Transacciones transaction = bp.addCustomer(cliente);
            return transaction;
        }
        // PUT: api/GestionCliente/5
        /// <summary>
        /// Actualiza un cliente existente en SAP B1
        /// </summary>
        /// <param name="cliente">JSON con estructura de cliente</param>
        /// <returns></returns>
        public Transacciones Put([FromBody]Cliente cliente)
        {
            BusinessPartner bp = new BusinessPartner();
            Transacciones transaction = bp.updateCustomer(cliente);
            return transaction;
        }
        
        public string Put(int id)
        {
            
            return "hello "+ id.ToString();
        }        
    }
}
