using System;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfTools.Datos;
using PdfTools.Metodos;

namespace PdfTools.Logica
{
    public class UnirPDFs
    {
        GestionLotes gestorLotes = new GestionLotes();
        GestionContenido gestorContenido = new GestionContenido();

        public PdfDocument ProcesarFicheros(ConfiguracionGeneral parametros)
        {
            var ficherosPdfs = gestorLotes.CargarFicheros(parametros.CarpetaEntrada);

            parametros.PdfSalida = string.IsNullOrWhiteSpace(parametros.PdfSalida)
                ? Path.Combine(parametros.CarpetaEntrada, "fichero_salida.pdf")
                : parametros.PdfSalida;

            // Fusiona los ficheros
            var documentoSalida = FusionarFicheros(parametros, ficherosPdfs);

            return documentoSalida;
        }

        public PdfDocument FusionarFicheros(ConfiguracionGeneral parametros, System.Collections.Generic.List<DocumentoLoteQR> ficherosPdfs)
        {
            var documentoSalida = gestorContenido.CrearDocumento();

            if(ficherosPdfs != null && ficherosPdfs.Count > 1)
            {
                foreach(var archivo in parametros.ListaArchivos)
                {
                    //string nombreArchivo = Path.GetFileNameWithoutExtension(archivo);

                    //Buscamos el fichero correspondiente en la lista de PDFs
                    var fichero = ficherosPdfs.FirstOrDefault(f =>
                        string.Equals(f.NombreBase,
                        archivo,
                        StringComparison.OrdinalIgnoreCase));

                    if(fichero == null)
                    {
                        Logger.Agregar($"El fichero {archivo} no existe");
                        continue;
                    }

                    try
                    {
                        using(PdfDocument pdfOrigen = PdfReader.Open(
                            fichero.RutaPdf,
                            PdfDocumentOpenMode.Import))
                        {
                            if(pdfOrigen.PageCount == 0)
                            {
                                continue;
                            }
                            foreach(PdfPage pagina in pdfOrigen.Pages)
                            {
                                documentoSalida.AddPage(pagina);
                            }
                        }
                    }

                    catch(FileNotFoundException ex)
                    {
                        Logger.Agregar($"El fichero {fichero.NombreBase} no existe.\n {ex.Message}");
                    }
                    catch(FileFormatException ex)
                    {
                        Logger.Agregar($"El fichero {fichero.NombreBase} no es correcto.\n{ex.Message}");
                    }
                    catch(PdfReaderException ex)
                    {
                        Logger.Agregar($"Error al abrir el fichero {fichero.NombreBase}.\n {ex.Message}");
                    }
                    catch(Exception ex)
                    {
                        Logger.Agregar($"Error al agregar el fichero {fichero.NombreBase}.\n{ex.Message}");
                    }
                }

            }

            return documentoSalida;
        }
    }
}
