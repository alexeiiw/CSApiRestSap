using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using REST_GestionClientes.Models;
namespace REST_GestionClientes.Controllers
{
    /// <summary>
    /// Objeto para gestión de parametros
    /// </summary>
    public class Parametros
    {
        /// <summary>
        /// Lee el archivo XML de configuración SQL Server
        /// </summary>
        /// <returns>Vector con los atributos para la conexión SQL</returns>
        private string[] readXML()
        {
            XmlDocument xDoc = new XmlDocument();
            string[] DatosConeccion = new string[4];
            try
            {
                DatosConeccion[0] = ConfigurationManager.AppSettings["SQLSERVER"];
                DatosConeccion[1] = ConfigurationManager.AppSettings["DBIntermedia"];
                DatosConeccion[2] = ConfigurationManager.AppSettings["SQLUser"];
                DatosConeccion[3] = ConfigurationManager.AppSettings["SQLPassword"];
                if (DatosConeccion[0] == "" || DatosConeccion[1] == "" || DatosConeccion[2] == "" || DatosConeccion[3] == "") DatosConeccion = null;
                
            }
            catch (Exception ex)
            {
                utilities.logger.Error("Error al Leer el archivo " + ex.Message + ex.StackTrace);
                
                return null;
            }
            return DatosConeccion;
        }
        /// <summary>
        /// Parametros de conexión SQL Server
        /// </summary>
        /// <returns>Retrona la cadena de conexión para un objeto de tipo SQLConnection</returns>
        public string ObtenerParametrosSqlServer()
        {
            XmlDocument xDoc = new XmlDocument();
            string conexion = null;
            try
            {
                //string Path = ObtenerPathArchivo();

                string[] PrConexion = readXML();
                if (PrConexion != null)
                {
                    char[] caracteres = { ';' };
                    char[] parCadena = { ',' };
                    string servidor = PrConexion[0].TrimEnd(caracteres);
                    string[] words = servidor.Split(parCadena);
                    string serv = words[0];
                   
                    conexion += "Data Source=\"" + PrConexion[0] + "\";";
                    conexion += "Initial Catalog =\"" + PrConexion[1] + "\";";
                    conexion += "User ID=\"" + PrConexion[2] + "\";";
                    conexion += "Password=\"" + PrConexion[3] + "\";";
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                utilities.logger.Error("01-ObtenerParametrosSqlServer - Error al Obtener los Parametros de Conexión :  "
                      + " " + ex.Message);
            }
            return conexion;
        }
        /// <summary>
        /// Retorna los parametros de conexión a la sociedad SAP
        /// </summary>
        /// <returns>Retorna los parametros de conexión a la sociedad SAP</returns>
        public DataTable getSAPparameters()
        {
            DataTable Parametros = new DataTable();
            SqlDataAdapter Adaptador = new SqlDataAdapter();
            SqlConnection con = new SqlConnection(this.ObtenerParametrosSqlServer());
            string query = @"
              SELECT nombre_parametro,valor FROM Parametros WHERE nombre_parametro = 'ServerSap'
              UNION
              SELECT nombre_parametro,valor FROM Parametros WHERE nombre_parametro = 'dbServerType'
              UNION
              SELECT nombre_parametro,valor FROM Parametros WHERE nombre_parametro = 'DbSap'
              UNION
              SELECT nombre_parametro,valor FROM Parametros WHERE nombre_parametro = 'PasswordSap'                            
              UNION
              SELECT nombre_parametro,valor FROM Parametros WHERE nombre_parametro = 'UserNameSap'
              UNION
              SELECT nombre_parametro,valor FROM Parametros WHERE nombre_parametro = 'Language_Errors';";
            try
            {
                SqlCommand consulta = new SqlCommand(query, con);
                Adaptador.SelectCommand = consulta;
                Adaptador.Fill(Parametros);
                con.Close();
            }
            catch (Exception ex)
            {
                utilities.logger.Error(System.Reflection.MethodBase.GetCurrentMethod().Name + ex.Message);
                return null;
            }
            return Parametros;
        }
        ///<summary>
        /// Este metodo desencripta la cadena que le envíamos en el parámentro de entrada.
        ///</summary>
        ///<param name="_cadenaAdesencriptar">Cadena a Desencryptar</param>

        public string DesEncriptar(string _cadenaAdesencriptar)
        {
            string result = string.Empty;

            _cadenaAdesencriptar = _cadenaAdesencriptar.Replace(";", "");
            byte[] decryted = Convert.FromBase64String(_cadenaAdesencriptar);

            result = System.Text.Encoding.Unicode.GetString(decryted);
            return result;
        }
    }
}