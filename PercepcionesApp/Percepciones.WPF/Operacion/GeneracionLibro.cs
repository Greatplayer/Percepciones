using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Percepciones.WPF.Entidades;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace Percepciones.WPF.Operacion
{
    public class GeneracionLibro
    {
        //private Microsoft.Office.Interop.Excel.Application app = null;
        //private Microsoft.Office.Interop.Excel.Workbook workbook = null;
        //private Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
        //private Microsoft.Office.Interop.Excel.Range workSheet_range = null;
        
        //DateTime starttime;
        //DateTime stoptime;

        StringBuilder listaHMayorD = new StringBuilder();
        StringBuilder listaNoIncluidosF = new StringBuilder();
        StringBuilder listaNoIncluidosSN = new StringBuilder();
        StringBuilder listaNoDetalleMesanterior = new StringBuilder();

        string[,] matriz = new string[,] {{string.Empty,"Comprobante de Pago",string.Empty,string.Empty, "Importe de la Transaccion", string.Empty,string.Empty},
                                               {"Fecha Tran.","Tipo", "Numero", "Tipo Transaccion","Debe","Haber","Saldo"}};

        PercepcionesEntities entidad = null;

        public GeneracionLibro(PercepcionesEntities entidad) 
        {
            this.entidad = entidad;
        }

        public List<string> ListarEjercicios() 
        {
            try
            {
                List<string> listaEjercicio = entidad.SP_LIB_LISTAR_EJERCICIO().ToList();
                return listaEjercicio;
            }
            catch (Exception)
            {   
                throw;
            }
        }

        public List<string> ListarPeriodo(string ejercicio) 
        {
            List<string> listaPeriodo = entidad.SP_LIB_LISTAR_PERIODO(ejercicio).ToList();
            return listaPeriodo;
        }

        public bool ProcesarArchivoLibro(ref string mensaje, string carpetaSeleccionada, string periodo)
        {
            bool flag = true;
            //Ingresar al servidor los archivos e ingresar a la tabla temporales los datos de ventas y cobros
            flag = CopiarArchivoServidor(ref mensaje, carpetaSeleccionada, periodo);
            
            if (flag)
            {
                LogApp.LogInfo("Metodo : ProcesarArchivoLibro", "Se copio archivos al servidor.");
                //Siguiente paso en el proceso del archivo - Clientes
                flag = CargarMaestroVentaCobroPeriodo(ref mensaje,periodo);

                if (flag)
                {
                    LogApp.LogInfo("Metodo : ProcesarArchivoLibro", "Se cargo los maestros de venta y cobros periodo:" + periodo);
                    flag = GenerarCliente(ref mensaje);

                    if (flag) 
                    {
                        LogApp.LogInfo("Metodo : ProcesarArchivoLibro", "Se genero los clientes:" + periodo);
                        flag = GenerarSaldoPeriodo(ref mensaje, periodo);
                    }//Mensaje de error retorno Generar Saldo Periodo
                }//Mensaje de error retorno de Carga Maestro Cobro
            }//Mensaje de error retorno Copia Archivo Servidor
            return flag;
        }

        private bool CopiarArchivoServidor(ref string mensaje, string carpetaSeleccionada, string periodo) 
        {
            try
            {

                string fileVenta = carpetaSeleccionada + ConfigurationManager.AppSettings.Get("nombreArchivoVenta") + periodo + ConfigurationManager.AppSettings.Get("extensionArchivo");
                string fileCobro = carpetaSeleccionada + ConfigurationManager.AppSettings.Get("nombreArchivoCobro") + periodo + ConfigurationManager.AppSettings.Get("extensionArchivo");

                string destinoVenta = ConfigurationManager.AppSettings.Get("carpetaInput") + ConfigurationManager.AppSettings.Get("nombreArchivoVenta") + ConfigurationManager.AppSettings.Get("extensionArchivo");
                string destinoCobro = ConfigurationManager.AppSettings.Get("carpetaInput") + ConfigurationManager.AppSettings.Get("nombreArchivoCobro") + ConfigurationManager.AppSettings.Get("extensionArchivo");

                File.Copy(@fileVenta, @destinoVenta, true);
                File.Copy(@fileCobro, @destinoCobro, true);

                var resultado = entidad.SP_LB_CARGAR_VENTAS_COBROS(destinoVenta, destinoCobro);

                return true;
            }
            catch (FileNotFoundException e)
            {
                mensaje = e.Message;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                mensaje = "No se poseen los permisos para copiar los archivos a la carpeta para la carga.";
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                mensaje = "La carpeta destinada para el input no existe.";
                return false;
            }
            catch (Exception ex)
            {
                mensaje = "Se origino un error en la aplicación - reiniciarla.";
                LogApp.LogError("Ingreso Data Base de datos", ex.InnerException.Message);
                return false;
            }
        }

        private bool CargarMaestroVentaCobroPeriodo(ref string mensaje, string periodo)
        {
            try
            {
                //Elimino la data del periodo
                entidad.SP_LB_ELIMINAR_DATA_PERIODO(periodo);

                //Ingreso todos los clientes nuevos que aparezcan en el archivo
                var resultado = entidad.SP_LB_CARGAR_MAESTRO_VENTA_COBRO(periodo);

                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Se origino un error en la carga de registros. Verificar los campos.";
                LogApp.LogError("Ingreso Data Base de datos - MaestroVentaCobroPeriodo", ex.InnerException.Message);
                return false;
            }
        }

        private bool GenerarSaldoPeriodo(ref string mensaje, string periodo) 
        {
            try 
            {
                LogApp.LogInfo("Metodo : ProcesarArchivoLibro", "Inicio de Saldo por periodo:" + periodo);
                entidad.SP_LB_GENERAR_SALDO_X_PERIODO(periodo, CalculoPeriodoAnterior(periodo));
                LogApp.LogInfo("Metodo : ProcesarArchivoLibro", "Fin de saldo por periodo:" + periodo);
                return true;
            }
            catch (Exception ex) 
            {
                mensaje = "Se origino un error en la generación del saldo.";
                LogApp.LogError("Ingreso Data Base de datos - Generar Saldo Periodo", ex.InnerException.Message);
                return false;
            }
        }

        private bool GenerarCliente(ref string mensaje) 
        {
            try
            {
                //Ingreso todos los clientes nuevos que aparezcan en el archivo
                int resultado = entidad.ExecuteStoreCommand("Exec SP_LB_GENERAR_CLIENTES");

                //Actualizo los campos
                resultado = entidad.ExecuteStoreCommand("Exec SP_LB_ACTUALIZAR_CLIENTES");

                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Se origino un error en la generación de los clientes. Verifique el log.";
                LogApp.LogError("Ingreso Data Base de datos", ex.InnerException.Message);
                return false;
            }
            
        }

        public bool GenerarTableExcel(ref string mensaje, string periodo) 
        {
            try
            {
                entidad.SP_LIB_ELIMINAR_EXCEL();
                entidad.SP_LB_GENERAR_EXCEL_VENTAS(periodo);
                entidad.SP_LB_GENERAR_EXCEL_COBROS(periodo);
                return true;
            }
            catch (Exception e)
            {
                mensaje = "Error en la generación del libro - Verificar Log";
                LogApp.LogError("Error en el generacion del libro", e.InnerException.Message);
                return false;
            }
            
        }

        #region Metodos Excel

        public bool GenerarLibroPeriodo(ref string mensaje, string periodo, CreateExcelDoc excell_app)
        {
            try
            {

                var lista = (from p in entidad.LIB_MASTER_CLIENTES select p).Distinct().OrderBy(x => x.CODIGO_BAT).ToList();

                //Nro de Columnas que separa cada reporte
                int separadorCliente = 2;
                //Donde se almacena la ultima fila del cliente impreso
                int numeroFilaAnteriorCliente = 0;
                int numeroCliente = 0;
                //progressBar1.Minimum = 0;
                //progressBar1.Maximum = lista.Count;

                foreach (var item in lista)
                //for (int i = inicio; i <= final; i++)
                {
                    //var item = lista[i];
                    Debug.WriteLine("cliente: " + item.CODIGO_BAT + " Nombre : " + item.OUTLET_NAME + "- iteracion : " + numeroCliente);
                    int numeroFilasOcupadas = 2; //Empieza en 2 por la cabecera
                    int filaInicioCliente = numeroFilaAnteriorCliente + separadorCliente;
                    numeroCliente++;
                    //progressBar1.Value = numeroCliente;
                    //Busqueda de comprobantes del cliente
                    List<LIB_GEN_EXCEL> listaFacturas = (from p in entidad.LIB_GEN_EXCEL
                                                         where p.COD_BAT == item.CODIGO_BAT
                                                         select p)
                                                        .OrderByDescending(x => x.FLAG_DEBE_HABER)
                                                        .ThenBy(n => n.FECHA_TRANSACCION).ToList<LIB_GEN_EXCEL>();

                    LIB_DETALLE_PERIODO_CLIENTE detallePeriodoAnterior = (from p in entidad.LIB_DETALLE_PERIODO_CLIENTE
                                                                          where p.LIB_MASTER_CLIENTES.ID_CLIENTE == item.ID_CLIENTE
                                                                             && p.PERIODO == ""
                                                                          select p)
                                                                          .FirstOrDefault<LIB_DETALLE_PERIODO_CLIENTE>();

                    if (detallePeriodoAnterior == null)
                    {
                        detallePeriodoAnterior = new LIB_DETALLE_PERIODO_CLIENTE() { SALDO = 0 };
                        listaNoDetalleMesanterior.Append(item.CODIGO_BAT + "-");
                    }

                    if (listaFacturas == null)
                    {
                        listaNoIncluidosF.Append(item.CODIGO_BAT + ",");
                        continue;
                    }

                    if (listaFacturas.Count == 0)
                    {
                        listaNoIncluidosF.Append(item.CODIGO_BAT + ",");
                        continue;
                    }

                    excell_app.CrearCabeceraBloque(1, matriz, filaInicioCliente, 2);
                    numeroFilasOcupadas = numeroFilasOcupadas + 2;

                    //Generacion Nombre -  Saldo
                    excell_app.CrearCeldaNombreSaldo(1, GenerarNombreSaldo(item.OUTLET_NAME, detallePeriodoAnterior.SALDO.Value), filaInicioCliente + 3, 2);
                    numeroFilasOcupadas = numeroFilasOcupadas + 3;

                    //Generacion Estado - Cuenta
                    int filasEstadocuenta = 0;
                    string[,] estadoCuenta = GenerarEstadocuenta(listaFacturas, ref filasEstadocuenta, item.CODIGO_BAT, detallePeriodoAnterior.SALDO.Value);

                    excell_app.CrearCeldaEstadoCuenta(1, estadoCuenta, filaInicioCliente + 7, 2, filasEstadocuenta, 7);
                    numeroFilasOcupadas = numeroFilasOcupadas + filasEstadocuenta;

                    //Suma para el siguiente cliente
                    numeroFilaAnteriorCliente += numeroFilasOcupadas + separadorCliente;

                    //if (numeroCliente == 50)
                    //    break;

                }

                return true;
            }
            catch (Exception e)
            {
                mensaje = "Error en la generación del libro - Verificar Log";
                LogApp.LogError("Error en el generacion del libro", e.InnerException.Message);
                return false;
            }
        }

        public void GenerarLibroPeriodo(ref string mensaje, LIB_MASTER_CLIENTES item, CreateExcelDoc excell_app, ref int separadorCliente,
                                        ref int numeroFilaAnteriorCliente, ref int numeroCliente, string periodo)
        {
            Debug.WriteLine("cliente: " + item.CODIGO_BAT + " Nombre : " + item.OUTLET_NAME + "- iteracion : " + numeroCliente);
            int numeroFilasOcupadas = 2; //Empieza en 2 por la cabecera
            int filaInicioCliente = numeroFilaAnteriorCliente + separadorCliente;
            numeroCliente++;
            //progressBar1.Value = numeroCliente;
            //Busqueda de comprobantes del cliente
            //List<LIB_GEN_EXCEL> listaFacturas = (from p in entidad.LIB_GEN_EXCEL
            //                                     where p.COD_BAT == item.CODIGO_BAT
            //                                     select p)
            //                                    .OrderByDescending(x => x.FLAG_DEBE_HABER)
            //                                    .ThenBy(n => n.FECHA_TRANSACCION).ToList<LIB_GEN_EXCEL>();


            List<LIB_GEN_EXCEL> listaFacturas = entidad.SP_LIB_LISTAR_DOCUMENTOS_GEN_EXCEL_X_CLIENTE(item.CODIGO_BAT).ToList<LIB_GEN_EXCEL>();

            //LIB_DETALLE_PERIODO_CLIENTE detallePeriodoAnterior = (from p in entidad.LIB_DETALLE_PERIODO_CLIENTE
            //                                                      where p.LIB_MASTER_CLIENTES.ID_CLIENTE == item.ID_CLIENTE
            //                                                         && p.PERIODO == periodo
            //                                                      select p)
            //                                                      .FirstOrDefault<LIB_DETALLE_PERIODO_CLIENTE>();

            LIB_DETALLE_PERIODO_CLIENTE detallePeriodoAnterior = entidad.SP_UBICAR_DETALLE_PERIODO_X_BAT_X_PERIODO(item.CODIGO_BAT, periodo).
                                                                 FirstOrDefault<LIB_DETALLE_PERIODO_CLIENTE>();

            if (detallePeriodoAnterior == null)
            {
                detallePeriodoAnterior = new LIB_DETALLE_PERIODO_CLIENTE() { SALDO = 0 };
                listaNoDetalleMesanterior.Append(item.CODIGO_BAT + "-");
            }

            if (listaFacturas == null)
            {
                listaNoIncluidosF.Append(item.CODIGO_BAT + ",");
                return;
            }

            if (listaFacturas.Count == 0)
            {
                listaNoIncluidosF.Append(item.CODIGO_BAT + ",");
                return;
            }

            excell_app.CrearCabeceraBloque(1, matriz, filaInicioCliente, 2);
            numeroFilasOcupadas = numeroFilasOcupadas + 2;

            //Generacion Nombre -  Saldo
            excell_app.CrearCeldaNombreSaldo(1, GenerarNombreSaldo(item.OUTLET_NAME, detallePeriodoAnterior.SALDO.Value), filaInicioCliente + 3, 2);
            numeroFilasOcupadas = numeroFilasOcupadas + 3;

            //Generacion Estado - Cuenta
            int filasEstadocuenta = 0;
            string[,] estadoCuenta = GenerarEstadocuenta(listaFacturas, ref filasEstadocuenta, item.CODIGO_BAT, detallePeriodoAnterior.SALDO.Value);

            excell_app.CrearCeldaEstadoCuenta(1, estadoCuenta, filaInicioCliente + 7, 2, filasEstadocuenta, 7);
            numeroFilasOcupadas = numeroFilasOcupadas + filasEstadocuenta;

            //Suma para el siguiente cliente
            numeroFilaAnteriorCliente += numeroFilasOcupadas + separadorCliente;
        }

        private string[,] GenerarEstadocuenta(List<LIB_GEN_EXCEL> listaDoc, ref int nroFilasOcupadas, string codigoCliente, decimal saldo)
        {
            List<string> lista = new List<string>();
            StringBuilder texto = new StringBuilder();
            decimal totalDebe = 0, totalHaber = 0;
            foreach (LIB_GEN_EXCEL factura in listaDoc)
            {

                if (factura.FLAG_DEBE_HABER == 1)
                {
                    saldo += factura.MONTO_COMPROBANTE - factura.PERCEPCION;
                    texto = new StringBuilder();
                    texto.Append(factura.FECHA_TRANSACCION + "|");
                    texto.Append(factura.TIPO_TRANSACCION + "|");//Corregir
                    texto.Append(factura.NRO_COMPROBANTE + "|");
                    texto.Append("VENTA" + "|");
                    texto.Append((factura.MONTO_COMPROBANTE - factura.PERCEPCION).ToString() + "|");
                    texto.Append(string.Empty + "|");
                    texto.Append(saldo.ToString());
                    lista.Add(texto.ToString());

                    saldo += factura.PERCEPCION;
                    texto = new StringBuilder();
                    texto.Append(factura.FECHA_TRANSACCION + "|");
                    texto.Append(factura.TIPO_TRANSACCION + "|");//Corregir
                    texto.Append(factura.NRO_COMPROBANTE + "|");
                    texto.Append("PERCEPCION POR COBRAR" + "|");
                    texto.Append(factura.PERCEPCION.ToString() + "|");
                    texto.Append(string.Empty + "|");
                    texto.Append(saldo.ToString());
                    lista.Add(texto.ToString());

                    totalDebe += factura.MONTO_COMPROBANTE;
                }
                else
                {
                    if (factura.MONTO_COMPROBANTE < 0)
                    {
                        saldo += (factura.MONTO_COMPROBANTE - factura.PERCEPCION);
                        texto = new StringBuilder();
                        texto.Append(factura.FECHA_TRANSACCION + "|");
                        texto.Append(factura.TIPO_TRANSACCION + "|");//Corregir
                        texto.Append(factura.NRO_COMPROBANTE + "|");
                        texto.Append("DESCUENTO" + "|");
                        texto.Append(string.Empty + "|");
                        texto.Append(decimal.Negate((factura.MONTO_COMPROBANTE - factura.PERCEPCION)).ToString() + "|");
                        texto.Append(saldo.ToString());
                        lista.Add(texto.ToString());

                        saldo += (factura.PERCEPCION);
                        texto = new StringBuilder();
                        texto.Append(factura.FECHA_TRANSACCION + "|");
                        texto.Append(factura.TIPO_TRANSACCION + "|");//Corregir
                        texto.Append(factura.NRO_COMPROBANTE + "|");
                        texto.Append("PERCEPCION POR COBRAR" + "|");
                        texto.Append(string.Empty + "|");
                        texto.Append(decimal.Negate(factura.PERCEPCION).ToString() + "|");
                        texto.Append(saldo.ToString());
                        lista.Add(texto.ToString());
                        totalHaber -= factura.MONTO_COMPROBANTE;
                    }
                    else
                    {
                        saldo -= factura.MONTO_COMPROBANTE;
                        texto = new StringBuilder();
                        texto.Append(factura.FECHA_TRANSACCION + "|");
                        texto.Append(factura.TIPO_TRANSACCION + "|");//Corregir
                        texto.Append(factura.NRO_COMPROBANTE + "|");
                        texto.Append("LIQ. CAJA" + "|");
                        texto.Append(string.Empty + "|");
                        texto.Append(factura.MONTO_COMPROBANTE.ToString() + "|");
                        texto.Append(saldo.ToString());
                        lista.Add(texto.ToString());
                        totalHaber += factura.MONTO_COMPROBANTE;
                    }
                }
            }

            string[,] matrizRetorno = new string[lista.Count + 1, 7];
            int fila = 0, columna = 0;
            foreach (var item in lista)
            {
                string[] elementos = item.Split('|');

                matrizRetorno[fila, columna] = elementos[0];
                matrizRetorno[fila, columna + 1] = elementos[1];
                matrizRetorno[fila, columna + 2] = elementos[2];
                matrizRetorno[fila, columna + 3] = elementos[3];
                matrizRetorno[fila, columna + 4] = elementos[4];
                matrizRetorno[fila, columna + 5] = elementos[5];
                matrizRetorno[fila, columna + 6] = elementos[6];
                fila++;
                nroFilasOcupadas++;
            }

            matrizRetorno[lista.Count, 0] = "Total";
            matrizRetorno[lista.Count, 1] = codigoCliente;
            matrizRetorno[lista.Count, 2] = string.Empty;
            matrizRetorno[lista.Count, 3] = string.Empty;
            matrizRetorno[lista.Count, 4] = totalDebe.ToString();
            matrizRetorno[lista.Count, 5] = totalHaber.ToString();
            matrizRetorno[lista.Count, 6] = saldo.ToString();
            nroFilasOcupadas++;

            return matrizRetorno;
        }

        private string[,] GenerarCabecera(string nombre, decimal saldo)
        {

            string[,] matriz = new string[,] {{string.Empty,"Comprobante de Pago",string.Empty,string.Empty, "Importe de la Transaccion", string.Empty,string.Empty},
                                               {"Fecha Tran.","Tipo", "Numero", "Tipo Transaccion","Debe","Haber","Saldo"}};

            return matriz;
        }

        private string[,] GenerarNombreSaldo(string nombre, decimal saldo)
        {
            string[,] matriz = new string[,] { { string.Empty, string.Empty, string.Empty, "Saldo Mes Anterior:", string.Empty, string.Empty, saldo.ToString() },
                                                { "Cliente", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty },
                                                { nombre, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty } };

            return matriz;
        }

        #endregion

        public List<LIB_MASTER_CLIENTES> ListarClientesOrdenBAT() 
        {
            return entidad.SP_LIB_LISTAR_CLIENTE().ToList();
        }

        private string CalculoPeriodoAnterior(string periodo) 
        {
            int anyo = int.Parse(periodo.Substring(0, 4));
            int mes = int.Parse(periodo.Substring(4, 2));

            if (mes == 01)
            {
                anyo--;
                return anyo.ToString() + "12";
            }
            else 
            {
                mes--;
                return anyo.ToString() + (mes < 10 ? "0" + mes : mes.ToString());
            }
        }

    }
}
