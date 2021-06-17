using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using REST_GestionClientes.Models;
using System.Web.Script.Serialization;

namespace REST_GestionClientes.Controllers
{
    /// <summary>
    /// Gestión Propia del Socio de Negocios
    /// </summary>
    public class BusinessPartner
    {
        #region Atributos
        /// <summary>
        /// Instancia para volcar LOG DB Intermedia
        /// </summary>
        private Db db;

        private SAP sap;
        /// <summary>
        /// Mensaje de Error
        /// </summary>
        private String errormessage;
        /// <summary>
        /// Código de Error
        /// </summary>
        private int errorcode;
        /// <summary>
        /// Objeto BusinessPartner
        /// </summary>
        private SAPbobsCOM.BusinessPartners obp;
        /// <summary>
        /// Constante RFC
        /// </summary>
        private readonly string RFC = "000000000000";

        /// <summary>
        /// Constante Moneda
        /// </summary>
        private readonly string CURRENCY = "##";
        #endregion
        public BusinessPartner()
        {
            this.sap = new SAP();
            this.db = new Db();
        }
        /// <summary>
        /// Agregara al cliente enviado a SAP con todas las caracteristicas que correspondan
        /// </summary>
        /// <param name="customer">Recibe al cliente por crear</param>
        /// <returns>
        ///     Success: Cliente insertado correctamente;
        ///     CodError - Mensaje de Error
        /// </returns>
        public Transacciones addCustomer(Cliente customer)
        {
            string CardCode = "";
            int series = -1;            
            this.errormessage = this.sap.connect();



            //if(series == -1)
            //{
            //    this.sap.disconnect();
            //    return "Error de conexion:" + serie;
            //}

            if (this.errormessage != "")
            {
                utilities.logger.Error("error - "+ System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge"+ this.errormessage);
                this.db.addLog(2, 0, "", "error"  + this.errormessage, 0);
                return new Transacciones("error", this.errormessage);
            }
                

            //string serie = sap.getSeries();
            Int32.TryParse(sap.getSeries(), out series);

            this.obp = (SAPbobsCOM.BusinessPartners)this.sap.getBObject().GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

            //Main BP
            this.obp.CardType = SAPbobsCOM.BoCardTypes.cCustomer;
            this.obp.Series = series;
            this.obp.CardName = customer.CardName;
            this.obp.CardForeignName = customer.CardName;
            this.obp.GroupCode = customer.GroupCode;
            this.obp.FederalTaxID = RFC;
            this.obp.Currency = CURRENCY;
            this.obp.Phone1 = customer.Phone1;
            this.obp.EmailAddress = customer.E_Mail;
            this.obp.AdditionalID = customer.AddId;
            this.obp.UnifiedFederalTaxID = customer.VatIdUnCmp;
            this.obp.UserFields.Fields.Item("U_CodSIFCO").Value = customer.U_CodSIFCO;
            this.obp.UserFields.Fields.Item("U_FechaInicio").Value = customer.U_FechaInicio;
            this.obp.UserFields.Fields.Item("U_AgenteRet").Value = customer.U_AgenteRet.ToString();
            this.obp.UserFields.Fields.Item("U_DatosIVE").Value = customer.AddId;
            try
            {

            
                //Contact Persons
                foreach (PersonaContacto contactperson in customer.ContactPerson)
                {
                    this.obp.ContactEmployees.Name = contactperson.Name;
                    this.obp.ContactEmployees.FirstName = contactperson.Name;
                    this.obp.ContactEmployees.Phone1 = contactperson.Tel1;
                    this.obp.ContactEmployees.Address = contactperson.Address;
                    this.obp.ContactEmployees.UserFields.Fields.Item("U_TipoRef").Value = contactperson.U_TipoRef.ToString();
                    this.obp.ContactEmployees.Add();
                }
                //Addresses
                foreach (Direccion address in customer.Addresses)
                {

                    this.obp.Addresses.AddressType = (address.AddresType.ToString().Equals("B") ? SAPbobsCOM.BoAddressType.bo_BillTo : SAPbobsCOM.BoAddressType.bo_ShipTo);
                    this.obp.Addresses.AddressName = address.Address;
                    this.obp.Addresses.Street = address.Street;
                    this.obp.Addresses.Country = address.Country;
                    this.obp.Addresses.State = address.State;
                    this.obp.Addresses.County = address.Municipio;
                    this.obp.Addresses.UserFields.Fields.Item("U_TipoVivienda").Value = address.U_TipoVivienda.ToString();
                    this.obp.Addresses.UserFields.Fields.Item("U_TiempoLab").Value = address.U_TiempoLab.ToString();
                    this.obp.Addresses.UserFields.Fields.Item("U_Salario").Value = address.U_Salario.ToString();
                    this.obp.Addresses.UserFields.Fields.Item("U_NombreTrabajo").Value = address.U_NombreTrabajo.ToString();
                    this.obp.Addresses.UserFields.Fields.Item("U_TelefonoTrabajo").Value = address.U_TelefonoTrabajo;
                    this.obp.Addresses.Add();
                }
            }
            catch (Exception e)
            {
                utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge" + e.Message);
                this.db.addLog(2, 0, "", "error" + e.Message, 0);
                return new Transacciones("error", e.Message);
            }

            if (this.obp.Add() != 0)
            {

                this.sap.getBObject().GetLastError(out this.errorcode, out this.errormessage);



                this.sap.disconnect();
                utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge"+ this.errorcode + this.errormessage);
                this.db.addLog(2, 0,"", "error" + this.errorcode + this.errormessage, 0);
                return new Transacciones("error", this.errorcode + this.errormessage);
            }
            else
            {
                this.sap.getBObject().GetNewObjectCode(out CardCode);
                //Datos IVE
                this.errormessage = Udo.newDataIve(this.sap.getBObject(), customer.IVE);

                if (this.errormessage != "")
                {
                    utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge" + this.errormessage);
                    this.db.addLog(2, 0,"", "error" + this.errorcode + this.errormessage, 0);
                    
                    if (obp.GetByKey(CardCode))
                    {
                        obp.Remove();
                    }
                    return new Transacciones("error", this.errormessage);                    
                }
                    
            }

            this.sap.disconnect();
            utilities.logger.Info("success - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge" + CardCode);
            this.db.addLog(2, 1, CardCode, "success - " + CardCode,0);
            return new Transacciones("success", CardCode);
        }

        /// <summary>
        /// Actualización de Cliente
        /// </summary>
        /// <param name="customer">Recibe al cliente por crear</param>
        /// <returns>
        ///     Success: Cliente insertado correctamente;
        ///     CodError - Mensaje de Error
        /// </returns>
        public Transacciones updateCustomer(Cliente customer)
        {


            this.errormessage = this.sap.connect();

            if (this.errormessage != "")
            {
                utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge" + this.errormessage);
                this.db.addLog(2, 0, "", "error"  + this.errormessage, 0);
                return new Transacciones("error", this.errormessage);
            }
                

            this.obp = (SAPbobsCOM.BusinessPartners)this.sap.getBObject().GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners);

            if (obp.GetByKey(customer.CardCode))
            {
                this.obp.CardName = customer.CardName;
                this.obp.CardForeignName = customer.CardName;
                this.obp.GroupCode = customer.GroupCode;
                this.obp.Phone1 = customer.Phone1;
                this.obp.EmailAddress = customer.E_Mail;
                this.obp.AdditionalID = customer.AddId;
                this.obp.UnifiedFederalTaxID = customer.VatIdUnCmp;
                this.obp.UserFields.Fields.Item("U_CodSIFCO").Value = customer.U_CodSIFCO;
                this.obp.UserFields.Fields.Item("U_FechaInicio").Value = customer.U_FechaInicio;
                this.obp.UserFields.Fields.Item("U_AgenteRet").Value = customer.U_AgenteRet.ToString();
                this.obp.UserFields.Fields.Item("U_DatosIVE").Value = customer.AddId;
                
                if (this.obp.Update() != 0)
                {

                    this.sap.getBObject().GetLastError(out this.errorcode, out this.errormessage);

                    

                    this.sap.disconnect();
                    utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge - " +this.errorcode +  this.errormessage);
                    this.db.addLog(2, 0, customer.CardCode, "error" + this.errorcode + this.errormessage, 0);
                    return new Transacciones("error", this.errorcode + this.errormessage);
                }
                else
                {
                    //Datos IVE
                    this.errormessage = Udo.updateDataIve(this.sap.getBObject(), customer.IVE);
                     
                    if (this.errormessage != "")
                    {
                        utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge - " + this.errormessage);
                        this.db.addLog(2, 0, customer.CardCode, "error" + this.errorcode + this.errormessage, 0);
                        return new Transacciones("error", this.errormessage);
                    }
                        
                }
            }
            else
            {
                this.sap.disconnect();
                utilities.logger.Error("error - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge - El código de cliente enviado no es válido");
                this.db.addLog(2,  0, customer.CardCode, "error - El código de cliente enviado no es válido", 0);
                return new Transacciones("error", "El código de cliente enviado no es válido");                
            }
            this.sap.disconnect();
            utilities.logger.Info("success - " + System.Reflection.MethodBase.GetCurrentMethod().Name + " - Messagge - " + customer.CardCode);
            this.db.addLog(2, 1, customer.CardCode, "success - " + customer.CardCode, 0);
            return new Transacciones("success", customer.CardCode);
        }
    }
}