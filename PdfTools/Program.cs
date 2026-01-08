using System;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Crea las instancias de las clases para poder acceder a las propiedades de las clases
            Instancias.Inicializar();

            // Objeto con el documento para insertar las imagenes
            PdfDocument documento = new PdfDocument();

            // Objeto con la pagina del PDF para añadir las imagenes (QR y marca de agua)
            PdfPage pagina = new PdfPage();

            // Objeto que representa un recuadro donde se incluira el QR y los textos
            XGraphics gfx = null;

            var DatosQR = Datos.Instancias.ConfiguracionQR;
            var Acciones = Datos.Instancias.Acciones;
            var Parametros = Datos.Instancias.ConfiguracionGeneral;

            var gestor = new Logica.GestionParametros();

            // Inicializa el log de resultados
            Logger.Limpiar();

            try
            {
                // Cargar configuración
                Utilidades.CargarParametros(args);

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
                    gestor.ValidarParametros();

                    // Insertar QR si no hay errores de configuración
                    if(!Logger.TieneErrores())
                    {
                        // Carga el documento con el PDF de entrada
                        documento = Utilidades.Generardocumento(Parametros.PdfEntrada);

                        // Establece la pagina 1 para insertar el QR y las imagenes
                        pagina = documento.Pages[0];

                        // Añade el recuadro a la pagina
                        gfx = XGraphics.FromPdfPage(pagina);

                        // Proceso para insertar el QR en el documento
                        var procesoPDF = new InsertaQR();
                        procesoPDF.InsertarQR(pagina, gfx);

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
                        // Carga en el documento el PDF de entrada
                        documento = Utilidades.Generardocumento(Parametros.PdfEntrada);

                        // Establece la pagina 1 para insertar el QR y las imagenes
                        pagina = documento.Pages[0];

                        // Añade el recuadro a la pagina
                        gfx = XGraphics.FromPdfPage(pagina);

                        // Inserta la marca de agua en el PDF
                        Utilidades.InsertaMarcaAgua(pagina, gfx, DatosQR.MarcaAgua);

                        // Guarda el PDF modificado en la ruta de salida
                        documento.Save(Parametros.PdfSalida);
                    }
                }

                // Revisa si hay que ejecutar acciones adicionales
                if(Instancias.Acciones.EjecutarAcciones)
                {
                    // Ejecuta las acciones adicionales que se hayan solicitado
                    Utilidades.GestionarAcciones();
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
