using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;
using PdfTools.Metodos;

namespace PdfTools.Metodos
{
    // Clase para la gestion de los procesos del lote de ficheros
    public class GestionLotes
    {
        GestionAcciones gestorAcciones = new GestionAcciones();

        // Procesado generico del lote de ficheros
        public void ProcesarLote<T>(ContextoEjecucion contexto) where T : IDocumentoLote, new()
        {
            // Este metodo recibe el tipo de documento a procesar, pero se condiciona a que sea del tipo IDocumentoLote y ademas se incluye el new() para poder crear instancias del tipo T
            var parametros = contexto.Parametros;
            var acciones = contexto.Acciones;
            StringBuilder resultadoLote = new StringBuilder();

            // Asigna la carpeta de salida a la misma de entrada si no se ha pasado
            parametros.CarpetaSalida = string.IsNullOrWhiteSpace(parametros.CarpetaSalida) ?
                parametros.CarpetaEntrada : parametros.CarpetaSalida;

            // Lista con los documentos a procesar. Se llama al metodo pasandole el tipo T para cargar la lista con el tipo de documento correspondiente
            List<T> ficherosLote = new List<T>();
            ficherosLote = CargarFicheros<T>(parametros.CarpetaEntrada);

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
                contexto.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.ProtegerLote);

            }
            // Una vez procesados los ficheros se añaden los mensajes del procesado al logger
            Logger.Agregar(resultadoLote);
        }


        // Metodo para la carga generica de ficheros
        public List<T> CargarFicheros<T>(string carpetaEntrada) where T : IDocumentoLote, new()
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

            // Lista del tipo pasado para agregar los nombres de los ficheros necesarios
            List<T> lote = new List<T>();

            // Si hay archivos PDF en la carpeta de entrada se lanza el proceso
            if(ArchivosPDF.Count > 0)
            {
                foreach(string archivoPDF in ArchivosPDF)
                {
                    string nombreBase = Path.GetFileNameWithoutExtension(archivoPDF);
                    string rutaTxt = Path.Combine(carpetaEntrada, nombreBase + ".txt");

                    // Creamos una instancia del tipo T (puede ser DocumentoLoteQR o DocumentoLoteProteger)
                    T doc = new T();
                    doc.NombreBase = nombreBase;
                    doc.RutaPdf = archivoPDF;
                    doc.RutaGuion = File.Exists(rutaTxt) ? rutaTxt : null;

                    // Solo si es un lote de QR, buscamos la imagen
                    if(doc is DocumentoLoteQR qr)
                    {
                        qr.RutaImagenQR = BuscarImagen(carpetaEntrada, nombreBase);
                    }

                    lote.Add(doc);
                }
            }

            return lote;
        }

        // Método auxiliar para buscar los fichero de imagen
        private string BuscarImagen(string carpeta, string nombreBase)
        {
            string[] extensionesImagen = { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tiff" };
            foreach(string fichero in Directory.GetFiles(carpeta, nombreBase + ".*"))
            {
                if(extensionesImagen.Contains(Path.GetExtension(fichero).ToLower()))
                {
                    return fichero;
                }
            }
            return null;
        }

        // Procesado de cada fichero del lote para añadir el QR y la marca de agua
        public void ProcesarFicheroLote(IDocumentoLote fichero, StringBuilder resultadoLote, ContextoEjecucion contexto)
        {
            if(fichero is DocumentoLoteQR ficheroQR)
            {
                // Ejecutar el proceso de insercion del QR
                EjecutarInsertarQR(ficheroQR, resultadoLote, contexto);
            }
            else if(fichero is DocumentoLoteProteger ficheroProteger)
            {
                // Ejecutar el proceso de proteccion del PDF
                EjecutarInsertarProteccion(ficheroProteger, resultadoLote, contexto);
            }
        }

        public void EjecutarInsertarQR(DocumentoLoteQR fichero, StringBuilder resultadoLote, ContextoEjecucion contexto)
        {
            // Se asignan las acciones del fichero
            var accionesFichero = PrepararAccionesFichero(contexto, Enums.AccionesProceso.InsertarLoteQR);

            // Se prepara el contexto del fichero
            ContextoEjecucion contextoFichero = PrepararContextoFichero(contexto, accionesFichero);

            // Se cargan los parametros del fichero, si el guion es valido, se asignan al contexto del fichero
            CargarParametrosFichero(fichero, contextoFichero, contexto);

            // Variables para simplificar la lectura de los datos del contexto del fichero
            var parametrosFichero = contextoFichero.Parametros;
            var datosQRFichero = contextoFichero.DatosQR;

            // Instancias para los gestores de datos
            var gestorParametros = new GestionParametros();
            var gestorContenido = new GestionContenido();

            // Fichero de entrada
            string pdfEntrada = fichero.RutaPdf;

            // Si la carpeta de salida es igual a la de entrada, se añade un sufijo a los ficheros
            string sufijoNombreSalida = parametrosFichero.CarpetaEntrada == parametrosFichero.CarpetaSalida
            ? "_salida.pdf"
            : ".pdf";

            string pdfSalida = string.IsNullOrWhiteSpace(parametrosFichero.PdfSalida)
                ? Path.Combine(parametrosFichero.RutaFicheros, Path.GetFileNameWithoutExtension(parametrosFichero.PdfEntrada) + sufijoNombreSalida)
                : parametrosFichero.PdfSalida;

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
                GrabarFichero(documento, pdfSalida, contextoFichero, resultadoLote, Enums.AccionesProceso.InsertarLoteQR);
            }
            else
            {
                // Si no se pasa el guion del fichero, se copia el fichero de entrada en la ruta de salida sin modificar
                File.Copy(pdfEntrada, pdfSalida, overwrite: true);

                // Se asigna el pdfActual por si hay que hacer acciones adicionales
                contextoFichero.PdfActual = pdfSalida;

                // Se marcan como ejecutadas las acciones de InsertarQR e InsertarLoteQR para evitar que den error (no hay guion)
                contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarQR);
                contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarLoteQR);
                contextoFichero.Acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.InsertarMarca);
            }

            // Gestión de mensajes del Logger
            if(Logger.TieneContenido())
            {
                resultadoLote.AppendLine($"- Fichero: {Path.GetFileNameWithoutExtension(pdfSalida)}: {Logger.Contenido()}");
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


        public void EjecutarInsertarProteccion(DocumentoLoteProteger fichero, StringBuilder resultadoLote, ContextoEjecucion contexto)
        {
            // Se asignan las acciones del fichero
            var accionesFichero = PrepararAccionesFichero(contexto, Enums.AccionesProceso.Proteger);

            // Se prepara el contexto del fichero
            ContextoEjecucion contextoFichero = PrepararContextoFichero(contexto, accionesFichero);

            // Se cargan los parametros del fichero, si el guion es valido, se asignan al contexto del fichero
            CargarParametrosFichero(fichero, contextoFichero, contexto);

            // Variables para simplificar la lectura de los datos del contexto del fichero
            var parametrosFichero = contextoFichero.Parametros;

            // Instancias para los gestores
            var gestorParametros = new GestionParametros();
            var gestorAcciones = new GestionAcciones();
            var gestorContenido = new GestionContenido();

            // Si la carpeta de salida es igual a la de entrada, se añade un sufijo a los ficheros
            string sufijoNombreSalida = parametrosFichero.CarpetaEntrada == parametrosFichero.CarpetaSalida
                ? "_protegido.pdf"
                : ".pdf";

            parametrosFichero.PdfSalida = string.IsNullOrWhiteSpace(parametrosFichero.PdfSalida)
                ? Path.Combine(parametrosFichero.RutaFicheros, Path.GetFileNameWithoutExtension(parametrosFichero.PdfEntrada) + sufijoNombreSalida)
                : parametrosFichero.PdfSalida;


            // Validaciones de contraseñas
            if(!string.IsNullOrEmpty(parametrosFichero.PasswordApertura) &&
                parametrosFichero.PasswordApertura == parametrosFichero.PasswordEdicion)
            {
                Logger.Agregar($"Fichero {fichero.NombreBase}: Las contraseñas no pueden ser iguales.");
            }
            else
            {
                // Llamada al método para aplicar la protección al PDF
                ProtegerPdf.AplicarProteccion(parametrosFichero, contextoFichero);
            }

            // Gestión de mensajes del Logger
            if(Logger.TieneContenido())
            {
                resultadoLote.AppendLine($"- Fichero: {Path.GetFileNameWithoutExtension(parametrosFichero.PdfSalida)}: {Logger.Contenido()}");
            }

            // Se asigna el pdfActual por si hay que hacer acciones adicionales
            contextoFichero.PdfActual = parametrosFichero.PdfSalida;

            // Acciones adicionales
            foreach(var accion in accionesFichero.AccionesProceso)
            {
                if(!accionesFichero.AccionesEjecutadas.Contains(accion))
                {
                    gestorAcciones.EjecutarAcciones(contextoFichero);
                }
            }
        }


        // Metodo para preparar las acciones del fichero, teniendo en cuenta las acciones globales y añadiendo la accion principal del proceso (InsertarLoteQR o Proteger)
        private ConfiguracionAcciones PrepararAccionesFichero(ContextoEjecucion contextoGlobal, Enums.AccionesProceso accionPrincipal)
        {
            var parametros = contextoGlobal.Parametros;
            var acciones = contextoGlobal.Acciones;

            // Creamos el objeto de acciones para este fichero concreto
            var accionesFichero = new ConfiguracionAcciones
            {
                AbrirVisor = acciones.AbrirVisor,
                CerrarVisor = acciones.CerrarVisor,
                AccionesProceso = new HashSet<Enums.AccionesProceso>(),
                AccionesEjecutadas = new HashSet<Enums.AccionesProceso>()
            };

            // Si hay acciones globales, las copiamos al fichero
            if(parametros.AccionGlobal)
            {
                accionesFichero.AccionesProceso = new HashSet<Enums.AccionesProceso>(acciones.AccionesProceso);
                accionesFichero.AccionesEjecutadas = new HashSet<Enums.AccionesProceso>(acciones.AccionesEjecutadas);
            }

            // Aseguramos que la acción principal (ej: InsertarLoteQR o Proteger) esté en la lista
            if(!accionesFichero.AccionesProceso.Contains(accionPrincipal))
            {
                accionesFichero.AccionesProceso.Add(accionPrincipal);
            }

            return accionesFichero;
        }


        // Metodo para preparar el contexto de ejecución del fichero, clonando los parámetros globales y asignando las acciones específicas del fichero
        private ContextoEjecucion PrepararContextoFichero(ContextoEjecucion contextoGlobal, ConfiguracionAcciones accionesFichero)
        {
            // Clonacion de los parámetros generales
            var parametrosFichero = new ConfiguracionGeneral(contextoGlobal.Parametros);

            // Creacion de los datos de QR vacios, se llenarán si el fichero es de tipo QR
            var datosQRFichero = new ConfiguracionQR();

            // Contexto de ejecución del fichero con todos los campos del contexto global
            ContextoEjecucion contextoFichero = new ContextoEjecucion
            {
                Parametros = parametrosFichero,
                DatosQR = datosQRFichero,
                Acciones = accionesFichero,
                PdfActual = null,
                RutaVisorPdf = contextoGlobal.RutaVisorPdf,
                CacheVisorPdf = contextoGlobal.CacheVisorPdf,
                EsperarCierreVisor = contextoGlobal.EsperarCierreVisor
            };

            return contextoFichero;
        }


        // Metodo para cargar los parámetros del guion del fichero, si el guion es valido, y asignar las rutas de entrada y salida al contexto del fichero
        private void CargarParametrosFichero<T>(T fichero, ContextoEjecucion contextoFichero, ContextoEjecucion contextoGlobal) where T : IDocumentoLote
        {
            // Si se ha pasado el fichero pdf y el guion se procesa el fichero
            if(fichero.EsValido)
            {
                // Cargar configuración del guion
                Utilidades.CargarParametros(contextoFichero, fichero.RutaGuion);

                // Si hay texto de marca de agua, añadimos la acción si no estaba
                if(contextoFichero.Parametros.TextoMarcaAgua.Trim() != string.Empty &&
                    !contextoFichero.Acciones.AccionesProceso.Contains(Enums.AccionesProceso.InsertarMarca))
                {
                    contextoFichero.Acciones.AccionesProceso.Add(Enums.AccionesProceso.InsertarMarca);
                }
            }

            // Se asigna la ruta del fichero de entrada
            contextoFichero.Parametros.PdfEntrada = fichero.RutaPdf;

            // Se asigna la ruta de salida de los ficheros
            contextoFichero.Parametros.RutaFicheros = contextoGlobal.Parametros.CarpetaSalida;
        }


        // Metodo para grabar el fichero PDF generado, gestionar los mensajes del logger y ejecutar las acciones adicionales (Visor, Imprimir, etc.)
        private void GrabarFichero(PdfDocument documento, string pdfSalida, ContextoEjecucion contextoFichero, StringBuilder resultadoLote, Enums.AccionesProceso accionPrincipal)
        {
            // Grabacion del documento si tiene contenido
            if(documento != null && documento.PageCount > 0)
            {
                documento.Save(pdfSalida);
                contextoFichero.PdfActual = pdfSalida;
                contextoFichero.Acciones.AccionesEjecutadas.Add(accionPrincipal);
            }
        }


    }
}
