using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Percepciones.WPF.EntidadesPrueba;
using System.Configuration;
using Percepciones.WPF.Entidades;
using System.Data.SqlClient;
using System.Data;
using Percepciones.WPF.Operacion;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;

namespace Percepciones.WPF
{
    /// <summary>
    /// Lógica de interacción para Interfaz_v1.xaml
    /// </summary>
    public partial class Interfaz_v1 : Window
    {
        private PercepcionesEntities entidad = new PercepcionesEntities();
        private GeneracionLibro gl = null;
        private decimal odysseyMontoCalculado;
        private decimal odysseyPerceCalculado;
        private DateTime dt = new DateTime(1,1,1,0,0,0);

        public Interfaz_v1()
        {

            try {
                entidad.CommandTimeout = 7200;
                LogApp.IniciarArchivo();
                InitializeComponent();
                LogApp.LogInfo("Inicio aplicacion", "Fecha Inicio: " + DateTime.Now.ToShortDateString() + " - Hora de Inicio:" + DateTime.Now.ToShortTimeString());
                //Inicia los parametros del tab de generación de libro
                gl = new GeneracionLibro(entidad);
                //IniciarTabLibro();
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        #region Generar Archivo Percepcion

        private void btnCargarArchivo_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.FileName = "txt_archivo_mes";
            //dlg.DefaultExt = ".txt";
            //dlg.Filter = "Text documents (.txt)|*.txt";
            //Nullable<bool> result = dlg.ShowDialog();
            //if (result == true)
            //{
            //    string filename = dlg.FileName;
            //    txtDireccion.Text = filename;
            //    txtVerificacionInfo.Text = string.Empty;
            //    HabilitarBotonVerificarInformacion();
            //}

            string ubicacion = string.Empty;
            if (UbicarArchivo(ref ubicacion))
            {
                txtDireccion.Text = ubicacion;
                txtVerificacionInfo.Text = string.Empty;
                HabilitarBotonVerificarInformacion();
            }
        }

        private void btnCargarNC_Click(object sender, RoutedEventArgs e)
        {
            string ubicacion = string.Empty;
            if (UbicarArchivo(ref ubicacion))
            {
                txtDireccionNC.Text = ubicacion;
                txtVerificacionInfo.Text = string.Empty;
                HabilitarBotonVerificarInformacion();
            }
        }

        private void btnVerificarInfo_Click(object sender, RoutedEventArgs e)
        {
            pgBar.Visibility = System.Windows.Visibility.Visible;
            txtVerificacionInfo.Text = string.Empty;
            string mensaje = string.Empty;
            bool errorProceso = VerificarErroresArchivo(ref mensaje);

            if (errorProceso)
            {
                txbMensaje.Text = mensaje;
            }
            else
            {
                errorProceso = DataBDIngresar(ref mensaje);
                GenerarGrillaResumenPrevio();
                if (errorProceso)
                {
                    btnGenerar.IsEnabled = true;
                    txbMensaje.Text = mensaje;
                }
                else
                {
                    txbMensaje.Text = mensaje;
                }
            }
            pgBar.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            pgBar.Visibility = System.Windows.Visibility.Visible;
            string mensajeFinal = string.Empty;
            bool flagOperacion = DataBDProcesar(ref mensajeFinal);

            if (flagOperacion)
            {
                flagOperacion = DataGenerarArchivos(ref mensajeFinal);

                if (flagOperacion)
                {
                    tabItem2.Focus();
                    txbMensaje.Text = "Operacion Completada - Puede verificar los archivos";
                }
                else
                {
                    txbMensaje.Text = mensajeFinal;
                }
            }
            else
            {
                txbMensaje.Text = mensajeFinal;
            }
            pgBar.Visibility = System.Windows.Visibility.Hidden;
        }

        #region ProcesarArchivo

        private bool DataBDIngresar(ref string mensaje)
        {
            try
            {
                bool flagRetorno = false;
                //Copio el archivo
                string destinoDoc = ConfigurationManager.AppSettings.Get("carpetaInput") + ConfigurationManager.AppSettings.Get("nombreArchivoEntradaDoc");
                string destinoNC = ConfigurationManager.AppSettings.Get("carpetaInput") + ConfigurationManager.AppSettings.Get("nombreArchivoEntradaNC");

                File.Copy(@txtDireccion.Text, @destinoDoc, true);
                File.Copy(@txtDireccionNC.Text, @destinoNC, true);

                var param1 = new SqlParameter("PathDocumentos", SqlDbType.VarChar);
                param1.Value = destinoDoc;
                var param2 = new SqlParameter("PathNotaCred", SqlDbType.VarChar);
                param2.Value = destinoNC;

                //Ejecuto el Stored Procedure que llenara las tablas
                int resultado = entidad.ExecuteStoreCommand("Exec SP_CARGAR_DOCUMENTOS_PERCEP @PathDocumentos, @PathNotaCred", param1, param2);

                return !flagRetorno;
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

        private bool DataBDProcesar(ref string mensaje)
        {
            try
            {
                btnGenerar.IsEnabled = false;
                List<DOC_NC_AG> listaCreditos = new List<DOC_NC_AG>();
                List<DOC_NC_AG> listaNoAplicados = new List<DOC_NC_AG>();
                listaCreditos = entidad.DOC_NC_AG.ToList();
                //var totalPercepcionResta = entidad.DOC_NC_AG.Sum(x => x.PERCEPCION);

                decimal totalDocRestado = 0;
                decimal totalDocNoRestado = 0;

                decimal totalPercepRestado = 0;
                decimal totalPercepNoRestado = 0;

                pgBar.Minimum = 0;
                pgBar.Maximum = listaCreditos.Count();
                pgBar.Value = 0;

                foreach (DOC_NC_AG credito in listaCreditos)
                {
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pgBar.SetValue);
                    Dispatcher.Invoke(updatePbDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, pgBar.Value + 1 });

                    //Busco que las facturas del cliente en el mes sean superior a las Notas de creditos del mes
                    var monto = entidad.DOCUMENTO.Where(x => x.outlet == credito.CODIGO_CLIENTE).Sum(x => x.total_amount);
                    if (decimal.Negate(credito.MONTO.Value) > monto || monto == null) //En caso no lo sean
                    {
                        //Se almacena el valor para el posterior reporte
                        listaNoAplicados.Add(credito);

                        //Se guarda un historial de la resta
                        GuardarResumenNC(new RESUMEN_RESTA() { CodCliente = credito.CODIGO_CLIENTE, NotaCredito = credito.MONTO.Value, SumaBoletas = monto == null ? 0 : monto.Value, Restado = "N", Percepcion = credito.PERCEPCION.Value });
                        totalDocNoRestado += credito.MONTO.Value;
                        totalPercepNoRestado += credito.PERCEPCION.Value;

                    }
                    else //Lo son
                    {
                        //Se guarda un historial de la resta
                        GuardarResumenNC(new RESUMEN_RESTA() { CodCliente = credito.CODIGO_CLIENTE, NotaCredito = credito.MONTO.Value, SumaBoletas = monto.Value, Restado = "S", Percepcion = credito.PERCEPCION.Value });

                        //Se asignan los valores
                        decimal montoRestar = credito.MONTO.Value;
                        decimal igvRestar = 0;// credito.IGV.Value;
                        decimal percepRestar = credito.PERCEPCION.Value;

                        //Se buscan los documentos a restar 
                        List<DOCUMENTO> documentosRestar = entidad.DOCUMENTO.Where(x => x.outlet == credito.CODIGO_CLIENTE).ToList();
                        //Se suma el restado total de la operacion
                        totalDocRestado += credito.MONTO.Value;
                        totalPercepRestado += credito.PERCEPCION.Value;

                        foreach (var dr in documentosRestar)
                        {
                            decimal montoRestado = montoRestar + dr.total_amount.Value;
                            decimal igvRestado = igvRestar + dr.igv.Value;
                            decimal percRestado = percepRestar + dr.percepcion.Value;

                            if (montoRestado < 0)
                            {
                                dr.total_amount = 0; dr.igv = 0; dr.percepcion = 0;
                                entidad.SaveChanges();
                                montoRestar = montoRestado; igvRestar = igvRestado; percepRestar = percRestado;
                            }
                            else
                            {
                                dr.total_amount = montoRestado;
                                dr.igv = igvRestado;
                                dr.percepcion = percRestado;
                                entidad.SaveChanges();
                                break;
                            }
                        }
                    }
                }
                var totalPercepResultado = entidad.DOCUMENTO.Sum(x => x.percepcion);
                var totalDocResultado = entidad.DOCUMENTO.Sum(x => x.total_amount);

                //label1.Text = "\r\nProceso terminado";

                List<DetalleOperacion> listaResumen = new List<DetalleOperacion>();
                listaResumen.Add(new DetalleOperacion() { Detalle = "Documentos", MontoDocumento = totalDocResultado.Value, MontoPercepcion = totalPercepResultado.Value });
                listaResumen.Add(new DetalleOperacion() { Detalle = "Notas de Crédito NAP", MontoDocumento = totalDocNoRestado, MontoPercepcion = totalPercepNoRestado });
                listaResumen.Add(new DetalleOperacion() { Detalle = "Casos Considerados", MontoDocumento = 0, MontoPercepcion = 0 });
                listaResumen.Add(new DetalleOperacion() { Detalle = "Total", MontoDocumento = (totalDocResultado.Value + totalDocNoRestado), MontoPercepcion = (totalPercepResultado.Value + totalPercepNoRestado) });
                listaResumen.Add(new DetalleOperacion() { Detalle = "Calculado por Odyssey", MontoDocumento = odysseyMontoCalculado, MontoPercepcion = odysseyPerceCalculado });
                listaResumen.Add(new DetalleOperacion() { Detalle = "Diferencia Odyssey - Total", MontoDocumento = (odysseyMontoCalculado - (totalDocResultado.Value + totalDocNoRestado)), MontoPercepcion = (odysseyPerceCalculado - (totalPercepResultado.Value + totalPercepNoRestado)) });

                GenerarGrillaResumen(listaResumen);
                GenerarGrillaNCNoAplicada(listaNoAplicados);
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Se origino un error. Verifique el log del sistema.";
                LogApp.LogError("Procesar Archivo", ex.InnerException.Message);
                return false;
            }
        }

        private bool DataGenerarArchivos(ref string mensaje)
        {
            bool flagError = GenerarFileDocumentos(ref mensaje);

            if (flagError)
            {
                flagError = GenerarFileNC_NoAplicada(ref mensaje);
                return flagError;
            }
            return flagError;
        }

        private void GenerarGrillaResumenPrevio()
        {
            var montoComprobantes = entidad.DOCUMENTO.Sum(x => x.total_amount);
            var perceComprobantes = entidad.DOCUMENTO.Sum(x => x.percepcion);
            var montoNC = entidad.DOC_NC_AG.Sum(x => x.MONTO);
            var perceNC = entidad.DOC_NC_AG.Sum(x => x.PERCEPCION);

            List<DetalleOperacion> listaResumen = new List<DetalleOperacion>();
            listaResumen.Add(new DetalleOperacion() { Detalle = "Documentos", MontoDocumento = montoComprobantes.Value, MontoPercepcion = perceComprobantes.Value });
            listaResumen.Add(new DetalleOperacion() { Detalle = "Notas de Crédito", MontoDocumento = montoNC.Value, MontoPercepcion = perceNC.Value });
            listaResumen.Add(new DetalleOperacion() { Detalle = "Total", MontoDocumento = (montoComprobantes.Value + montoNC.Value), MontoPercepcion = (perceComprobantes.Value + perceNC.Value) });

            dgResumenPrevio.ItemsSource = listaResumen;
        }

        private void GenerarGrillaResumen(List<DetalleOperacion> lista)
        {
            dgCalculoFinal.ItemsSource = lista;
        }

        private void GenerarGrillaNCNoAplicada(List<DOC_NC_AG> lista)
        {
            dgNotaCredito.ItemsSource = lista;
        }

        private void GuardarResumenNC(RESUMEN_RESTA item)
        {
            entidad.AddObject("RESUMEN_RESTA", item);
            entidad.SaveChanges();
        }

        private bool UbicarArchivo(ref string ubicacion)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "txt_archivo_mes";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            Nullable<bool> result = dlg.ShowDialog();
            ubicacion = dlg.FileName;
            return result.Value;
        }

