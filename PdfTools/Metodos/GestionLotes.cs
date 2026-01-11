using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class GestionLotes
    {
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

            if(ArchivosPDF.Count > 0)
            {
                // Extensiones validas de arvhicos
                string[] extensionesImagen = { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tiff" };

                foreach(string rutaPdf in ArchivosPDF)
                {
                    string nombreBase = Path.GetFileNameWithoutExtension(rutaPdf);
                    string rutaTxt = Path.Combine(carpetaEntrada, nombreBase + ".txt");

                    string rutaImagen = null;

                    foreach(string fichero in Directory.GetFiles(carpetaEntrada, nombreBase + ".*"))
                    {
                        string extension = Path.GetExtension(fichero).ToLower();

                        if(extensionesImagen.Contains(extension))
                        {
                            rutaImagen = fichero;
                            break;
                        }
                    }

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

    }


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
