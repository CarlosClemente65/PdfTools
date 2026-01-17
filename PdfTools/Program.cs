using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                if(Logger.TieneContenido())
                {
                    Logger.Guardar(Parametros.FicheroSalida);
                    return;
                }

                // Si se ha solicitado cerrar el visor, se cierra antes de iniciar el proceso
                if(Acciones.CerrarVisor)
                {
                    Utilidades.CerrarVisor();
                }

                // Instancia para gestionar el contenido del PDF
                GestionContenido gestorContenido = new GestionContenido();

                // Proceso por lotes en caso de haber pasado una carpeta; el proceso de unir ficheros se gestiona despues de la gestion del QR y la marca de agua
                if(Parametros.ProcesarCarpeta && Acciones.AccionPDF != Enums.AccionesPDF.Unir)
                {
                    StringBuilder resultadoLote = new StringBuilder();

                    GestionLotes gestorLotes = new GestionLotes();

                    // Asigna la carpeta de salida a la misma de entrada si no se ha pasado
                    Parametros.CarpetaSalida = string.IsNullOrWhiteSpace(Parametros.CarpetaSalida) ?
                        Parametros.CarpetaEntrada : Parametros.CarpetaSalida;

                    // Lista con los documentos a procesar
                    List<DocumentoLoteQR> ficherosLote = new List<DocumentoLoteQR>();
                    ficherosLote = gestorLotes.CargarFicheros(Parametros.CarpetaEntrada);

                    // Procesar cada fichero
                    foreach(var fichero in ficherosLote)
                    {
                        // Controla si se han pasado todos los datos necesarios antes de procesarlo
                        if(fichero.EsValido)
                        {
                            gestorLotes.ProcesarFicheroLote(Parametros, fichero, resultadoLote);
                        }
                        else
                        {
                            // Graba el logger cuando no se ha pasado el guion del fichero
                            resultadoLote.AppendLine($"- Fichero {fichero.NombreBase}.pdf: No se han pasado parametros del fichero ");
                        }

                        // Una vez procesao el fichero se limpia el logger para procesar el siguiente fichero
                        Logger.Limpiar();
                    }

                    // Una vez procesados los ficheros se añaden los mensajes del procesado al logger
                    Logger.Agregar(resultadoLote);
                }

                // Si no se procesa la carpeta, se chequea si hay que insertar el QR a un PDF individual
                else if(DatosQR.InsertarQR)
                {
                    // Valida parametros obligatorios
                    gestor.ValidarParametros(Parametros, DatosQR);

                    // Insertar QR si no hay errores de configuración
                    if(Logger.EstaVacio())
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
                    gestorContenido.AgregarMarcaAgua(Parametros, DatosQR);
                }

                // Revisa si hay que ejecutar acciones adicionales
                if(Acciones.EjecutarAcciones)
                {
                    var gestorAcciones = new GestionAcciones();
                    // Ejecuta las acciones adicionales que se hayan solicitado
                    gestorAcciones.ProcesarAccion(Parametros, Acciones);
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
