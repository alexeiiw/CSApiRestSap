using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using REST_GestionClientes.Controllers;
namespace REST_GestionClientes.Models
{
    /// <summary>
    /// Clase para manipular la DB intermedia
    /// </summary>
    public class Db
    {
        Parametros parametros;
        public Db()
        {
            this.parametros = new Parametros();
        }
        /// <summary>
        /// Agrega Registro a Log
        /// </summary>
        /// <param name="ResultadoTransaccion"> Exito o Fracaso</param>
        /// <param name="TipoTransaccion">Tipo Transaccion</param>
        /// <param name="idOBjetoSAP"> OBjectCode SAP B1</param>
        /// <param name="Mensaje">Mensaje de LOG</param>
        /// <param name="TipoOperacion">Insercion | Actualzacion</param>
        public void addLog(int ResultadoTransaccion, int TipoTransaccion, string idOBjetoSAP, string Mensaje, int TipoOperacion)
        {
            string query = @"INSERT INTO Bitacora 
                             (Fecha,TipoTransaccion,ResultadoTransaccion,idOBjetoSAP,Mensaje,TipoOperacion) VALUES
                             (GETDATE(),'" + ResultadoTransaccion+"','"+TipoTransaccion+"','"+idOBjetoSAP+"','"+Mensaje+"','"+TipoOperacion+"')";
            SqlConnection con = new SqlConnection(this.parametros.ObtenerParametrosSqlServer());
            try
            {
                con.Open();
                SqlCommand consulta = new SqlCommand(query, con);
                consulta.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge" + e.Message);
            }
            

            con.Close();            
        }
    }
}