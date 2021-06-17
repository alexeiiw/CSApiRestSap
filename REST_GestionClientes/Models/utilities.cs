using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using NLog;
using System.Data.SqlClient;
using System.Data;

namespace REST_GestionClientes.Models
{
    /// <summary>
    /// Objeto de Gestion de utilidades
    /// Principalmente Grabacion de mensajes en Logger.
    /// </summary>
    public class utilities
    {
        public static string Error = "1";
        public static mensajes mensajeSuccess = new mensajes();
        public static Logger logger
        {
            get { return LogManager.GetCurrentClassLogger(); }
        }

        public  Transacciones  accionesMensajes(mensajes msg)
        {
            mensajes msjLimpio = new mensajes();

            int resultado, TipoOperacion=1;

            string idObjetoSap, mensaje,tipoTransaccion;

            foreach (Transacciones t in msg.transacciones)
            {
                string [] aux = t.mensaje.Split('|');
                tipoTransaccion = aux.Count() == 3 ?(aux[2]) : "-1";
                
                mensaje = aux[0]+"|"+aux[1];
                idObjetoSap = aux[0];
                if (t.codigo.Equals("success"))
                {
                    resultado = 1;
                }else
                {
                    resultado = 0;
                    idObjetoSap = string.Empty;
                    
                }
                insertarTransaccion(resultado,idObjetoSap,(tipoTransaccion),mensaje,TipoOperacion);
                msjLimpio.transacciones.Add(new Transacciones(t.codigo,mensaje));

            }
            if (msjLimpio.transacciones.Count > 1)
            {
                string[] primerMensaje = msjLimpio.transacciones[0].mensaje.Split('|');
                string[] nMensaje;
                for (int i = 1; i < msjLimpio.transacciones.Count; i++)
                {
                    nMensaje = msjLimpio.transacciones[i].mensaje.Split('|');
                    primerMensaje[0] += "&" + nMensaje[0];
                    primerMensaje[1] += "&" + nMensaje[1];
                }
                msjLimpio.transacciones[0].mensaje = primerMensaje[0] + "|" + primerMensaje[1];
            }
            return msjLimpio.transacciones[0];
        }
        public void insertarTransaccion(int ResultadoTransaccion, string IdObjetoSAP, string TipoTransaccion, string Mensaje, int TipoOperacion)
        {
            try
            {
                SqlConnection conn = new SqlConnection(getConexionString());
                conn.Open();
                string query = "insert into Bitacora (Fecha,ResultadoTransaccion,IdObjetoSAP,TipoTransaccion,Mensaje,TipoOperacion) values (GETDATE(),@ResultadoTransaccion,@IdObjetoSAP,@TipoTransaccion,@Mensaje,@TipoOperacion)";
                SqlCommand commandInsertar = new SqlCommand(query, conn);
                commandInsertar.Parameters.Add("@ResultadoTransaccion", SqlDbType.Int).Value = ResultadoTransaccion;
                commandInsertar.Parameters.Add("@IdObjetoSAP", SqlDbType.VarChar).Value = IdObjetoSAP;
                commandInsertar.Parameters.Add("@TipoTransaccion", SqlDbType.VarChar).Value = TipoTransaccion;
                commandInsertar.Parameters.Add("@Mensaje", SqlDbType.VarChar).Value = Mensaje;
                commandInsertar.Parameters.Add("@TipoOperacion", SqlDbType.Int).Value = TipoOperacion;

                commandInsertar.ExecuteNonQuery();
                conn.Close();
                conn.Dispose();
            }
            catch (Exception e)
            {
                logger.Error("Error al insertar en la bitacora. |" + e.Message);
            }
        }

        public string getConexionString()
        {
            string cadena;
            cadena = string.Format("Data Source = {0}; Initial Catalog= {1}; User ID={2}; Password={3}", ConfigurationManager.AppSettings["SQLSERVER"], ConfigurationManager.AppSettings["DBIntermedia"], ConfigurationManager.AppSettings["SQLUser"], ConfigurationManager.AppSettings["SQLPassword"]);
            return cadena;
        }
    }
}