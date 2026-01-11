using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    // Clase para la gestion de los procesos del lote de ficheros
    public class GestionLotes
    {
        // Genera una lista con los ficheros PDF a procesar en la carpeta de entrada
        public List<DocumentoLoteQR> CargarFicheros(string carpetaEntrada)
        {
            // Lista de los archivos PDF de la carpeta
            List<string> ArchivosPDF = new List<string>();

            // Carga la lista con los ficheros PDF a procesar
            try
            {
                ArchivosPDF.AddRange(Directory.GetFiles(carpetaEntrada, "*.pdf"));
            }

            catch(Exception ex)
            {
                Logger.Agregar("No hay ningun fichero PDF en la carpeta seleccionada\r\n" + ex);
                return null;
            }

            // Objeto con los nombres de los ficheros necesarios
            List<DocumentoLoteQR> lote = new List<DocumentoLoteQR>();

            // SI hay archivos PDF en la carpeta de entrada se lanza el proceso
            if(ArchivosPDF.Count > 0)
            {
                // Extensiones validas de arvhicos
                string[] extensionesImagen = { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tiff" };

                // Procesa cada archivo PDF leido en la carpeta
                foreach(string rutaPdf in ArchivosPDF)
                {
                    string nombreBase = Path.GetFileNameWithoutExtension(rutaPdf); // Necesario para localizar el resto de ficheros necesarios (guion y imagen del qr)
                    string rutaTxt = Path.Combine(carpetaEntrada, nombreBase + ".txt"); // Nombre del guion que debe tener el mismo nombre que el PDF

                    string rutaImagen = null;

                    // Localiza si existe el fichero con la imagen a insertar en el PDF
                    foreach(string fichero in Directory.GetFiles(carpetaEntrada, nombreBase + ".*"))
                    {
                        string extension = Path.GetExtension(fichero).ToLower();

                        if(extensionesImagen.Contains(extension))
                        {
                            rutaImagen = fichero;
                            break;
                        }
                    }

                    // Crea el objeto con los nombres de los ficheros incluidos en la carpeta.
                    DocumentoLoteQR doc = new DocumentoLoteQR
                    {
                        NombreBase = nombreBase,
                        RutaPdf = rutaPdf,
                        RutaGuion = File.Exists(rutaTxt) ? rutaTxt : null,
                        RutaImagenQR = rutaImagen
                    };

                    lote.Add(doc);
                }
            }

            return lote;
        }


        // Procesasdo de cada fichero del lote para añadir el QR y la marca de agua
        public StringBuilder ProcesarFicheroLote(ConfiguracionGeneral parametros, DocumentoLoteQR fichero, StringBuilder resultadoLote)
        {
            // Instancias de los objetos necesarias para cada PDF a procesar
            var datosQRFichero = new ConfiguracionQR();
            var parametrosFichero = new ConfiguracionGeneral();
            var guionFichero = fichero.RutaGuion;
            var accionesFichero = new ConfiguracionAcciones();

            // Instancias para los gestores de datos
            var gestorParametros = new GestionParametros();
            var gestorContenido = new GestionContenido();

            // Cargar configuración del guion
            datosQRFichero = Utilidades.CargarParametros(parametrosFichero, datosQRFichero, accionesFichero, guionFichero);

            // Asigna los valores segun los datos leidos del guion
            parametrosFichero.PdfEntrada = fichero.RutaPdf; // El fichero de entrada siempre sera el PDF leido de la carpeta
            parametrosFichero.RutaFicheros = parametros.CarpetaSalida; // Se fija la ruta de salida de los ficheros

            // Controla si se ha pasado un fichero con la imagen
            if(!string.IsNullOrEmpty(fichero.RutaImagenQR))
            {
                datosQRFichero.UsarQrExterno = true;
                datosQRFichero.NombreFicheroQR = fichero.RutaImagenQR;
            }

            // Valida los parametros
            gestorParametros.ValidarParametros(parametrosFichero, datosQRFichero);

            // Añadir el QR al PDF
            gestorContenido.AgregarQR(parametrosFichero, datosQRFichero);

            // Gestion del mensaje para controlar el resultado
            if(Logger.TieneContenido())
            {
                resultadoLote.AppendLine($"- Fichero: {fichero.NombreBase}.pdf: {Logger.Contenido()}");
            }
            else
            {
                resultadoLote.AppendLine($"- Fichero {fichero.NombreBase}.pdf: QR añadido correctamente");
            }

            return resultadoLote;
        }

    }


    // Clase para gestionar los nombres de cada uno de los ficheros en el proceso por lotes
    public class DocumentoLoteQR
    {
        // Nombre base del documento (para relacionar los ficheros)
        public string NombreBase { get; set; }

        // Ruta del PDF
        public string RutaPdf { get; set; }

        // Ruta del guion
        public string RutaGuion { get; set; }

        // Ruta de la imagen del QR
        public string RutaImagenQR { get; set; }

        // Control de si tiene todos los ficheros minimos
        public bool EsValido
        {
            get
            {
                return !string.IsNullOrEmpty(RutaPdf) && !string.IsNullOrEmpty(RutaGuion);
            }
        }
    }
}
