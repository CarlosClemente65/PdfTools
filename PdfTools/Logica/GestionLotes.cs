using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    // Clase para la gestion de los procesos del lote de ficheros
    public class GestionLotes
    {
        GestionAcciones gestorAcciones = new GestionAcciones();
        // Metodo de entrada para procesar el lote de ficheros a añadir el QR
        public void ProcesarLoteQR(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            var acciones = contexto.Acciones;

            StringBuilder resultadoLote = new StringBuilder();

            // Asigna la carpeta de salida a la misma de entrada si no se ha pasado
            parametros.CarpetaSalida = string.IsNullOrWhiteSpace(parametros.CarpetaSalida) ?
                parametros.CarpetaEntrada : parametros.CarpetaSalida;

            // Lista con los documentos a procesar
            List<DocumentoLoteQR> ficherosLote = new List<DocumentoLoteQR>();
            ficherosLote = CargarFicheros(parametros.CarpetaEntrada);

            // Procesar cada fichero
            foreach(var fichero in ficherosLote)
            {
                ProcesarFicheroLote(fichero, resultadoLote, contexto);

                // Una vez procesado el fichero se limpia el logger para procesar el siguiente fichero
                Logger.Limpiar();
            }

            // Si se han pasado acciones de forma global, como se ejecutan con cada fichero, se marcan como completadas
            if(parametros.AccionGlobal)
            {
                contexto.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarMarca); // La marca de agua se inserta a la vez que el QR.
                contexto.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.Abrir);
                contexto.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.Imprimir);
                contexto.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.Visualizar);
            }

            // Una vez procesados los ficheros se añaden los mensajes del procesado al logger
            Logger.Agregar(resultadoLote);

        }

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
                Logger.Agregar($"No hay ningun fichero PDF en la carpeta seleccionada: {ex}");
                return null;
            }

            // Objeto con los nombres de los ficheros necesarios
            List<DocumentoLoteQR> lote = new List<DocumentoLoteQR>();


            // Si hay archivos PDF en la carpeta de entrada se lanza el proceso
            if(ArchivosPDF.Count > 0)
            {
                // Extensiones validas de arvhicos
                string[] extensionesImagen = { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tiff" };

                // Procesa cada archivo PDF leido en la carpeta
                foreach(string archivoPDF in ArchivosPDF)
                {
                    string nombreBase = Path.GetFileNameWithoutExtension(archivoPDF); // Necesario para localizar el resto de ficheros necesarios (guion y imagen del qr)
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
                        RutaPdf = archivoPDF,
                        RutaGuion = File.Exists(rutaTxt) ? rutaTxt : null,
                        RutaImagenQR = rutaImagen
                    };

                    lote.Add(doc);
                }
            }

            return lote;
        }


        // Procesado de cada fichero del lote para añadir el QR y la marca de agua
        public void ProcesarFicheroLote(DocumentoLoteQR fichero, StringBuilder resultadoLote, ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            var acciones = contexto.Acciones;
            var guionFichero = fichero.RutaGuion;

            // Se asignan las acciones del fichero
            var accionesFichero = new ConfiguracionAcciones
            {
                AbrirVisor = acciones.AbrirVisor,
                CerrarVisor = acciones.CerrarVisor,
                AccionesProceso = new HashSet<Enums.AccionesProceso>(),
                AccionesEjecutadas = new HashSet<Enums.AccionesProceso>()
            };

            if(parametros.AccionGlobal)
            {
                // Si se han pasado acciones globlales, se asignan al fichero
                accionesFichero.AccionesProceso = new HashSet<Enums.AccionesProceso>(acciones.AccionesProceso);
                accionesFichero.AccionesEjecutadas = new HashSet<Enums.AccionesProceso>(acciones.AccionesEjecutadas);
            }

            // Detecta si no se ha grabado la accion de InsertarLoteQR para añadirla
            if(!accionesFichero.AccionesProceso.Contains(Enums.AccionesProceso.InsertarLoteQR))
            {
                accionesFichero.AccionesProceso.Add(Enums.AccionesProceso.InsertarLoteQR);
            }

            // Instancias de los objetos necesarias para cada PDF a procesar
            var parametrosFichero = new ConfiguracionGeneral(parametros);
            var datosQRFichero = new ConfiguracionQR();
            // Instancia del contexto de ejecucion para el fichero
            ContextoEjecucion contextoFichero = new ContextoEjecucion
            {
                Parametros = parametrosFichero,
                DatosQR = datosQRFichero,
                Acciones = accionesFichero,
                PdfActual = null,
                RutaVisorPdf = contexto.RutaVisorPdf,
                CacheVisorPdf = contexto.CacheVisorPdf,
                EsperarCierreVisor = contexto.EsperarCierreVisor
            };

            // Instancias para los gestores de datos
            var gestorParametros = new GestionParametros();
            var gestorContenido = new GestionContenido();

            // Si se ha pasado el fichero pdf y el guion se procesa el fichero
            if(fichero.EsValido)
            {
                // Cargar configuración del guion
                Utilidades.CargarParametros(contextoFichero, guionFichero);

                // Si se ha pasado en los parametros el 'Textomarca' se asigna la accion
                if(contextoFichero.Parametros.TextoMarcaAgua.Trim() != string.Empty &&
                   !accionesFichero.AccionesProceso.Contains(Enums.AccionesProceso.InsertarMarca))
                {
                    accionesFichero.AccionesProceso.Add(Enums.AccionesProceso.InsertarMarca);
                }


                // Asigna los valores segun los datos leidos del guion
                parametrosFichero.PdfEntrada = fichero.RutaPdf; // El fichero de entrada siempre sera el PDF leido de la carpeta

                // Se fija la ruta de salida de los ficheros
                parametrosFichero.RutaFicheros = parametros.CarpetaSalida;

                // Si la carpeta de salida es igual a la de entrada, se añade un sufijo a los ficheros
                string sufijoNombreSalida = parametros.CarpetaEntrada == parametros.CarpetaSalida
                    ? "_salida.pdf"
                    : ".pdf";

                // Controla si se ha pasado un fichero con la imagen
                if(!string.IsNullOrEmpty(fichero.RutaImagenQR))
                {
                    datosQRFichero.UsarQrExterno = true;
                    datosQRFichero.NombreFicheroQR = fichero.RutaImagenQR;
                }

                // Valida los parametros
                gestorParametros.ValidarParametros(contextoFichero);

                // Añadir el QR al PDF
                PdfDocument documento = gestorContenido.AgregarQR(contextoFichero);

                // Graba el documento si tiene paginas
                if(documento.PageCount > 0)
                {
                    string pdfSalida = string.IsNullOrWhiteSpace(parametrosFichero.PdfSalida)
                        ? Path.Combine(parametrosFichero.RutaFicheros, Path.GetFileNameWithoutExtension(parametrosFichero.PdfEntrada) + sufijoNombreSalida)
                        : parametrosFichero.PdfSalida;

                    documento.Save(pdfSalida);
                    contextoFichero.PdfActual = pdfSalida;
                    contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarLoteQR);
                }

                // Gestion del mensaje para controlar el resultado en caso de error
                if(Logger.TieneContenido())
                {
                    resultadoLote.AppendLine($"- Fichero: {fichero.NombreBase}.pdf: {Logger.Contenido()}");
                }
            }
            else
            {
                // Si no se pasa el guion del fichero, se copia el fichero de entrada en la ruta de salida sin modificar
                string pdfEntrada = fichero.RutaPdf;

                // Calcula el sufijo a añadir dependiendo si la carpeta de salida es la misma que la de entrada
                string sufijoPdfSalida = parametrosFichero.CarpetaEntrada == parametrosFichero.CarpetaSalida
                    ? "_salida.pdf"
                    : ".pdf";

                // Si no hay carpeta de salida se coge la de entrada
                string carpetaSalida = !string.IsNullOrWhiteSpace(parametrosFichero.CarpetaSalida) ? parametrosFichero.CarpetaSalida : parametrosFichero.CarpetaEntrada;

                // Asigna el nombre del fichero a graba, teniendo en cuenta la carpeta de salida
                string pdfSalida = string.IsNullOrWhiteSpace(parametrosFichero.PdfSalida)
                    ? Path.Combine(carpetaSalida, Path.GetFileNameWithoutExtension(pdfEntrada) + sufijoPdfSalida)
                    : parametrosFichero.PdfSalida;

                File.Copy(pdfEntrada, pdfSalida, overwrite: true);

                // Se asigna el pdfActual por si hay que hacer acciones adicionales
                contextoFichero.PdfActual = pdfSalida;

                // Se marcan como ejecutadas las acciones de InsertarQR e InsertarLoteQR para evitar que den error (no hay guion)
                contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarQR);
                contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarLoteQR);
                contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarMarca);
            }

            // Ejecuta las acciones adicionales que se pasen en el guion del fichero
            foreach(var accion in accionesFichero.AccionesProceso)
            {
                if(!accionesFichero.AccionesEjecutadas.Contains(accion))
                {
                    gestorAcciones.EjecutarAcciones(contextoFichero);
                }
            }
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