        private bool VerificarErroresArchivo(ref string mensaje)
        {
            try
            {
                bool flagRetorno = false;
                Queue<string> lineasErroneas = new Queue<string>();

                if (!File.Exists(txtDireccion.Text))
                {
                    mensaje = "Archivo de Documentos no existe en la ruta especificada.";
                    return !flagRetorno;
                }
                else if (!File.Exists(txtDireccionNC.Text))
                {
                    mensaje = "Archivo de Nota de Creditos no existe en la ruta especificada.";
                    return !flagRetorno;
                }
                else if (string.IsNullOrWhiteSpace(txtOdysseyMonto.Text) || string.IsNullOrWhiteSpace(txtOdysseyPerce.Text))
                {
                    mensaje = "Debe ingresar los valores de Odyssey para el calculo.";
                    return !flagRetorno;
                }

                odysseyMontoCalculado = decimal.Parse(txtOdysseyMonto.Text);
                odysseyPerceCalculado = decimal.Parse(txtOdysseyPerce.Text);

                int nroLinea = 1;
                using (StreamReader sr = File.OpenText(txtDireccion.Text))
                {
                    String linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        //Verificar que la linea cumple con los resultados - 11 
                        int nroDatos = linea.Split(',').Count();
                        if (nroDatos != 11)
                            lineasErroneas.Enqueue("Linea observada - Archivo Documento : " + nroLinea);

                        nroLinea++;
                    }
                }

                nroLinea = 1;
                using (StreamReader sr = File.OpenText(txtDireccionNC.Text))
                {
                    String linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        //Verificar que la linea cumple con los resultados - 11
                        int nroDatos = linea.Split(',').Count();
                        if (nroDatos != 7)
                            lineasErroneas.Enqueue("Linea observada - Archivo Nota Credito : " + nroLinea);

                        nroLinea++;
                    }
                }

