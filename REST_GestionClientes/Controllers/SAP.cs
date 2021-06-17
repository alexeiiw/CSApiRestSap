using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace REST_GestionClientes.Controllers
{
    /// <summary>
    /// Objeto de Acceso y Gestión dentro de SAP B1
    /// </summary>
    public class SAP
    {
        #region Atributos de la clase
        /// <summary>
        /// Attributo Compañia
        /// </summary>
        private SAPbobsCOM.Company oCompany;
        /// <summary>
        /// Código de Error
        /// </summary>
        private long lRetCode;
        /// <summary>
        /// Código de error
        /// </summary>
        private int lErrCode;
        /// <summary>
        /// Mensaje de Error
        /// </summary>
        private String message;
        #endregion

        public SAP() { }
        /// <summary>
        /// Conexión hacia Sociedad SAP B1
        /// </summary>
        /// <returns>Mensaje de error, si existiese problemas con la misma o cadena vacía de ser una conexión exitosa</returns>
        public String connect()
        {
            this.oCompany = new SAPbobsCOM.Company();
            Parametros parametros = new Parametros();


            foreach (DataRow parameter in parametros.getSAPparameters().Rows)
            {
                if (parameter["nombre_parametro"].ToString().Equals("ServerSap"))
                    oCompany.Server = parameter["valor"].ToString();
                else if (parameter["nombre_parametro"].ToString().Equals("DbServerType"))
                {
                    switch (parameter["valor"].ToString())
                    {
                        case "dst_MSSQL2008":
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                            break;
                        case "dst_MSSQL2012":
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                            break;
                        case "dst_MSSQL2014":
                            oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                            break;
                        //case "dst_MSSQL2016":
                        //    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
                        //    break;
                    }
                }
                else if (parameter["nombre_parametro"].ToString().Equals("DbSap"))
                    oCompany.CompanyDB = parameter["valor"].ToString();
                else if (parameter["nombre_parametro"].ToString().Equals("UserNameSap"))
                    oCompany.UserName = parameter["valor"].ToString();
                else if (parameter["nombre_parametro"].ToString().Equals("PasswordSap"))
                    oCompany.Password = parametros.DesEncriptar(parameter["valor"].ToString());
                //else if (parameter["nombre_parametro"].ToString().Equals("language"))
                //     oCompany.language = parameter["valor"].ToString();
            }
            //Parametros.Rows[0]["nombre_parametro"]




            lRetCode = oCompany.Connect();


            if (lRetCode != 0)
            {
                oCompany.GetLastError(out lErrCode, out message);
                return message + lErrCode.ToString();
            }
            return "";
        }
        /// <summary>
        /// Desconectandose de la sociedad SAP
        /// </summary>
        public void disconnect()
        {
            oCompany.Disconnect();
        }
        /// <summary>
        /// Retorna la compañia para poder acceder a distintos attributos de la misma.
        /// </summary>
        /// <returns></returns>
        public SAPbobsCOM.Company getBObject()
        {

            return oCompany;
        }
        /// <summary>
        /// Retorna el número de serie del cliente ubicado en la tabla de parámetros sap(Código 899)
        /// </summary>
        /// <returns>Código de serie de numeración</returns>
        public string getSeries()
        {
            try
            {
                SAPbobsCOM.Recordset irs;
                irs = ((SAPbobsCOM.Recordset)(this.oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
                irs.DoQuery("SELECT Code,U_Valor FROM [@FE_PARAM] WHERE Code = 899");

                if (irs.RecordCount == 0) return "No se ha definido el parámetro 899 - Serie de numeración de cliente (INTEFAZ SCR)";

                irs.MoveFirst();
                while (!irs.EoF)
                {
                    if (irs.Fields.Item("Code").Value.ToString().Contains("899")) return irs.Fields.Item("U_Valor").Value.ToString();
                    irs.MoveNext();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }
    }
}