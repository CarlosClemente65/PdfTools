using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfTools.Datos;
using PdfTools.Logica;
using PdfTools.Metodos;

namespace PdfTools
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Crea las instancias de las clases para poder acceder a las propiedades de las clases

            var Parametros = new Datos.ConfiguracionGeneral();
            var DatosQR = new Datos.ConfiguracionQR();
            var Acciones = new Datos.ConfiguracionAcciones();
            var gestor = new GestionParametros();

            // Inicializa el log de resultados
            Logger.Limpiar();

            try
            {
                // Validar los parámetros de entrada (si fallan se registran en el log y no se continua)
                if(args.Length < 2)
                {
                    Logger.Agregar("Parámetros insuficientes.");
                    return;
                }

                if(args[0] != "ds123456")
                {
                    Logger.Agregar("Clave de inicio incorrecta.");
                    return;
                }

                string guion = args[1];
                // Control si no existe el archivo del guion
                if(!File.Exists(guion))
                {
                    Logger.Agregar("El archivo de guion no existe.");
                    return;
                }

                // Cargar configuración
                Utilidades.CargarParametros(Parametros, DatosQR, Acciones, guion);

                // Si se ha generado algun error, no continua
                if(Logger.TieneErrores())
                {
                    Logger.Guardar(Parametros.FicheroSalida);
                    return;
                }

                // Si se ha solicitado cerrar el visor, se cierra antes de iniciar el proceso
                if(Acciones.CerrarVisor)
                {
                    Utilidades.CerrarVisor();
                }

                // Instancia para gestionar el ocntenido del PDF
                GestionContenido gestorContenido = new GestionContenido();

                // Proceso por lotes en caso de haber pasado una carpeta
                if(Parametros.ProcesarCarpeta)
                {
                    StringBuilder resultadoLote = new StringBuilder();

                    GestionLotes gestionLotes = new GestionLotes();

                    // Asigna la carpeta de salida a la misma de entrada si no se ha pasado
                    Parametros.CarpetaSalida = string.IsNullOrWhiteSpace(Parametros.CarpetaSalida) ?
                        Parametros.CarpetaEntrada : Parametros.CarpetaSalida;

                    // Lista con los documentos a procesar
                    List<DocumentoLoteQR> ficherosLote = new List<DocumentoLoteQR>();
                    ficherosLote = gestionLotes.CargarFicheros(Parametros.CarpetaEntrada);

                    foreach(var fichero in ficherosLote)
                    {
                        if(fichero.EsValido)
                        {
                            var datosQRFichero = new ConfiguracionQR();
                            var parametrosFichero = new ConfiguracionGeneral();
                            var guionFichero = fichero.RutaGuion;
                            var accionesFichero = new ConfiguracionAcciones();

                            // Cargar configuración
                            datosQRFichero = Utilidades.CargarParametros(parametrosFichero, datosQRFichero, accionesFichero, guionFichero);

                            // Asigna los valores segun los datos leidos del guion
                            parametrosFichero.PdfEntrada = fichero.RutaPdf;
                            parametrosFichero.RutaFicheros = Parametros.CarpetaSalida;

                            // Controla si se ha pasado un fichero con la imagen
                            if(!string.IsNullOrEmpty(fichero.RutaImagenQR))
                            {
                                datosQRFichero.UsarQrExterno = true;
                                datosQRFichero.NombreFicheroQR = fichero.RutaImagenQR;
                            }

                            // Valida los parametros
                            gestor.ValidarParametros(parametrosFichero, datosQRFichero);

                            // Añadir el QR al PDF
                            gestorContenido.AgregarQR(parametrosFichero, datosQRFichero);

                            // Gestion mensaje para controlar el resultado
                            if(Logger.TieneErrores())
                            {
                                resultadoLote.AppendLine($"- Fichero: {fichero.NombreBase}.pdf: {Logger.Contenido()}");
                            }
                            else
                            {
                                resultadoLote.AppendLine($"- Fichero {fichero.NombreBase}.pdf: QR añadido correctamente");
                            }

                        }
                        else
                        {
                            // Graba el logger cuando no se ha pasado el guion del fichero
                            resultadoLote.AppendLine($"- Fichero {fichero.NombreBase}.pdf: No se han pasado parametros del fichero ");
                        }

                        // Una vez procesao el fichero se limpia el logger para procesar el siguiente fichero
                        Logger.Limpiar();
                    }

                    Logger.Agregar(resultadoLote);
                }

                // Valida parametros obligatorios en caso de que haya que añadir el QR
                else if(DatosQR.InsertarQR)
                {
                    gestor.ValidarParametros(Parametros, DatosQR);

                    // Insertar QR si no hay errores de configuración
                    if(!Logger.TieneErrores())
                    {
                        // Proceso para insertar el QR en el documento
                        var procesoPDF = new InsertaQR();

                        gestorContenido.AgregarQR(Parametros, DatosQR);
                    }
                }
                else
                {
                    // Si no hay que insertar el QR se revisa si hay que añadir la marca de agua
                    string textoMarcaAgua = DatosQR.MarcaAgua;

                    if(!string.IsNullOrEmpty(textoMarcaAgua))
                    {
                        GestionContenido gestorProceso = new GestionContenido();

                        // Carga en el documento el PDF de entrada
                        PdfDocument documento = PdfReader.Open(Parametros.PdfEntrada, PdfDocumentOpenMode.Modify);

                        // Utiliza el mismo documento abierto para añadirle la marca de agua
                        documento = gestorProceso.InsertaMarcaAgua(documento, textoMarcaAgua);

                        // Guarda el PDF modificado en la ruta de salida
                        Parametros.PdfSalida = string.IsNullOrWhiteSpace(Parametros.PdfSalida)
                                ? Path.Combine(Parametros.RutaFicheros, Path.GetFileNameWithoutExtension(Parametros.PdfEntrada) + "_salida.pdf")
                                : Parametros.PdfSalida;
                        documento.Save(Parametros.PdfSalida);
                    }
                }

                // Revisa si hay que ejecutar acciones adicionales
                if(Acciones.EjecutarAcciones)
                {
                    var gestorAcciones = new GestionAcciones();
                    // Ejecuta las acciones adicionales que se hayan solicitado
                    Enums.AccionesPDF accion = Acciones.AccionPDF;
                    gestorAcciones.ProcesarAccion(Parametros, accion);
                }
            }

            catch(InvalidOperationException ex)
            {
                Logger.Agregar(ex.Message);
            }

            catch(Exception ex)
            {
                Logger.Agregar($"Se ha producido un error al procesar el fichero. Mensaje: {ex.Message}");
            }

            finally
            {
                // Al finalizar, genera el fichero del log con el resultado
                Logger.Guardar(Parametros.FicheroSalida);
            }
        }

    }
}