                mensaje = lineasErroneas.Count > 0 ? "Se hallaron lineas incorrectas" : "Archivos Correctos.";

                foreach (Object item in lineasErroneas)
                {
                    txtVerificacionInfo.Text = txtVerificacionInfo.Text + item.ToString() + "\r\n";
                }

                return lineasErroneas.Count > 0 ? !flagRetorno : flagRetorno;
            }
            catch (Exception)
            {
                mensaje = "Se origino un error en la aplicación - reiniciarla.";
                return false;
            }

        }

        private void HabilitarBotonVerificarInformacion()
        {
            btnVerificarInfo.IsEnabled = false;

            if (txtDireccion.Text != string.Empty && txtDireccionNC.Text != string.Empty)
                btnVerificarInfo.IsEnabled = true;
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void UpdateTextStatusDelegate(System.Windows.DependencyProperty dp, Object value);

        #region ArchivoPercepciones - Probado

        private bool GenerarFileDocumentos(ref string mensaje)
        {
            try
            {
                List<DOCUMENTO> listaDocumento = entidad.DOCUMENTO.OrderBy(x => x.document_number).ToList<DOCUMENTO>();
                string rutaOutput = ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("nombreArchivoDoc");

                using (StreamWriter file = new StreamWriter(@rutaOutput))
                {
                    //foreach (DocumentoPrueba doc in listaDocumento)
                    foreach (DOCUMENTO doc in listaDocumento)
                    {
                        if (doc.total_amount == 0)
                            continue;

                        StringBuilder cadenaEscribir = GenerarLineaDocumento(doc);
                        file.WriteLine(cadenaEscribir.ToString());
                    }
                }
                return true;
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
                mensaje = "Se origino un error en la aplicación. Verifique el log.";
                LogApp.LogError("Generar File Archivos", ex.InnerException.Message);
                return false;
            }
        }

        private StringBuilder GenerarLineaDocumento(DOCUMENTO documento)
        {
            string tipoDoc = documento.outlet.Substring(0, 1) == "R" ? "06" : "01";
            string razonSocial = string.Empty, apePaterno = string.Empty, apeMaterno = string.Empty, nombres = string.Empty, numeroDocumento = string.Empty;
            numeroDocumento = documento.outlet.Substring(2);

            //if (documento.id == 6631)
            //    apeMaterno = string.Empty;

            if (tipoDoc == "06")
            {
                //ruc con nombre comercial -- Inicia con 20
                if (documento.outlet.Substring(2, 2) == "20")
                    razonSocial = documento.razon_social;
                else
                {//ruc con nombre de persona natural -- Inicia con 10
                    string[] nombreSeparado = documento.razon_social.Split(' ').Where(x => x != "" || x != string.Empty).ToArray();
                    apePaterno = nombreSeparado[0];
                    apeMaterno = nombreSeparado.Count() > 2 ? nombreSeparado[1] : string.Empty;
                    nombres = nombreSeparado.Count() > 2 ? nombreSeparado[2] : nombreSeparado[1];
                }
            }
            else
            {
                //cliente con DNI
                string[] nombreSeparado = documento.razon_social.Split(' ').Where(x => x != "" || x != string.Empty).ToArray();
                apePaterno = nombreSeparado[0];
                apeMaterno = nombreSeparado.Count() > 2 ? nombreSeparado[1] : string.Empty;
                nombres = nombreSeparado.Count() > 2 ? nombreSeparado[2] : nombreSeparado[1];
            }

            string serieComprobante, numeroComprobante, fechaComprobante, derechoFiscal, percepcion, monto, tipoComprobante;


            string comprobante = documento.document_number.Split('-')[1];
            serieComprobante = comprobante.Substring(0, 3);
            numeroComprobante = comprobante.Substring(3, 7);

            fechaComprobante = documento.fecha;
            derechoFiscal = documento.document_number.Substring(0, 2) == "01" ? "1" : "0";

            decimal division = (documento.percepcion.Value / documento.total_amount.Value) * 100;
            if (division > 1)
                percepcion = "0";
            else
                percepcion = "1";

            monto = documento.total_amount.Value.ToString();

            tipoComprobante = documento.document_number.Substring(0, 2);

            StringBuilder lineaRegreso = new StringBuilder();
            lineaRegreso.Append(tipoDoc + "|");//Tipo de Doc Cliente
            lineaRegreso.Append(numeroDocumento + "|");//Documento Cliente
            lineaRegreso.Append(razonSocial + "|");//Razon Social - En caso Doc cliente es 6
            lineaRegreso.Append(apePaterno + "|");//Ap Paterno - En caso Doc cliente es 1
            lineaRegreso.Append(apeMaterno + "|");//Ap Materno - En caso Doc cliente es 1
            lineaRegreso.Append(nombres + "|");//Nombres - En caso Doc cliente es 1
            lineaRegreso.Append(serieComprobante + "|");//Serie del comprobante (factura)
            lineaRegreso.Append(numeroComprobante + "|");//Numero del comprobante (factura)
            lineaRegreso.Append(fechaComprobante + "|");//Fecha del comprobante (factura)
            lineaRegreso.Append(derechoFiscal + "|");//Derecho Fiscal 
            lineaRegreso.Append("0" + "|");//Material Construccion -  siempre 0 
            lineaRegreso.Append(percepcion + "|");//0.5 % percepcion
            lineaRegreso.Append(monto + "|");//monto
            lineaRegreso.Append(tipoComprobante + "|");//Tipo Comprobante (factura o boleta)

            return lineaRegreso;
        }

        #endregion

        #region Nota de Credito No Aplicada - Probado

        private bool GenerarFileNC_NoAplicada(ref string mensaje)
        {
            try
            {
                List<RESUMEN_RESTA> listaDocumento = entidad.RESUMEN_RESTA.Where(x => x.Restado == "N").OrderBy(x => x.CodCliente).ToList<RESUMEN_RESTA>();
                string rutaOutput = ConfigurationManager.AppSettings.Get("carpetaOutput") + ConfigurationManager.AppSettings.Get("nombreArchivoNC");

                using (StreamWriter file = new StreamWriter(@rutaOutput))
                {
                    foreach (RESUMEN_RESTA doc in listaDocumento)
                    {
                        StringBuilder cadenaEscribir = GenerarLineaNC(doc);
                        file.WriteLine(cadenaEscribir.ToString());
                    }
                }
                return true;
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
                mensaje = "Se origino un error en la aplicación. Verifique el log.";
                LogApp.LogError("Generar File Nota Credito No Aplicada", ex.InnerException.Message);
                return false;
            }
        }

        private StringBuilder GenerarLineaNC(RESUMEN_RESTA rresta)
        {
            StringBuilder lineaRegreso = new StringBuilder();
            lineaRegreso.Append(rresta.CodCliente + "|");//Tipo de Doc Cliente
            lineaRegreso.Append(rresta.NotaCredito + "|");//Razon Social - En caso Doc cliente es 6
            lineaRegreso.Append(rresta.Percepcion + "|");//Ap Paterno - En caso Doc cliente es 1
            //lineaRegreso.Append("" + "|");//Fecha de NC
            //lineaRegreso.Append("" + "|");//Numero de NC

            return lineaRegreso;
        }

        #endregion

        #endregion

        #region Eventos Textbox

        private void txtOdysseyMonto_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (char.IsNumber(e.Text, 0))
            {
                e.Handled = false;
            }
            else if (e.Text == "." && !txtOdysseyMonto.Text.Contains('.'))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtOdysseyPerce_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (char.IsNumber(e.Text, 0))
            {
                e.Handled = false;
            }
            else if (e.Text == "." && !txtOdysseyPerce.Text.Contains('.'))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtOdysseyMonto_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void txtOdysseyPerce_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        #endregion

        #endregion

        #region TAB Generacion Libro

        private void IniciarTabLibro() 
        {
            CargarEjercicio();
            CargarRutaDefault();
        }

        private void CargarRutaDefault() 
        {
            string fuenteInputLibro = ConfigurationManager.AppSettings.Get("carpetaLibroDefault");
            txtRutaInputLibro.Text = fuenteInputLibro;
        }

        private void CargarEjercicio()
        {
            List<string> ejercicios = gl.ListarEjercicios();
            cboEjercicio.Items.Add(new ComboBoxItem() { Content = "SELECCIONE" });
            foreach (string item in ejercicios)
            {
                cboEjercicio.Items.Add(new ComboBoxItem() { Content = item });
            }
            cboEjercicio.SelectedIndex = 0;
        }

        private void cboEjercicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cboPeriodo.Items.Clear();
            List<string> periodos = gl.ListarPeriodo((((sender as ComboBox).SelectedItem) as ComboBoxItem).Content.ToString());
            cboPeriodo.Items.Add(new ComboBoxItem() { Content = "SELECCIONE" });
            foreach (string item in periodos)
            {
                cboPeriodo.Items.Add(new ComboBoxItem() { Content = item });
            }
            cboPeriodo.SelectedIndex = 0;
        }
        
        //Para una grilla de reportes de los libros
        private void CargarDataLibro()
        {
            
        }

        #region PROCESO GeneracionLibro

        private void btnProcesarArchivo_Click(object sender, RoutedEventArgs e)
        {
            MostrarMensaje("Procesando Archivo ingresados al Servidor...");
            string mensaje = string.Empty;
            bool flag = gl.ProcesarArchivoLibro(ref mensaje, txtRutaInputLibro.Text, cboPeriodo.Text);

            if (flag)
            {
                MostrarMensaje("Los procesos del periodo se generaron.");
                //MostrarMensaje("Los archivos se copiaron correctamente al servidor. Empieza la generación de Excel.");

                //LogApp.LogInfo("Button click", "Inicio de excel:" + cboPeriodo.Text);

                ////Nro de Columnas que separa cada reporte
                //int separadorCliente = 2;
                ////Donde se almacena la ultima fila del cliente impreso
                //int numeroFilaAnteriorCliente = 0;
                //int numeroCliente = 0;

                //CreateExcelDoc excell_app = new CreateExcelDoc();
                //List<LIB_MASTER_CLIENTES> listaClientes = gl.ListarClientesOrdenBAT();

                //pgProcesoLibro.Minimum = 0;
                //pgProcesoLibro.Maximum = listaClientes.Count();
                //pgProcesoLibro.Value = 0;

                //foreach (LIB_MASTER_CLIENTES cliente in listaClientes)
                //{
                //    MostrarMensaje("Procesando Cliente BAT: " + cliente.CODIGO_BAT);
                //    AumentarBarra();
                //    gl.GenerarLibroPeriodo(ref mensaje, cliente, excell_app, ref separadorCliente,
                //                                                             ref numeroFilaAnteriorCliente,
                //                                                             ref numeroCliente, cboPeriodo.Text);
                //}
            }   
            else
                MostrarMensaje(mensaje);
        }

        private void btnGenerarLibro_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            string mensaje = string.Empty;
            bool flag;
            try
            {   
                flag = gl.GenerarTableExcel(ref mensaje, cboPeriodo.Text);

                if (flag) 
                {
                    //Nro de Columnas que separa cada reporte
                    int separadorCliente = 2;
                    //Donde se almacena la ultima fila del cliente impreso
                    int numeroFilaAnteriorCliente = 0;
                    int numeroCliente = 0;

                    CreateExcelDoc excell_app = new CreateExcelDoc();
                    List<LIB_MASTER_CLIENTES> listaClientes = gl.ListarClientesOrdenBAT();

                    //List<LIB_MASTER_CLIENTES> listaClientes = new List<LIB_MASTER_CLIENTES>();
                    //listaClientes.Add(new LIB_MASTER_CLIENTES() { CODIGO_BAT = "161440000000113", OUTLET_NAME = "CHACON DE TTITO FLORENTINA" });
                    
                    pgProcesoLibro.Minimum = 0;
                    pgProcesoLibro.Maximum = listaClientes.Count();
                    pgProcesoLibro.Value = 0;

                    foreach (LIB_MASTER_CLIENTES cliente in listaClientes)
                    {
                        AumentarBarra();
                        MostrarMensaje("Procesando Cliente BAT: " + cliente.CODIGO_BAT);
                        MostrarMensajeProceso(numeroCliente, listaClientes.Count());
                        gl.GenerarLibroPeriodo(ref mensaje, cliente, excell_app, ref separadorCliente,
                                                                                 ref numeroFilaAnteriorCliente,
                                                                                 ref numeroCliente, cboPeriodo.Text);

                        //if (numeroCliente == 500)
                        //    break;
                    }
                    dispatcherTimer.Stop();
                    MostrarMensaje("Se completo la generación de Archivo Excel");
                }
                else
                {
                    MostrarMensaje(mensaje);
                }
            }
            catch (Exception ex)
            {
                mensaje = "Se origino un error en la aplicación - reiniciarla.";
                LogApp.LogError("Ingreso Data Base de datos", ex.InnerException.Message);
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string fuenteInputLibro = ConfigurationManager.AppSettings.Get("carpetaLibroDefault");
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Seleccione Carpeta";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = fuenteInputLibro;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = fuenteInputLibro;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var folder = dlg.FileName;
                txtRutaInputLibro.Text = folder;
            }
        }

        #endregion

        #endregion

        private void AumentarBarra() 
        {
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pgProcesoLibro.SetValue);
            Dispatcher.Invoke(updatePbDelegate,
                              System.Windows.Threading.DispatcherPriority.Background,
                              new object[] { ProgressBar.ValueProperty, pgProcesoLibro.Value + 1 });
        }

        private void MostrarMensaje(string mensaje)
        {
            UpdateTextStatusDelegate updatePbDelegate = new UpdateTextStatusDelegate(txbMensaje.SetCurrentValue );
            Dispatcher.Invoke(updatePbDelegate,
                              System.Windows.Threading.DispatcherPriority.Background,
                              new object[] { TextBlock.TextProperty , mensaje });

            txbMensaje.Text = mensaje;
        }

        private void MostrarMensajeProceso(int numeroCliente, int totalCliente)
        {
            string mensaje = "Procesando : " + numeroCliente + " de " + totalCliente;
            UpdateTextStatusDelegate updatePbDelegate = new UpdateTextStatusDelegate(txbProceso.SetCurrentValue);
            Dispatcher.Invoke(updatePbDelegate,
                              System.Windows.Threading.DispatcherPriority.Background,
                              new object[] { TextBlock.TextProperty, mensaje });

            txbProceso.Text = mensaje;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dt = dt.AddSeconds(1);
            txbConteo.Text = string.Format("{0:HH:mm:ss}", dt);
        }
    }
}
