using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using REST_GestionClientes.Controllers;
using System.Text;
using System.Data;
using SAPbobsCOM;

namespace REST_GestionClientes.Models
{
    public class Acciones
    {
        #region variables de clase
        //variables comunes de la clase
        SAPbobsCOM.Company empresa = new SAPbobsCOM.Company();
        SAPbobsCOM.Recordset _rs;
        string _error = "";
        string _newObject;
        string _query;
        double _totalFactura = 0;
        Transacciones _transaccion;
        #endregion
        #region Conexion a BD 
        /// <summary>
        /// Metodo que realiza la conexión hacia SAP
        /// </summary>
        /// <returns>Devuelve una cadena cpn valor 1 si la conexion es exitosa.</returns>
        public string ConectarEmpresa()
        {
            string versionDb = "";
            Parametros p = new Parametros();
            DataTable datos = p.getSAPparameters();


            if (datos == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00021).");
                return "(00021)|Parámetros de conexión hacia la base de datos intermedia incorrectos.";
            }

            if (datos.Rows.Count == 0)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00021).");
                return "(00021)|Parámetros de conexión hacia la base de datos intermedia incorrectos.";
            }

            foreach (DataRow row in datos.Rows)
            {
                switch (row["nombre_parametro"].ToString())
                {
                    case "DbSap": { empresa.CompanyDB = row["valor"].ToString(); } break;
                    case "DbServerType": { versionDb = row["valor"].ToString(); } break;
                    case "PasswordSap": { empresa.Password = p.DesEncriptar(row["valor"].ToString()); } break;
                    case "ServerSap": { empresa.Server = row["valor"].ToString(); } break;
                    case "UserNameSap": { empresa.UserName = row["valor"].ToString(); } break;
                }
            }
            //  empresa.LicenseServer = "PROGRAMADOR2";
            switch (versionDb)
            {
                case "dst_MSSQL2008": { empresa.DbServerType = BoDataServerTypes.dst_MSSQL2008; } break;
                case "dst_MSSQL2012": { empresa.DbServerType = BoDataServerTypes.dst_MSSQL2012; } break;
                case "dst_MSSQL2014": { empresa.DbServerType = BoDataServerTypes.dst_MSSQL2014; } break;
                //case "dst_MSSQL2016": { empresa.DbServerType = BoDataServerTypes.dst_MSSQL2016; } break;
                default:
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00003).");
                        return ("(00003)|Versión de base de datos no valida, verifique los parametros de conexión.");

                    }

            }
            utilities.logger.Debug("Iniciando conexión con SAP.");
            int estadoConeccion = empresa.Connect();
            if (estadoConeccion != 0)
            {
                empresa.GetLastError(out estadoConeccion, out _error);
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00006).");
                return (
                    $"(00006)|Error de conexion hacia sap codigo: {estadoConeccion}, verifique los parametros de conexión. {_error}"
                );
            }
            utilities.logger.Debug("Conectado con SAP correctamente");
            return "1";
        }
        /// <summary>
        /// Metodo para finalizar la conexión hacia SAP.
        /// </summary>
        public void DesconectarEmpresa()
        {
            empresa.Disconnect();
        }
        #endregion
        #region Validaciones de documentos
        /// <summary>
        /// Hace las validaciones a la partida enviada.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>String con valor 1 si la partida es valida.</returns>
        public string ValidarPartida(partida p)
        {
            string ret = "1";
            decimal creditoMedio = 0, debitoMedio = 0;
            decimal creditoRubro = 0, debitoRubro = 0;
            try
            {
                if (p.tipo == 2)
                {
                    foreach (MediosDePago m in p.detallePartida.mediosDePago)
                    {
                        if (m.credito == null)
                        {
                            utilities.logger.Error(
                                $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                            return "(00023)|El parametro 'credito' no esta definido en el medio de pago.";
                        }
                        if (m.debito == null)
                        {
                            utilities.logger.Error(
                                $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                            return "(00023)|El parametro 'debito' no esta definido en el medio de pago.";
                        }
                        if (Convert.ToDecimal(m.credito) != 0 && Convert.ToDecimal(m.debito) != 0)
                        {
                            utilities.logger.Error(
                                $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00007).");
                            return "(00007)|Un medio de pago no puede tener saldo en el credito y debito al mismo tiempo";
                        }
                        creditoMedio += Convert.ToDecimal(m.credito);
                        debitoMedio += Convert.ToDecimal(m.debito);
                    }
                }
            }
            catch (Exception)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00008).");
                return "(00008)|Error el convertir los valores de debito o credito en valores numericos en el medio de pago.";
            }

            try
            {
                foreach (Rubros r in p.detallePartida.rubros)
                {
                    if (r.credito == null)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                        return "(00023)|El parametro 'credito' no esta definido.";
                    }
                    if (r.debito == null)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                        return "(00023)|El parametro 'debito' no esta definido.";
                    }
                    if (Convert.ToDecimal(r.credito) != 0 && Convert.ToDecimal(r.debito) != 0)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00007).");
                        return "(00007)|Un rubro no puede tener saldo en el credito y debito al mismo tiempo";
                    }
                    creditoRubro += Convert.ToDecimal(r.credito);
                    debitoRubro += Convert.ToDecimal(r.debito);
                }
            }
            catch (Exception)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00008).");
                return "(00008)|Error el convertir los valores de debito o credito en valores numericos.";
            }
            if (p.tipo == 2 && creditoRubro != debitoMedio)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00009).");
                return "(00009)|La partida no esta balanceada.";
            }
            if (creditoRubro != debitoRubro && p.tipo != 2)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00009).");
                return "(00009)|La partida no esta balanceada.";
            }
            return ret;
        }
        /// <summary>
        /// Valida una partida de tipo 1 para verificar que tenga los datos minimos para la funcionalidad de desembolsos.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>String con valor 1 si la partida es valida.</returns>
        public string ValidarDesembolso(partida p)
        {
            if (p.codigoCliente == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                return "(00023)|El parametro 'codigoCliente' no esta definido.";
            }
            if (p.codigoCliente == "")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                return "(00010)|El parametro 'codigoCliente' no puede estar vacio para una partida de tipo 1.";
            }
            if (p.codigoOrigenCredito == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                return "(00023)|El parametro 'codigoOrigenCredito' no esta definido.";
            }
            if (p.codigoOrigenCredito == "")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                return "(00010)|El parametro 'codigoOrigenCredito' no puede estar vacio para una partida de tipo 1.";
            }
            if (p.numeroCreditoSIFCO == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                return "(00023)|El parametro 'numeroCreditoSIFCO' no esta definido.";
            }
            if (p.numeroCreditoSIFCO == "")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                return "(00010)|El parametro 'numeroCreditoSIFCO' no puede estar vacio para una partida de tipo 1.";
            }
            foreach (Rubros r in p.detallePartida.rubros)
            {
                if (Convert.ToDecimal(r.credito) != 0)
                {
                    if (r.esAfectoAIva == null)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                        return "(00023)|El parametro 'esAfectoAIva' no esta definido.";
                    }
                    if (r.esAfectoAIva.ToLower() != "s" && r.esAfectoAIva.ToLower() != "n")
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                        return "(00010)|Los valores permitidos para el parametro 'esAfectoAIva' en un desembolso son 'N' o 'S'";
                    }
                }
                if (r.codigo == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'codigoCuentaMayor' no esta definido.";
                }
                if (r.codigo == "")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'codigoCuentaMayor' no puede estar vacio para una partida de tipo 1.";
                }
            }

            return "1";
        }
        /// <summary>
        /// Valida una partida de tipo 2 para verificar que tenga los datos minimos para la funcionalidad de Cuotas.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>String con valor 1 si la partida es valida.</returns>
        public string ValidarCuota(partida p)
        {
            int cont;

            if (p.codigoCliente == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                return "(00023)|El parametro 'codigoCliente' no esta definido.";
            }
            if (p.codigoCliente == "")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                return "(00010)|El parametro 'codigoCliente' no puede estar vacio para una partida de tipo 2.";
            }
            if (p.codigoOrigenCredito == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                return "(00023)|El parametro 'codigoOrigenCredito' no esta definido.";
            }
            if (p.codigoOrigenCredito == "")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                return "(00010)|El parametro 'codigoOrigenCredito' no puede estar vacio para una partida de tipo 2.";
            }
            if (p.numeroCreditoSIFCO == null)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                return "(00023)|El parametro 'numeroCreditoSIFCO' no esta definido.";
            }
            if (p.numeroCreditoSIFCO == "")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                return "(00010)|El parametro 'numeroCreditoSIFCO' no puede estar vacio para una partida de tipo 2.";
            }

            foreach (MediosDePago r in p.detallePartida.mediosDePago)
            {
                cont = 0;
                if (r.esCheque == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esCheque' no esta definido.";
                }
                if (r.esCheque.ToLower() != "s" && r.esCheque.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'esCheque' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }
                if (r.esTransferencia == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esTransferencia' no esta definido.";
                }
                if (r.esTransferencia.ToLower() != "s" && r.esTransferencia.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'esTransferencia' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }
                if (r.esEfectivo == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esEfectivo' no esta definido.";
                }
                if (r.esEfectivo.ToLower() != "s" && r.esEfectivo.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'esEfectivo' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }
                if (r.esDeposito == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esDeposito' no esta definido.";
                }
                if (r.esDeposito.ToLower() != "s" && r.esDeposito.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'esDeposito' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }

                //Datos cheques
                if (r.esCheque.ToLower() == "s")
                {
                    if (r.fecha == null)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                        return "(00023)|El parametro 'fecha' no esta definido.";
                    }
                    if (r.fecha == "")
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                        return "(00010)|Para rubros que son medios de pago con 'esCheque=\"S\"', el parametro 'fecha' no puede estar vacio.";
                    }
                    if (!IsDate(r.fecha)) return "(00022)|Fecha en el medio de pago (Cheque) incorrecta.";
                    if (r.nombreBanco == null)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                        return "(00023)|El parametro 'nombreBanco' no esta definido.";
                    }
                    if (r.nombreBanco == "")
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                        return "(00010)|Para rubros que son medios de pago con 'esCheque=\"S\"', el parametro 'nombreBanco' no puede estar vacio.";
                    }
                    if (r.numeroDocumento == null)
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                        return "(00023)|El parametro 'numeroDocumento' no esta definido.";
                    }
                    if (r.numeroDocumento == "")
                    {
                        utilities.logger.Error(
                            $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                        return "(00010)|Para rubros que son medios de pago con 'esCheque=\"S\"', el parametro 'numeroDocumento' no puede estar vacio.";
                    }
                }


                if (r.esCheque.ToLower() == "s") cont++;
                if (r.esDeposito.ToLower() == "s") cont++;
                if (r.esEfectivo.ToLower() == "s") cont++;
                if (r.esTransferencia.ToLower() == "s") cont++;
                if (cont > 1)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00012).");
                    return "(00012)|Un medio de pago no puede tener mas de un parametro de los siguientes {esDeposito, esEfectivo, esTransferencia, esCheque} con valor 'S'";
                }

            }

            foreach (Rubros r in p.detallePartida.rubros)
            {

                cont = 0;
                if (r.codigo == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'codigoCuentaMayor' no esta definido.";
                }
                if (r.codigo == "")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'codigoCuentaMayor' no puede estar vacio para una partida de tipo 2.";
                }
                if (r.facturable == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'facturable' no esta definido.";
                }
                if (r.facturable.ToLower() != "s" && r.facturable.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'facturable' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }
                if (r.esCapital == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esCapital' no esta definido.";
                }
                if (r.esCapital.ToLower() != "s" && r.esCapital.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'esCapital' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }

                if (r.esAfectoAIva == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esAfectoAIva' no esta definido.";
                }
                if (r.esAfectoAIva.ToLower() != "s" && r.esAfectoAIva.ToLower() != "n")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'esAfectoAIva' no puede estar vacio y los valores permitidos son 'S' o 'N' para una partida de tipo 2.";
                }

                if (r.esCapital == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'esCapital' no esta definido.";
                }

                if (r.facturable == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'facturable' no esta definido.";
                }

                if (r.esCapital.ToLower() == "s") cont++;
                if (r.facturable.ToLower() == "s") cont++;
                if (cont > 1)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00012).");
                    return "(00012)|Un Rubro no puede tener mas de un parametro de los siguientes {esCapital, facturable} con valor 'S'";
                }

            }

            return "1";
        }
        /// <summary>
        /// Valida una partida de tipo 3 para verificar que tenga los datos minimos para la funcionalidad de Cuotas.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>String con valor 1 si la partida es valida.</returns>
        public string ValidarAjuste(partida p)
        {
            foreach (Rubros r in p.detallePartida.rubros)
            {
                if (r.codigo == null)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00023).");
                    return "(00023)|El parametro 'codigoCuentaMayor' no esta definido.";
                }
                if (r.codigo == "")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00010).");
                    return "(00010)|El parametro 'codigoCuentaMayor' de cada rubro no puede estar vacio para una partida de tipo 3.";
                }
            }
            return "1";
        }
        #endregion 
        #region Acciones Desembolso
        /// <summary>
        /// Ejecuta las acciones de un desembolso.
        /// </summary>
        /// <param name="partida"></param>
        /// <returns>String con valos 1 si el proceso se ejecuta correctamente.</returns>
        public string EjecutarDesembolso(partida partida)
        {
            string salida = "1";
            if (partida == null) return "(00001)|Estructura del archivo Json incorrecta.";
            empresa.StartTransaction();

            salida = CrearNotaDeDebito(partida);
            if (salida != "1")
            {
                empresa.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return salida;
            }
            try
            {
                empresa.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                return "(00025)|" + e.Message;
            }

            return salida;
        }
        /// <summary>
        /// Crea una nota de debito de Clientes en SAP.
        /// </summary>
        /// <param name="partida"></param>
        /// <returns>String con valos 1 si el proceso se ejecuta correctamente.</returns>
        public string CrearNotaDeDebito(partida partida)
        {

            SAPbobsCOM.Documents notaDebito = (SAPbobsCOM.Documents)empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
            _rs = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            _query = $"SELECT * FROM [{empresa.CompanyDB}].[dbo].[@PARAMETROS_INTERFAS]";
            _rs.DoQuery(_query);
            int serie = 0;
            int condicionDePago = 0;
            //Obtencion de parámetros 
            while (!_rs.EoF)
            {
                if (_rs.Fields.Item("Name").Value.ToString() == "serieDesembolso")
                {
                    serie = Convert.ToInt32(_rs.Fields.Item("U_valor").Value.ToString());
                    break;
                }
                _rs.MoveNext();
            }
            _rs.MoveFirst();
            while (!_rs.EoF)
            {
                if (_rs.Fields.Item("Name").Value.ToString() == "condicionDePago")
                {
                    condicionDePago = Convert.ToInt32(_rs.Fields.Item("U_valor").Value.ToString());
                    break;
                }
                _rs.MoveNext();
            }
            if (serie == 0 || condicionDePago == 0)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00013).");
                return "(00013)|El número de serie, o el codigo de la condición de pago no es correcto, verificar tabla de parametros en SAP.";
            }
            notaDebito.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Service;//tipo de documento
            notaDebito.CardCode = partida.codigoCliente;//cliente
            if (!IsDate(partida.fechaContabilizacion)) return "(00022)|Fecha de contabilizacion incorrecta.";
            notaDebito.DocDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);//fecha de contabilizacion del documento
            if (!IsDate(partida.fechaVencimiento)) return "(00022)|Fecha de vencimiento incorrecta.";
            notaDebito.DocDueDate = DateTime.ParseExact(partida.fechaVencimiento, "yyyy-MM-dd", null);//fecha de contabilizacion del documento
            notaDebito.DocumentSubType = SAPbobsCOM.BoDocumentSubType.bod_DebitMemo;//tipo de documento nota de debito
            notaDebito.DocCurrency = "GTQ";//moneda del documento
            notaDebito.PaymentGroupCode = condicionDePago;//condicion de pago
            notaDebito.Series = serie;//serie del documento
            notaDebito.NumAtCard = partida.numeroCreditoSIFCO;
            string codigoCuentaTransitoriaDesembolso = string.Empty;
            double monto = 0;
            foreach (Rubros detalle in partida.detallePartida.rubros)
            {

                if (Convert.ToDouble(detalle.credito) > 0)
                {
                    if (detalle != partida.detallePartida.rubros.First()) notaDebito.Lines.Add();
                    monto = Convert.ToDouble(detalle.credito);
                    _query = GetQueryCuentas(detalle.codigo);
                    if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'codigo' es incorrecto.";
                    codigoCuentaTransitoriaDesembolso = GetCodigoCuenta(detalle.codigo);
                    notaDebito.Lines.AccountCode = codigoCuentaTransitoriaDesembolso;// cuenta mayor
                    if (notaDebito.Lines.AccountCode == "0") return (
                        $"(00026)|No se ha podido encontrar la cuenta contable {detalle.codigo} en la base de datos de SAP."
                    );
                    //impuesto
                    if (detalle.esAfectoAIva.ToLower() == "s")
                    {
                        notaDebito.Lines.TaxCode = "IVA";
                    }
                    else
                    {
                        notaDebito.Lines.TaxCode = "EXE";
                    }
                    //precio bruto

                    notaDebito.Lines.PriceAfterVAT = monto;
                    //descripcion del item
                    //notaDebito.Lines.ItemDescription = detalle.descripcion;
                    notaDebito.Lines.Add();
                }

            }

            _query = $"select * from [@ORCRED] where Code='{partida.codigoOrigenCredito}'";
            if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'codigoOrigenCredito' incorrecto.";
            //campos de usuario
            notaDebito.UserFields.Fields.Item("U_CreditoSIFCO").Value = partida.numeroCreditoSIFCO;
            notaDebito.UserFields.Fields.Item("U_OrigenCredito").Value = partida.codigoOrigenCredito;
            notaDebito.UserFields.Fields.Item("U_DoctoFiscal").Value = "N";

            int res = notaDebito.Add();
            if (res != 0)
            {
                empresa.GetLastError(out res, out _error);
                utilities.Error = _error + "|131";
                return _error;
            }
            empresa.GetNewObjectCode(out _newObject);
            notaDebito.GetByKey(Convert.ToInt32(_newObject));
            int docNum = notaDebito.DocNum;
            _transaccion = new Transacciones("success", docNum + "|Documento Creado: Nota de débito de clientes|131");
            utilities.mensajeSuccess.transacciones.Add(_transaccion);
            //llamado para la creación del asiento contable.
            string salida = CrearAsientoNotaDebito(docNum, partida, codigoCuentaTransitoriaDesembolso, monto);
            if (salida != "1") return salida;
            return "1";
        }
        /// <summary>
        /// Crea el Asiento contable para trasladar la deuda de un desembolso a un proveedor.
        /// </summary>
        /// <param name="docNum">numero de la nota de debito (Desembolso)</param>
        /// <param name="partida">Json de la partida</param>
        /// <param name="codigoCuentaTransitoriaDesembolso"> codigo de la cuenta Transitoria de desembolsos</param>
        /// <param name="monto">monto del desembolso</param>
        /// <returns></returns>
        private string CrearAsientoNotaDebito(int docNum, partida partida, string codigoCuentaTransitoriaDesembolso, double monto)
        {
            int diasExtras = -169;
            SAPbobsCOM.JournalEntries asiento = (SAPbobsCOM.JournalEntries)empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
            _rs = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
           // _query = $"SELECT * FROM [{empresa.CompanyDB}].[dbo].[t]";
            _query = $"SELECT * FROM [{empresa.CompanyDB}].[dbo].[@PARAMETROS_INTERFAS]";
            _rs.DoQuery(_query);
            string codigoTransaccion = "";
            while (!_rs.EoF)
            {
                if (_rs.Fields.Item("Name").Value.ToString() == "diasExtras")
                {
                    diasExtras = Convert.ToInt32(_rs.Fields.Item("U_valor").Value.ToString());
                }
                else if (_rs.Fields.Item("Name").Value.ToString() == "codigoTransaccion")
                {
                    codigoTransaccion = (_rs.Fields.Item("U_valor").Value.ToString());
                }
                _rs.MoveNext();
            }
            if (diasExtras == -169)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00014).");
                return "(00014)|El valor del parámetro diasExtras no es correcto o bien no existe dicho parámetro, verificar la tabla de parametros en SAP.";
            }
            if (string.IsNullOrEmpty(codigoTransaccion))
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00014).");
                return "(00014)|El valor del parámetro codigoTransaccion no es correcto o bien no existe dicho parámetro, verificar la tabla de parametros en SAP.";
            }
            asiento.ReferenceDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            asiento.DueDate = (DateTime.ParseExact(partida.fechaVencimiento, "yyyy-MM-dd", null).AddDays(diasExtras));
            string fecha = asiento.DueDate.ToString();
            asiento.TaxDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            asiento.Memo = $"Desembolso {docNum} - Crédito {partida.numeroCreditoSIFCO}";
            asiento.TransactionCode = codigoTransaccion;//codigo de transaccion DSMB
            asiento.Reference = docNum.ToString();

            _query = $"SELECT U_Proveedor FROM [@ORCRED] WHERE Code='{partida.codigoOrigenCredito}'";
            _rs.DoQuery(_query);
            if (_rs.RecordCount <= 0)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00017).");
                return $"(00017)|Código de origen de crédito SIFCO ({partida.codigoOrigenCredito}) invalido";
            }
            string cuentaProveedor = _rs.Fields.Item("U_Proveedor").Value.ToString();

            //Linea de credito
            asiento.Lines.ShortName = codigoCuentaTransitoriaDesembolso; //nombre corto de la cuenta
            asiento.Lines.AccountCode = codigoCuentaTransitoriaDesembolso; //codigo de la cuenta
            asiento.Lines.ContraAccount = cuentaProveedor;//codigo de la cuenta en contra
            asiento.Lines.Credit = 0; // credito de la linea
            asiento.Lines.Debit = monto;//debito de la linea
            asiento.Lines.Add();
            //Linea de debito
            asiento.Lines.ShortName = cuentaProveedor; //nombre corto de la cuenta
            asiento.Lines.AccountCode = ValidarCodigoCliente(cuentaProveedor, false); //codigo de la cuenta
            asiento.Lines.ContraAccount = codigoCuentaTransitoriaDesembolso;//codigo de la cuenta en contra
            asiento.Lines.Credit = monto; // credito de la linea
            asiento.Lines.Debit = 0;//debito de la linea
            asiento.Lines.Add();

            int res = asiento.Add();
            if (res != 0)
            {
                empresa.GetLastError(out res, out _error);
                utilities.Error = _error + "|30";
                return _error;
            }
            empresa.GetNewObjectCode(out _newObject);
            asiento.GetByKey(Convert.ToInt32(_newObject));
            int numeroAsiento = asiento.Number;
            _transaccion = new Transacciones("success", numeroAsiento + "|Documento Creado: Asiento|30");
            utilities.mensajeSuccess.transacciones.Add(_transaccion);
            return "1";
        }


        #endregion
        #region Acciones Cuotas
        /// <summary>
        /// Ejecuta las acciones de la funcionalidad Cuotas.
        /// </summary>
        /// <param name="partida"></param>
        /// <returns>String von valor 1 si el proceso es exitoso.</returns>
        public string EjecutarPagoCuota(partida partida)
        {
            string salida = "1";
            if (partida == null) return "(00001)|Estructura del archivo Json incorrecta.";

            empresa.StartTransaction();
            utilities.logger.Debug("Creando factura..");
            salida = CrearFactura(partida);
            if (salida != "1")
            {

                empresa.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return salida;
            }
            try
            {
                empresa.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                return "(00025)|" + e.Message;
            }

            return salida;
        }
        /// <summary>
        /// Busca entre los rubros de la partida si hay rubros facturables.
        /// </summary>
        /// <param name="partida"></param>
        /// <returns></returns>
        private string CrearFactura(partida partida)
        {
            string ret = "1";

            List<Rubros> rublosFacturables = new List<Rubros>();
            List<MediosDePago> cheques = new List<MediosDePago>();
            List<MediosDePago> depositos = new List<MediosDePago>();
            List<MediosDePago> transferencias = new List<MediosDePago>();
            List<Rubros> pagosACapital = new List<Rubros>();
            List<MediosDePago> pagosEnEfectivo = new List<MediosDePago>();

            foreach (MediosDePago rubroActual in partida.detallePartida.mediosDePago)
            {

                if (rubroActual.esCheque.ToLower() == "s")
                {
                    utilities.logger.Debug("Medio de pago con cheque encontrado.");
                    _query = GetQueryCuentas(rubroActual.codigoCuentaMayor);
                    if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'codigoCuentaMayor' incorrecto.";
                    cheques.Add(rubroActual);
                }
                else if (rubroActual.esDeposito.ToLower() == "s")
                {
                    utilities.logger.Debug("Medio de pago con deposito encontrado.");
                    _query = GetQueryCuentas(rubroActual.codigoCuentaMayor);
                    if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'codigoCuentaMayor' incorrecto.";
                    depositos.Add(rubroActual);
                }
                else if (rubroActual.esEfectivo.ToLower() == "s")
                {
                    utilities.logger.Debug("Medio de pago en efectivo encontrado.");
                    _query = GetQueryCuentas(rubroActual.codigoCuentaMayor);
                    if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'codigoCuentaMayor' incorrecto.";
                    pagosEnEfectivo.Add(rubroActual);
                }
                else if (rubroActual.esTransferencia.ToLower() == "s")
                {
                    utilities.logger.Debug("Medio de pago con transferencia encontrado.");
                    _query = GetQueryCuentas(rubroActual.codigoCuentaMayor);
                    if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'codigoCuentaMayor' incorrecto.";
                    transferencias.Add(rubroActual);
                }

            }
            foreach (Rubros rubroActual in partida.detallePartida.rubros)
            {
                if (rubroActual.facturable.ToLower() == "s")
                {
                    utilities.logger.Debug("Rublo facturable encontrado");
                    rublosFacturables.Add(rubroActual);
                }
                else if (rubroActual.esCapital.ToLower() == "s")
                {
                    utilities.logger.Debug("Pago a capital encontrado");
                    pagosACapital.Add(rubroActual);

                }

            }

            bool hayFactura = false;
            if (rublosFacturables.Count > 0)
            {
                utilities.logger.Debug("Creando factura para los rublos facturables");
                ret = AccionesRublosFacturables(rublosFacturables, partida);
                if (ret != "1") return ret + "|132";
                utilities.logger.Debug("Factura creada correctamente");
                hayFactura = true;
            }
            utilities.logger.Debug("Efectuando abonos");
            ret = EfectuarAbonos(partida, cheques, pagosEnEfectivo, transferencias, depositos, pagosACapital, hayFactura);
            if (ret != "1") return ret + "|24";
            utilities.logger.Debug("Abonos efectuados correctamente.");
            return ret;
        }
        /// <summary>
        /// Crea la factura de Deudores si existen rubros que sean facturables.
        /// </summary>
        /// <param name="rublosFacturables"></param>
        /// <param name="partida"></param>
        /// <returns></returns>
        private string AccionesRublosFacturables(List<Rubros> rublosFacturables, partida partida)
        {
            string ret = "1";

            SAPbobsCOM.Documents factura = (SAPbobsCOM.Documents)empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);
            _rs = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            //Query hacia la tabla de parametros en SAP
            _query = $"SELECT * FROM [{empresa.CompanyDB}].[dbo].[@PARAMETROS_INTERFAS]";
            _rs.DoQuery(_query);
            int serie = 0;
            int condicionDePago = 0;
            string tipoCompraVenta = string.Empty, agenciaFacturaDeudor = string.Empty, deptoFacturaDeudor = string.Empty;
            //Obtencion de parametros de la tabla Para @PARAMETROS_INTERFAS en SAP (UDO)
            utilities.logger.Debug("Buscando los parámetros serieFacturas,tipoCompraVenta,condicionDePago,agenciaFacturaDeudor,deptoFacturaDeudor en SAP.");
            while (!_rs.EoF)
            {
                try
                {
                    switch ((string)_rs.Fields.Item("Name").Value.ToString())
                    {
                        case "serieFacturas": { serie = Convert.ToInt32(_rs.Fields.Item("U_valor").Value.ToString()); } break;
                        case "tipoCompraVenta": { tipoCompraVenta = _rs.Fields.Item("U_valor").Value.ToString(); } break;
                        case "condicionDePago": { condicionDePago = Convert.ToInt32(_rs.Fields.Item("U_valor").Value.ToString()); } break;
                        case "agenciaFacturaDeudor": { agenciaFacturaDeudor = _rs.Fields.Item("U_valor").Value.ToString(); } break;
                        case "deptoFacturaDeudor": { deptoFacturaDeudor = _rs.Fields.Item("U_valor").Value.ToString(); } break;
                    }
                    _rs.MoveNext();
                }
                catch (Exception)
                {
                    return "(00014)|Verificar los valores de la tabla de parametros en SAP, posibles valores nulos o incorrectos.";
                }

            }
            //Validación de que se hallan obtenido los valores desde el UDO
            if (serie == 0 || condicionDePago == 0)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00014).");
                return "(00014)|El número de serie, o el codigo de la condición de pago para las facturas de deudor no es el indicado, verificar tabla de parametros en SAP.";
            }
            if (string.IsNullOrEmpty(tipoCompraVenta))
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00014).");
                return "(00014)|El valor del parámetro tipoCompraVenta no es correcto o bien no existe dicho parámetro, verificar la tabla de parametros en SAP.";
            }
            if (string.IsNullOrEmpty(agenciaFacturaDeudor))
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00014).");
                return "(00014)|El valor del parámetro agenciaFacturaDeudor no es correcto o bien no existe dicho parámetro, verificar la tabla de parametros en SAP.";
            }
            if (string.IsNullOrEmpty(deptoFacturaDeudor))
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00014).");
                return "(00014)|El valor del parámetro deptoFacturaDeudor no es correcto o bien no existe dicho parámetro, verificar la tabla de parametros en SAP.";
            }
            utilities.logger.Debug("Parámetros en SAP encontrados.");
            //Se especifica que tipo de factura sera, si de servicios o artículos.
            factura.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Service;
            factura.CardCode = partida.codigoCliente;
            if (!IsDate(partida.fechaContabilizacion)) return "(00022)|Fecha de contabilizacion incorrecta.";
            factura.DocDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            if (!IsDate(partida.fechaVencimiento)) return "(00022)|Fecha de contabilizacion incorrecta.";
            factura.DocDueDate = DateTime.ParseExact(partida.fechaVencimiento, "yyyy-MM-dd", null);
            factura.Series = serie;
            //Creación lineas de articulos
            foreach (Rubros item in rublosFacturables)
            {
                utilities.logger.Debug("Agregando linea en la factura por un rublo facturable.");
                if (item != rublosFacturables.First()) factura.Lines.Add();
                //Se Asigna el código de cuenta o artículo a cada linea de la factura.

                factura.Lines.AccountCode = GetCodigoCuenta(item.codigo);
                if (factura.Lines.AccountCode == "0") return (
                    $"(00026)|No se ha podido encontrar la cuenta contable {item.codigo} en la base de datos de SAP.");
                _query =
                    $"SELECT * FROM [{empresa.CompanyDB}].[dbo].[@PARAMETROS_INTERFAS] WHERE Name =\'{item.codigo}\'";
                _rs.DoQuery(_query);
                if (_rs.RecordCount > 0)
                {
                    factura.Lines.ItemDescription = _rs.Fields.Item("U_valor").Value.ToString();
                }
                else
                {
                    _query = $"SELECT AcctName FROM OACT WHERE AcctCode='{factura.Lines.AccountCode}'";
                    _rs.DoQuery(_query);
                    factura.Lines.ItemDescription = _rs.Fields.Item("AcctName").Value.ToString();
                }
                //factura.Lines.ItemDescription = getCodigoCuenta(item.codigo);
                if (Convert.ToDecimal(item.debito) == 0 && Convert.ToDecimal(item.credito) != 0)
                {
                    factura.Lines.PriceAfterVAT = Convert.ToDouble(item.credito);
                    _totalFactura += factura.Lines.PriceAfterVAT;
                }
                else
                {
                    factura.Lines.PriceAfterVAT = Convert.ToDouble(item.debito);
                    _totalFactura += factura.Lines.PriceAfterVAT;
                }
                // tipo de impuesto que afectara a ala linea
                if (item.esAfectoAIva.ToLower() == "s")
                {
                    factura.Lines.TaxCode = "IVA";
                }
                else
                {
                    factura.Lines.TaxCode = "EXE";
                }
                //query = (string.Format("SELECT U_TipoCV FROM OITM WHERE ItemCode='{0}'", item.codigo));
                //query=string.Format("SELECT * FROM [{0}].[dbo].[{1}]", empresa.CompanyDB, "@PARAMETROS_INTERFAS");
                //rs.DoQuery(query);
                //if (rs.RecordCount <= 0)
                //{
                //    utilities.logger.Error(string.Format("Error en metodo: {0} código de error (00015).", System.Reflection.MethodBase.GetCurrentMethod().Name));
                //    return string.Format("(00015)|Código de cuenta invalido ({0})", item.codigo);
                //}

                factura.Lines.UserFields.Fields.Item("U_Tipo").Value = tipoCompraVenta;  //Tipo Compra\Venta (CDU)
                factura.Lines.CostingCode = agenciaFacturaDeudor;//Agencia
                factura.Lines.CostingCode2 = deptoFacturaDeudor;//DEpto/area
                utilities.logger.Debug("Linea agregada en la factura.");
            }
            //obtención de la serie y número de factura de la resolución indicada en la tabla de parametros SAP.
            _query =
                "select Code,U_Siguiente,U_Serie from [@FE_RES] where Code = (select U_valor from [@PARAMETROS_INTERFAS] where Name=\'resolucionFacturas\')";
            _rs.DoQuery(_query);
            string facturaSiguiente = "";
            string facturasSerie = "";
            string codigoResolucion = "";
            if (_rs.RecordCount > 0)
            {
                facturaSiguiente = (_rs.Fields.Item("U_Siguiente").Value.ToString());
                facturasSerie = _rs.Fields.Item("U_Serie").Value.ToString();
                codigoResolucion = _rs.Fields.Item("Code").Value.ToString();
            }
            else
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00016).");
                return "(00016)|No se pudierón encontrar los parámetros serie y numero de factura ";
            }
            _query = $"SELECT * FROM OCRD WHERE CardCode='{partida.codigoCliente}'";
            _rs.DoQuery(_query);
            if (_rs.RecordCount <= 0)
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00019).");
                return "(00019)|El codigo de cliente no es valido o es incorrecto.";
            }
            utilities.logger.Debug("Agregando campos de usuario a la factura.");
            //Campos de usuarios
            factura.UserFields.Fields.Item("U_FE_Res").Value = codigoResolucion;
            factura.UserFields.Fields.Item("U_DoctoFiscal").Value = "S";
            factura.UserFields.Fields.Item("U_DoctoSerie").Value = facturasSerie;
            factura.UserFields.Fields.Item("U_DoctoNo").Value = facturaSiguiente;
            factura.UserFields.Fields.Item("U_Nit").Value = _rs.Fields.Item("AddID").Value;
            factura.UserFields.Fields.Item("U_Nombre").Value = _rs.Fields.Item("CardName").Value;
            factura.UserFields.Fields.Item("U_CreditoSIFCO").Value = partida.numeroCreditoSIFCO;
            factura.UserFields.Fields.Item("U_OrigenCredito").Value = partida.codigoOrigenCredito;
            factura.PaymentGroupCode = condicionDePago;
            factura.NumAtCard = facturasSerie + "-" + facturaSiguiente;
            utilities.logger.Debug("Campos de usuario agregados.");
            //agregado de la factura hacia SAP
            utilities.logger.Debug("Ingresando la factura a SAP.");
            int res = factura.Add();
            if (res != 0)
            {
                empresa.GetLastError(out res, out _error);
                utilities.Error = _error + "|132";
                return _error;
            }
            utilities.logger.Debug("Factura ingresada correctamente en SAP.");
            empresa.GetNewObjectCode(out _newObject);
            factura.GetByKey(Convert.ToInt32(_newObject));
            int docNum = factura.DocNum;
            _totalFactura = factura.DocTotal;
            _transaccion = new Transacciones("success", docNum + "|Documento Creado: Factura de deudores|132");
            utilities.mensajeSuccess.transacciones.Add(_transaccion);
            return ret;
        }
        /// <summary>
        /// Crea un Pago Recibido en SAP. 
        /// </summary>
        /// <param name="partida"></param>
        /// <param name="cheques"></param>
        /// <param name="efectivo"></param>
        /// <param name="transferencias"></param>
        /// <param name="depositos"></param>
        /// <param name="pagosACapital"></param>
        /// <param name="hayFactura"></param>
        /// <returns></returns>
        private string EfectuarAbonos(partida partida, List<MediosDePago> cheques, List<MediosDePago> efectivo, List<MediosDePago> transferencias, List<MediosDePago> depositos, List<Rubros> pagosACapital, bool hayFactura)
        {
            SAPbobsCOM.Payments pago = (SAPbobsCOM.Payments)empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
            _rs = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            _query = $"SELECT * FROM [{empresa.CompanyDB}].[dbo].[@PARAMETROS_INTERFAS]";
            _rs.DoQuery(_query);
            int serie = 0;
            utilities.logger.Debug("Buscando la serie para la creación del pago recibido en SAP");
            while (!_rs.EoF)
            {
                if (_rs.Fields.Item("Name").Value.ToString() == "seriePagoRecibido")
                {
                    serie = Convert.ToInt32(_rs.Fields.Item("U_valor").Value.ToString());
                    break;
                }
                _rs.MoveNext();
            }
            if (serie == 0)
            {
                utilities.logger.Error($"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00020).");
                return "(00020)|El número de serie para los pagos recibidos es incorrecto, verificar tabla de parametros en SAP.";
            }
            utilities.logger.Debug("Serie encontrada.");
            pago.CardCode = partida.codigoCliente;
            if (!IsDate(partida.fechaContabilizacion)) return "(00022)|Fecha de contabilizacion incorrecta.";
            if (!IsDate(partida.fechaVencimiento)) return "(00022)|Fecha de contabilizacion incorrecta.";
            pago.DocDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            pago.DueDate = DateTime.ParseExact(partida.fechaVencimiento, "yyyy-MM-dd", null);
            pago.TaxDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            pago.Series = serie;
            pago.DocCurrency = "GTQ";

            // Cambio para agregar el número de boleta del banco al campo de la referencia 2 en el asiento contable - Alex García - 04/08/2021
            // Obtiene el número de la boleta
            foreach (MediosDePago objMediosPago in partida.detallePartida.mediosDePago)
            {
                string strBoleta = objMediosPago.numeroDocumento.Trim().Replace("\0", "").Substring(0,8);
                pago.Reference2 = strBoleta;
            }

            int total_pagos = pagosACapital.Count();
            int contador = 0;

            foreach (Rubros r in pagosACapital)
            {
                contador = contador + 1;
                utilities.logger.Debug("Creando pago a capital");
                if (r != pagosACapital.First()) pago.Invoices.Add();
                _query = $"SELECT * FROM OINV WHERE U_CreditoSIFCO='{partida.numeroCreditoSIFCO}' AND DocSubType='DN'";
                _rs.DoQuery(_query);
                if (_rs.RecordCount <= 0)
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00017).");
                    return $"(00017)|Número de credito SIFCO ({partida.numeroCreditoSIFCO}) invalido";
                }

                pago.Invoices.DocEntry = Convert.ToInt32(_rs.Fields.Item("DocEntry").Value.ToString());
                pago.Invoices.SumApplied = Convert.ToDecimal(r.debito) == 0 ? Convert.ToDouble(r.credito) : Convert.ToDouble(r.debito);
                if ( contador == total_pagos) {
                    pago.Invoices.Add();
                }
                utilities.logger.Debug("Pagos a capital agregados.");
            }

            int Docentry_ra = Convert.ToInt32(_rs.Fields.Item("DocEntry").Value.ToString());
            if (hayFactura)
            {
                pago.Invoices.DocEntry = Convert.ToInt32(_newObject);
                pago.Invoices.SumApplied = _totalFactura;
                pago.Invoices.Add();
            }
            //pagos en efectivo
            foreach (MediosDePago rEfectivo in efectivo)
            {
                utilities.logger.Debug("Agregando medio de pago en efectivo.");
                pago.CashAccount = GetCodigoCuenta(rEfectivo.codigoCuentaMayor);
                if (pago.CashAccount == "0") return (
                    $"(00026)|No se ha podido encontrar la cuenta contable {rEfectivo.codigoCuentaMayor} en la base de datos de SAP."
                );
                pago.CashSum = Convert.ToDecimal(rEfectivo.credito) == 0 ? Convert.ToDouble(rEfectivo.debito) : Convert.ToDouble(rEfectivo.credito);
                utilities.logger.Debug("Medio de pago en efectivo agregado.");
            }

            //pagos con checkes
            foreach (MediosDePago medio in cheques)
            {
                utilities.logger.Debug("Agregando medio de pago en cheque.");
                if (cheques.First() != medio) pago.Checks.Add();
                utilities.logger.Debug("obteniendo codigo de cuenta para el cheque.");
                pago.CheckAccount = GetCodigoCuenta(medio.codigoCuentaMayor);
                if (pago.CheckAccount == "0") return (
                    $"(00026)|No se ha podido encontrar la cuenta contable {medio.codigoCuentaMayor} en la base de datos de SAP."
                );
                utilities.logger.Debug($"{pago.CheckAccount}");
                utilities.logger.Debug("Agregando fecha al cheque");
                pago.Checks.DueDate = Convert.ToDateTime(medio.fecha);
                utilities.logger.Debug($"{pago.Checks.DueDate}");
                utilities.logger.Debug("Agregando numero de cheque");
                pago.Checks.CheckNumber = Convert.ToInt32(medio.numeroDocumento);
                utilities.logger.Debug($"{ pago.Checks.CheckNumber}");
                utilities.logger.Debug("Codigo de ciudad");
                pago.Checks.CountryCode = "GT";
                utilities.logger.Debug($"{ pago.Checks.CountryCode}");
                utilities.logger.Debug("Agregando nombre del banco");
                pago.Checks.BankCode = medio.nombreBanco;
                utilities.logger.Debug($"{  pago.Checks.BankCode}");
                utilities.logger.Debug("Agregando endoso");
                pago.Checks.Trnsfrable = medio.endosoCheque.ToLower() == "s" ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;
                utilities.logger.Debug("Agregando monto");
                pago.Checks.CheckSum = Convert.ToDecimal(medio.credito) == 0 ? Convert.ToDouble(medio.debito) : Convert.ToDouble(medio.credito);
                utilities.logger.Debug($"{pago.Checks.CheckSum}");
                utilities.logger.Debug("Medio de pago en cheque agregado.");
            }

            if (transferencias.Count > 0 || depositos.Count > 0)
            {
                utilities.logger.Debug("Agregando medio de pago en depositos y transferencias.");
                GeneralDataCollection rows;
                GeneralData rowData;
                CompanyService serviceCompany = empresa.GetCompanyService();
                var servicioGeneral = serviceCompany.GetGeneralService("FRPAGO");
                var datosServicio = (GeneralData)servicioGeneral.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData);
                datosServicio.SetProperty("U_CardCode", partida.codigoCliente);
                StringBuilder comentario = new StringBuilder();
                if (transferencias.Count > 0) comentario.Append(transferencias.First().descripcion);
                bool hayDetalle = false;

                //transferencias
                foreach (MediosDePago rTransferencias in transferencias)
                {
                    if (Convert.ToInt16(rTransferencias.moneda) == 1)
                    {
                        rTransferencias.moneda = "GTQ";
                    }
                    else {
                        rTransferencias.moneda = "USD";
                    }
                    utilities.logger.Debug("Agregando medio de pago como transferencia.");
                    pago.TransferAccount = GetCodigoCuenta(rTransferencias.codigoCuentaMayor);
                    if (pago.TransferAccount == "0") return ($"(00026)|No se ha podido encontrar la cuenta contable {rTransferencias.codigoCuentaMayor} en la base de datos de SAP."
                    );
                    pago.TransferSum += Convert.ToDecimal(rTransferencias.credito) == 0 ? Convert.ToDouble(rTransferencias.debito) : Convert.ToDouble(rTransferencias.credito);
                    if (!((string.IsNullOrEmpty(rTransferencias.nombreBanco) && string.IsNullOrEmpty(rTransferencias.numeroDocumento)) || string.IsNullOrEmpty(rTransferencias.numeroDocumento)))
                    {
                        hayDetalle = true;
                        if (transferencias.Count == 1)
                        {

                            MediosDePago transferencia = transferencias.First();
                            pago.TransferDate = Convert.ToDateTime(transferencia.fecha);
                        }

                        rows = datosServicio.Child("FRPAGO2");

                        _query = $"SELECT * FROM ODSC WHERE BankCode='{rTransferencias.nombreBanco}'";
                        if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'nombreBanco' incorrecto.";
                        var banco = rTransferencias.nombreBanco;
                        var numeroDoc = rTransferencias.numeroDocumento;
                        var fecha = rTransferencias.fecha;
                        _query = $"select * from UFD1 where TableID='@FRPAGO2' and FldValue='{rTransferencias.moneda}'";
                        if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'moneda' incorrecto.";
                        var moneda = rTransferencias.moneda;
                        var monto = Convert.ToDecimal(rTransferencias.credito) == 0 ? Convert.ToDouble(rTransferencias.debito) : Convert.ToDouble(rTransferencias.credito);

                        rowData = rows.Add();
                        rowData.SetProperty("U_Banco", banco);
                        rowData.SetProperty("U_Transfer", numeroDoc);
                        rowData.SetProperty("U_Fecha", fecha);
                        rowData.SetProperty("U_Moneda", moneda);
                        rowData.SetProperty("U_Monto", monto);

                    }
                    utilities.logger.Debug("Transferencia agregada");
                }



                if (depositos.Count > 0)
                {
                    comentario.Append("\n " + depositos.First().descripcion);
                }
                datosServicio.SetProperty("Remark", comentario.ToString());

                //depositos

                foreach (MediosDePago rdepositos in depositos)
                {

                    if(Convert.ToInt16(rdepositos.moneda) == 1)
                    {
                        rdepositos.moneda = "GTQ";
                    }
                    else {
                        rdepositos.moneda = "USD";
                    }


                    if (Convert.ToString(rdepositos.tipoDeposito) == "")
                    {
                        rdepositos.tipoDeposito = "X";
                    }

                    utilities.logger.Debug("Agregando medio de pago como deposito.");
                    pago.TransferAccount = GetCodigoCuenta(rdepositos.codigoCuentaMayor);
                    if (pago.TransferAccount == "0") return (
                        $"(00026)|No se ha podido encontrar la cuenta contable {rdepositos.codigoCuentaMayor} en la base de datos de SAP."
                    );
                    pago.TransferSum += Convert.ToDecimal(rdepositos.credito) == 0 ? Convert.ToDouble(rdepositos.debito) : Convert.ToDouble(rdepositos.credito);
                    if (!((string.IsNullOrEmpty(rdepositos.nombreBanco) && string.IsNullOrEmpty(rdepositos.numeroDocumento)) || string.IsNullOrEmpty(rdepositos.numeroDocumento)))
                    {
                        hayDetalle = true;
                        if (depositos.Count == 1 && transferencias.Count == 0)
                        {
                            MediosDePago deposito = depositos.First();
                            pago.TransferDate = Convert.ToDateTime(deposito.fecha);
                        }

                        rows = datosServicio.Child("FRPAGO1");

                        {
                            _query = $"SELECT * FROM ODSC WHERE BankCode='{rdepositos.nombreBanco}'";
                            if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'nombreBanco' incorrecto.";

                            var banco = rdepositos.nombreBanco;
                            var numeroDoc = rdepositos.numeroDocumento;
                            var fecha = rdepositos.fecha;

                            _query = $"select * from UFD1 where TableID='@FRPAGO1' and FldValue='{rdepositos.moneda}'";
                            if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'moneda' incorrecto.";

                            var moneda = rdepositos.moneda;
                            var monto = Convert.ToDecimal(rdepositos.credito) == 0 ? Convert.ToDouble(rdepositos.debito) : Convert.ToDouble(rdepositos.credito);
                            _query = $"select * from UFD1 where TableID='@FRPAGO1' and FldValue='{rdepositos.tipoDeposito}'";
                            if (!ValidarCodigo(_query)) return "(00024)|Valor del parámetro 'tipoDeposito' incorrecto.";

                            var tipo = rdepositos.tipoDeposito;
                            rowData = rows.Add();
                            rowData.SetProperty("U_Banco", banco);
                            rowData.SetProperty("U_Boleta", numeroDoc);
                            rowData.SetProperty("U_Fecha", fecha);
                            rowData.SetProperty("U_Moneda", moneda);
                            rowData.SetProperty("U_Monto", monto);
                            rowData.SetProperty("U_Tipo", tipo);
                        }

                    }
                    utilities.logger.Debug("Deposito agregado.");
                }
                if (hayDetalle)
                {
                    GeneralDataParams response = servicioGeneral.Add(datosServicio);
                    string Code = response.GetProperty("DocEntry").ToString();
                    empresa.GetNewObjectCode(out _newObject);
                    pago.UserFields.Fields.Item("U_DetallePago").Value = Code;
                }
                utilities.logger.Debug("Transferencias y depositos agregados.");
            }
            utilities.logger.Debug("Agregando el pago a SAP.");
            pago.SaveXML(@"E:\Desktop\pruebas\pagoAntesDeEnviarASAP.xml");
            int res = pago.Add();
            if (res != 0)
            {
                empresa.GetLastError(out res, out _error);
                utilities.Error = _error + "|24";
                return _error;
            }
            utilities.logger.Debug("Pago agregado a SAP correctamente.");
            empresa.GetNewObjectCode(out _newObject);
            pago.GetByKey(Convert.ToInt32(_newObject));
            int docNum = pago.DocNum;
            _transaccion = new Transacciones("success", docNum + "|Documento Creado: Pago recibido|24");
            utilities.mensajeSuccess.transacciones.Add(_transaccion);
            return "1";
        }
        #endregion
        #region Acciones Ajustes Varios
        /// <summary>
        /// Ejecutas las Acciones de la funcionalidad de Ajustes Varios.
        /// </summary>
        /// <param name="partida"></param>
        /// <returns>String con valor 1 si no hay errores durante el proceso.</returns>
        public string EjecutarAjuste(partida partida)
        {
            string salida = "1";
            if (partida == null) return "(00001)|Estructura del archivo Json incorrecta.";
            empresa.StartTransaction();

            salida = CrearAsiento(partida);
            if (salida != "1")
            {
                empresa.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);
                return salida;
            }

            try
            {
                empresa.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                return "(00025)|" + e.Message;
            }

            return salida;
        }
        /// <summary>
        /// Crea un Asiento Contable en SAP.
        /// </summary>
        /// <param name="partida"></param>
        /// <returns></returns>
        private string CrearAsiento(partida partida)
        {
            SAPbobsCOM.JournalEntries asiento = (SAPbobsCOM.JournalEntries)empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
            if (!IsDate(partida.fechaContabilizacion)) return "(00022)|Fecha de contabilizacion incorrecta.";
            if (!IsDate(partida.fechaVencimiento)) return "(00022)|Fecha de contabilizacion incorrecta.";
            asiento.ReferenceDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            asiento.DueDate = DateTime.ParseExact(partida.fechaVencimiento, "yyyy-MM-dd", null);
            asiento.TaxDate = DateTime.ParseExact(partida.fechaContabilizacion, "yyyy-MM-dd", null);
            List<Rubros> creditos = new List<Rubros>();
            List<Rubros> debitos = new List<Rubros>();

            // Cambio para agregar el número de boleta del banco al campo de la referencia 2 en el asiento contable - Alex García - 04/08/2021
            // Obtiene el número de la boleta
            foreach (MediosDePago objMediosPago in partida.detallePartida.mediosDePago)
            {
                string strBoleta = objMediosPago.numeroDocumento.Trim().Replace("\0", "").Substring(0, 8);
                asiento.Reference2 = strBoleta;
            }

            //separacion de rubros con monto en creditos y debitos
            foreach (Rubros r in partida.detallePartida.rubros)
            {
                if (Convert.ToDouble(r.credito) > 0) creditos.Add(r);
                else debitos.Add(r);
            }
            string primeraCuentaDebito = ObtenerCuenta(debitos.First().codigo, true);
            string primeraCuentaCredito = ObtenerCuenta(creditos.First().codigo, true);
            if (primeraCuentaDebito == "0")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00018).");
                return "(00018)|(débito) Cuenta de rubro invalida";
            }
            if (primeraCuentaCredito == "0")
            {
                utilities.logger.Error(
                    $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00018).");
                return "(00018)|(crédito) Cuenta de rubro invalida";
            }
            foreach (Rubros rubroActual in creditos)
            {
                string cuenta = ObtenerCuenta(rubroActual.codigo, false);
                if (cuenta == "0")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00018).");
                    return "(00018)|(crédito) Cuenta de rubro invalida";
                }
                bool esCuentaCliente = ValidarCodigoCliente(rubroActual.codigo, false) == "0" ? false : true;
                asiento.Lines.ShortName = cuenta;
                if (esCuentaCliente) asiento.Lines.ShortName = rubroActual.codigo;
                asiento.Lines.AccountCode = cuenta;
                asiento.Lines.ContraAccount = primeraCuentaDebito;
                asiento.Lines.Credit = Convert.ToDouble(rubroActual.credito);
                asiento.Lines.Debit = 0;
                asiento.Lines.Add();
            }
            foreach (Rubros rubroActual in debitos)
            {
                string cuenta = ObtenerCuenta(rubroActual.codigo, false);
                if (cuenta == "0")
                {
                    utilities.logger.Error(
                        $"Error en metodo: {System.Reflection.MethodBase.GetCurrentMethod().Name} código de error (00018).");
                    return "(00018)|(débito) Cuenta de rubro invalida";
                }
                bool esCuentaCliente = ValidarCodigoCliente(rubroActual.codigo, false) == "0" ? false : true;
                asiento.Lines.ShortName = cuenta;
                if (esCuentaCliente) asiento.Lines.ShortName = rubroActual.codigo;
                asiento.Lines.AccountCode = cuenta;
                asiento.Lines.ContraAccount = primeraCuentaCredito;
                asiento.Lines.Credit = 0;
                asiento.Lines.Debit = Convert.ToDouble(rubroActual.debito);
                asiento.Lines.Add();
            }
            int res = asiento.Add();
            if (res != 0)
            {
                empresa.GetLastError(out res, out _error);
                utilities.Error = _error + "|30";
                return _error;
            }
            empresa.GetNewObjectCode(out _newObject);
            asiento.GetByKey(Convert.ToInt32(_newObject));
            int docNum = asiento.Number;
            _transaccion = new Transacciones("success", docNum + "|Documento Creado: Asiento|30");
            utilities.mensajeSuccess.transacciones.Add(_transaccion);

            return "1";
        }
        #endregion
        #region Otros metodos
        /// <summary>
        /// Evalua una cuenta mayor para saber si es una cuenta contable o un codigo de cliente.
        /// </summary>
        /// <param name="cadena"></param>
        /// <param name="esPrimera"></param>
        /// <returns>String con valor 0 si el codigo de cuenta mayor no existe. </returns>
        public string ObtenerCuenta(string cadena, bool esPrimera)
        {
            string codigoCuenta = "0";
            if (GetCodigoCuenta(cadena) == "0")
            {
                if (ValidarCodigoCliente(cadena, esPrimera) != "0") codigoCuenta = ValidarCodigoCliente(cadena, esPrimera);
                return codigoCuenta;
            }
            else
            {
                codigoCuenta = GetCodigoCuenta(cadena);
            }
            return codigoCuenta;
        }
        /// <summary>
        /// Obtiene el codigo de cuenta interno de sap para una cuenta contable.
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns>String con valor 0 si el codigo de cuenta mayor no existe.</returns>
        public string GetCodigoCuenta(string codigo)
        {
            string cuenta = "0";
            string comando = "";
            SAPbobsCOM.Recordset rs2 = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            comando = GetQueryCuentas(codigo);
            rs2.DoQuery(comando);
            cuenta = rs2.RecordCount > 0 ? rs2.Fields.Item("AcctCode").Value.ToString() : "0";

            return cuenta;
        }
        /// <summary>
        /// Valida el codigo de una cuenta mayor para un asiento contable en SAP.
        /// </summary>
        /// <param name="codigo"></param>
        /// <param name="esPrimera"></param>
        /// <returns>String con valor 0 si el codigo de cliente no existe.</returns>
        public string ValidarCodigoCliente(string codigo, bool esPrimera)
        {
            string retorno;
            SAPbobsCOM.Recordset rs2 = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            string comando = string.Format("SELECT DebPayAcct FROM OCRD WHERE CardCode='{0}'", codigo);
            rs2.DoQuery(comando);
            retorno = rs2.RecordCount > 0 ? rs2.Fields.Item("DebPayAcct").Value.ToString() : "0";
            if (esPrimera && retorno != "0") retorno = codigo;
            return retorno;
        }
        /// <summary>
        /// valida que el formato de fecha sea del tipo AAAA-MM-DD
        /// </summary>
        /// <param name="inputDate"></param>
        /// <returns></returns>
        public bool IsDate(string inputDate)
        {
            bool isDate = true;
            try
            {
                DateTime dateValue;
                dateValue = DateTime.ParseExact(inputDate, "yyyy-MM-dd", null);
            }
            catch
            {
                isDate = false;
            }
            return isDate;
        }
        /// <summary>
        /// valida si el codigo de cuenta existe en SAP
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public bool ValidarCodigo(string query)
        {
            SAPbobsCOM.Recordset rs2 = ((SAPbobsCOM.Recordset)(empresa.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)));
            rs2.DoQuery(query);
            if (rs2.RecordCount <= 0) return false;
            return true;
        }
        /// <summary>
        /// obtiene el query para consultar si la cuenta existe en base de datos segun el formato de la cuenta enviada
        /// </summary>
        /// <param name="cuenta"> ej: 123456789 ó 12302315-00</param>
        /// <returns></returns>
        public string GetQueryCuentas(string cuenta)
        {
            string comando = "";
            if (cuenta.Contains("-"))
            {
                var split = cuenta.Split('-');
                comando = $"SELECT AcctCode FROM OACT WHERE Segment_0='{split[0]}' AND Segment_1='{split[1]}'";
            }
            else
            {
                comando = $"SELECT AcctCode FROM OACT WHERE Segment_0='{cuenta}'";
            }
            return comando;
        }
        #endregion
    }
}