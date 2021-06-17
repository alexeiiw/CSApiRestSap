using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using REST_GestionClientes.Models;
namespace REST_GestionClientes.Controllers
{
    public class enviarPartidaController : ApiController
    {
        Acciones nAcciones;
        string ret = "";
        mensajes mensaje;
        // GET: api/enviarPartida
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/enviarPartida/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/enviarPartida
        public Transacciones Post([FromBody]partida value)
        {
            utilities.mensajeSuccess.transacciones.Clear();
            mensaje = new mensajes();
            Transacciones transaccion;
            utilities u = new utilities();
            try
            {
                nAcciones = new Acciones();
                partida partida = value;
                if (partida == null)
                {
                    transaccion = new Transacciones("error", "(00001)|Estructura JSON de cadena de entrada incorrecto.|-1");
                    utilities.mensajeSuccess.transacciones.Clear();
                    mensaje.transacciones.Add(transaccion);
                    transaccion = u.accionesMensajes(mensaje);
                    return transaccion;
                }
                switch (partida.tipo)
                {
                    case 1:
                        {
                            utilities.logger.Debug("Partida de Tipo 1, Validando Json");
                            ret = nAcciones.ValidarPartida(partida);
                            if (ret != "1") { ret += "|131"; break; }
                            ret = nAcciones.ValidarDesembolso(partida);
                            if (ret != "1") { ret += "|131"; break; }
                            utilities.logger.Debug("Json validado correctamente");
                            ret = nAcciones.ConectarEmpresa();
                            if (ret != "1") { ret += "|131"; break; }
                            utilities.logger.Debug("Ejecutando Desembolso...");
                            ret = nAcciones.EjecutarDesembolso(partida);
                            if (ret != "1") { ret += "|131"; break; }
                            utilities.logger.Debug("Desembolso ejecutado correctamente");
                            utilities.logger.Debug("Desconectandose de SAP");
                            nAcciones.DesconectarEmpresa();
                            utilities.logger.Debug("desconectado Correctamente");
                        } break;
                    case 2:
                        {
                            utilities.logger.Debug("Partida de Tipo 2, Validando Json");
                            ret = nAcciones.ValidarPartida(partida);
                            if (ret != "1") break;
                            ret = nAcciones.ValidarCuota(partida);
                            if (ret != "1") break;
                            utilities.logger.Debug("Json validado correctamente");
                            ret = nAcciones.ConectarEmpresa();
                            if (ret != "1") break;
                            utilities.logger.Debug("Ejecutando Pago de cuota...");
                            ret = nAcciones.EjecutarPagoCuota(partida);
                            utilities.logger.Debug("Pago de cuota ejecutado correctamente");
                            utilities.logger.Debug("Desconectandose de SAP");
                            nAcciones.DesconectarEmpresa();
                            utilities.logger.Debug("desconectado Correctamente");
                        } break;
                    case 3:
                        {
                            utilities.logger.Debug("Partida de Tipo 3, Validando Json");
                            ret = nAcciones.ValidarPartida(partida);
                            if (ret != "1") { ret += "|30"; break; }
                            ret = nAcciones.ValidarAjuste(partida);
                            if (ret != "1") { ret += "|30"; break; }
                            utilities.logger.Debug("Json validado correctamente");
                            ret = nAcciones.ConectarEmpresa();
                            if (ret != "1") { ret += "|30"; break; }
                            utilities.logger.Debug("Ejecutando Ajustes varios...");
                            ret = nAcciones.EjecutarAjuste(partida);
                            if (ret != "1") { ret += "|30"; break; }
                            utilities.logger.Debug("Ajustes varios efectuado correctamente...");
                            utilities.logger.Debug("Desconectandose de SAP");
                            nAcciones.DesconectarEmpresa();
                            utilities.logger.Debug("desconectado Correctamente");
                        } break;
                    default:
                        {
                            transaccion = new Transacciones("error", "(00002)|Tipo de partida incorrecto|-1");
                            mensaje.transacciones.Add(transaccion);
                            utilities.logger.Error( $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00002).");
                            utilities.mensajeSuccess.transacciones.Clear();
                            transaccion = u.accionesMensajes(mensaje);
                            return transaccion;
                        }
                }
            }
            catch (Exception e)
            {
                if (e.Message != "" && e.Message != "1" && utilities.Error=="1")
                {
                    utilities.logger.Error(e.StackTrace + " \n (00005) " + e.Message);
                    transaccion = new Transacciones("error", "(00005)|"+e.Message + "|-1");
                    mensaje.transacciones.Add(transaccion);
                    utilities.mensajeSuccess.transacciones.Clear();
                    transaccion = u.accionesMensajes(mensaje);
                    return transaccion;
                  
                }
                utilities.logger.Error(e.StackTrace + " \n (00004) " + utilities.Error);
                transaccion = new Transacciones("error", "(00004)|" + utilities.Error);
                mensaje.transacciones.Add(transaccion);
                utilities.mensajeSuccess.transacciones.Clear();
                transaccion = u.accionesMensajes(mensaje);
                return transaccion;
            }

            if (ret != "1")
            {
                transaccion = new Transacciones("error",ret);
                mensaje.transacciones.Add(transaccion);
                utilities.mensajeSuccess.transacciones.Clear();
                transaccion = u.accionesMensajes(mensaje);
                return transaccion;
            }
            return u.accionesMensajes(utilities.mensajeSuccess);
        }

        // PUT: api/enviarPartida/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/enviarPartida/5
        //public void Delete(int id)
        //{
        //}
    }
}
