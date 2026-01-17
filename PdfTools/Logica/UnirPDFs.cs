using System;
using System.Collections.Generic;
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

            // Si no se ha pasado la lista de ficheros, se carga con los PDFs de la carpeta
            if (parametros.ListaArchivos.Count == 0)
            {
                foreach (var fichero in ficherosPdfs)
                {
                    parametros.ListaArchivos.Add(fichero.NombreBase);
                }
            }

            // Fusiona los ficheros
            PdfDocument documentoSalida = new PdfDocument();
            FusionarFicheros(parametros, ficherosPdfs, documentoSalida);

            return documentoSalida;
        }

        public void FusionarFicheros(ConfiguracionGeneral parametros, List<DocumentoLoteQR> ficherosPdfs, PdfDocument documentoSalida)
        {
            // Al pasar 'documentoSalida' como referencia no hay que devolver el resultado, ya que se asigna al mismo objeto desde el que se llama al metodo
            if(ficherosPdfs != null && ficherosPdfs.Count > 1)
            {
                foreach(var archivo in parametros.ListaArchivos)
                {
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
                        Logger.Agregar($"El fichero \"{fichero.NombreBase}\" no existe.");
                    }
                    catch(FileFormatException ex)
                    {
                        Logger.Agregar($"El fichero \"{fichero.NombreBase}\" no es correcto. - Error: {ex.Message}");
                    }
                    catch(PdfReaderException ex)
                    {
                        Logger.Agregar($"Error al abrir el fichero \"{fichero.NombreBase}\". - Error: {ex.Message}");
                    }
                    catch(InvalidOperationException ex)
                    {
                        Logger.Agregar($"El fichero \"{fichero.NombreBase}\" no es un fichero PDF valido.");
                    }
                    catch(Exception ex)
                    {
                        Logger.Agregar($"Error al agregar el fichero \"{fichero.NombreBase}\". - Error: {ex.Message}");
                    }
                }
            }
        }
    }
}
