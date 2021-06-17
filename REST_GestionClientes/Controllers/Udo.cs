using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using REST_GestionClientes.Models;
namespace REST_GestionClientes.Controllers
{
    /// <summary>
    /// Clase para manipular Objetos de Usuario en SAP B1
    /// </summary>
    public class Udo
    {
        #region Atributos
        private static SAPbobsCOM.GeneralService oGeneralService;
        private static SAPbobsCOM.GeneralData oGeneralData;
        private static SAPbobsCOM.GeneralDataParams oGeneralParams;
        private static SAPbobsCOM.CompanyService oCompanyService;
        #endregion
        /// <summary>
        /// Crea un nuevo 
        /// </summary>
        /// <param name="company">Compañia SAP</param>
        /// <param name="ive">Objeto IVE con información cargada</param>
        /// <returns></returns>
        public static string newDataIve(SAPbobsCOM.Company company, Ive ive)
        {
            try
            {

                oCompanyService = company.GetCompanyService();
                // Get GeneralService (oCmpSrv is the CompanyService)
                oGeneralService = oCompanyService.GetGeneralService("DATOSIVE");
                // Create data for new row in main UDO
                oGeneralData = ((SAPbobsCOM.GeneralData)
                    (oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)));

                oGeneralData.SetProperty("Code", ive.Code);
                oGeneralData.SetProperty("Name", ive.Name);
                oGeneralData.SetProperty("U_Nombre1", ive.U_Nombre1);
                oGeneralData.SetProperty("U_Nombre2", ive.U_Nombre2);
                oGeneralData.SetProperty("U_Apellido1", ive.U_Apellido1);
                oGeneralData.SetProperty("U_Apellido2", ive.U_Apellido2);
                oGeneralData.SetProperty("U_ApellidoC", ive.U_ApellidoC);
                oGeneralData.SetProperty("U_FechaNac", ive.U_FechaNac);
                oGeneralData.SetProperty("U_CodPais", ive.U_CodPais);
                oGeneralData.SetProperty("U_Sexo", ive.U_Sexo.ToString());
                oGeneralData.SetProperty("U_TipoP", ive.U_TipoP.ToString());
                oGeneralData.SetProperty("U_RazonSocial", ive.U_RazonSocial.ToString());
                oGeneralData.SetProperty("U_TipoId", ive.U_TipoId.ToString());


                oGeneralParams = oGeneralService.Add(oGeneralData);
                if (oGeneralParams != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oGeneralParams);
                    oGeneralParams = null;
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
        /// <summary>
        /// Actualiza Información de IVE
        /// </summary>
        /// <param name="company">Sociedad SAP B1</param>
        /// <param name="ive"> Objeto IVE</param>
        /// <returns></returns>
        public static string updateDataIve(SAPbobsCOM.Company company, Ive ive)
        {
            try
            {

                oCompanyService = company.GetCompanyService();
                // Get GeneralService (oCmpSrv is the CompanyService)
                oGeneralService = oCompanyService.GetGeneralService("DATOSIVE");
                // Create data for new row in main UDO
                oGeneralData = ((SAPbobsCOM.GeneralData)
                    (oGeneralService.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData)));

                oGeneralData.SetProperty("Code", ive.Code);
                oGeneralData.SetProperty("Name", ive.Name);
                oGeneralData.SetProperty("U_Nombre1", ive.U_Nombre1);
                oGeneralData.SetProperty("U_Nombre2", ive.U_Nombre2);
                oGeneralData.SetProperty("U_Apellido1", ive.U_Apellido1);
                oGeneralData.SetProperty("U_Apellido2", ive.U_Apellido2);
                oGeneralData.SetProperty("U_ApellidoC", ive.U_ApellidoC);
                oGeneralData.SetProperty("U_FechaNac", ive.U_FechaNac);
                oGeneralData.SetProperty("U_CodPais", ive.U_CodPais);
                oGeneralData.SetProperty("U_Sexo", ive.U_Sexo.ToString());
                oGeneralData.SetProperty("U_TipoP", ive.U_TipoP.ToString());
                oGeneralData.SetProperty("U_TipoId", ive.U_TipoId.ToString());


                oGeneralService.Update(oGeneralData);


            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
    }
}