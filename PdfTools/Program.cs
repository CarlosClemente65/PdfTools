using System;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
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
            var gestor = new Logica.GestionParametros();

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
                    Logger.Guardar();
                    return;
                }

                // Si se ha solicitado cerrar el visor, se cierra antes de iniciar el proceso
                if(Acciones.CerrarVisor)
                {
                    Utilidades.CerrarVisor();
                }

                // Valida parametros obligatorios en caso de que haya que añadir el QR
                if(DatosQR.InsertarQR)
                {
                    gestor.ValidarParametros(Parametros, DatosQR);

                    // Insertar QR si no hay errores de configuración
                    if(!Logger.TieneErrores())
                    {
                        // Proceso para insertar el QR en el documento
                        var procesoPDF = new InsertaQR();
                        PdfDocument documento = procesoPDF.InsertarQR(Parametros.PdfEntrada, DatosQR);

                        if(!Logger.TieneErrores())
                        {
                            // Guarda el PDF modificado en la ruta de salida
                            documento.Save(Parametros.PdfSalida);
                        }
                    }
                }
                else
                {
                    // Si no hay que insertar el QR se revisa si hay que añadir la marca de agua
                    if(!string.IsNullOrEmpty(DatosQR.MarcaAgua))
                    {
                        Metodos.InsertarMarcaAgua gestorProceso = new Metodos.InsertarMarcaAgua();

                        // Carga en el documento el PDF de entrada
                        PdfDocument documento = PdfReader.Open(Parametros.PdfEntrada, PdfDocumentOpenMode.Modify);

                        // Utiliza el mismo documento abierto para añadirle la marca de agua
                        documento = gestorProceso.InsertaMarcaAgua(documento, Parametros, DatosQR);
                        
                        // Guarda el PDF modificado en la ruta de salida
                        documento.Save(Parametros.PdfSalida);
                    }
                }

                // Revisa si hay que ejecutar acciones adicionales
                if(Acciones.EjecutarAcciones)
                {
                    var gestorAcciones = new GestionAcciones();
                    // Ejecuta las acciones adicionales que se hayan solicitado
                    gestorAcciones.();
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

            // Al finalizar, genera el fichero del log con el resultado
            Logger.Guardar();
        }
    }
}
