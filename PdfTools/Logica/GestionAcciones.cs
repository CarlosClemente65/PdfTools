using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class GestionAcciones
    {
        // Gestiona las acciones de abrir, imprimir o visualizar el PDF con SumatraPDF
        public void ProcesarAcciones(ConfiguracionGeneral parametros, ConfiguracionAcciones acciones)
        {
            // Ruta del ejecutable SumatraPDF 
            string rutaSumatra = Utilidades.rutaSumatra;
            string cacheSumatra = Utilidades.cacheSumatra;

            // Instancia para la fusion de archivos
            UnirPDFs gestorFusion = new UnirPDFs();
            string ficheroPDF = string.Empty;
            PdfDocument fusionPDFs = null;

            // Si no se ha indicado el PDF de salida, se usa el de entrada
            ficheroPDF = string.IsNullOrWhiteSpace(parametros.PdfSalida)
                ? parametros.PdfEntrada
                : parametros.PdfSalida;

            try
            {
                // Borrado de la carpeta de cache antes de la ejecucion
                if(Directory.Exists(Utilidades.cacheSumatra))
                {
                    Directory.Delete(Utilidades.cacheSumatra, true);
                }

                // Controla si esta disponible el programa para evitar excepciones
                if(!File.Exists(Utilidades.rutaSumatra))
                {
                    throw new InvalidOperationException("No se pudo lanzar la impresion del PDF.");
                }

                // Crea un proceso para ejecutar el programa SumatraPDF
                var psi = new ProcessStartInfo();
                psi.FileName = rutaSumatra;
                psi.WorkingDirectory = Path.GetDirectoryName(rutaSumatra);

                bool espera = true; // Indica si hay que esperar al cierre del visor

                //Configura los parametros segun si se va a imprimir, abrir o visualizar el PDF
                switch(acciones.AccionPDF)
                {
                    // Configura el proceso para lanzar la impresion silenciosa en la impresora predeterminada
                    case Enums.AccionesPDF.Imprimir:
                        acciones.AbrirVisor = true;
                        psi.Arguments = $"-print-to-default -silent \"{ficheroPDF}\""; // Imprime el PDF en la impresora predeterminada
                        psi.CreateNoWindow = true; // No crea ninguna ventana
                        psi.WindowStyle = ProcessWindowStyle.Hidden; // El proceso esta oculto
                        psi.UseShellExecute = false; // Ejecuta el proceso directamente sin usar la shell de windows
                        break;

                    case Enums.AccionesPDF.Abrir:
                    case Enums.AccionesPDF.Visualizar:
                        acciones.AbrirVisor = true;
                        psi.Arguments = $"{ficheroPDF}"; // Fichero PDF para abrir o visualizar
                        psi.CreateNoWindow = false; // Se crea la ventana del proceso
                        psi.WindowStyle = ProcessWindowStyle.Normal; // Estilo de la ventana del proceso
                        psi.UseShellExecute = true; // Usa el shell de Windows para abrir SumatraPDF normalmente (ventana visible)

                        // En la accion de visualizar no se espera a cerrar el visor
                        if(acciones.AccionPDF == Enums.AccionesPDF.Visualizar)
                        {
                            espera = false;
                        }

                        break;

                    case Enums.AccionesPDF.Unir:
                        // Fusiona los fichero PDFs de la carpeta
                        fusionPDFs = gestorFusion.ProcesarFicheros(parametros);
                        if(fusionPDFs != null && fusionPDFs.PageCount > 0)
                        {
                            fusionPDFs.Save(parametros.PdfSalida);
                        }

                        // Parametros necesarios para mostrar el PDF fusionado

                        /* Comentado porque de momento no se implanta

                        psi.Arguments = $"{parametros.PdfSalida}"; // Fichero PDF para abrir o visualizar
                        psi.CreateNoWindow = false; // Se crea la ventana del proceso
                        psi.WindowStyle = ProcessWindowStyle.Normal; // Estilo de la ventana del proceso
                        psi.UseShellExecute = true; // Usa el shell de Windows para abrir SumatraPDF normalmente (ventana visible)

                        */

                        break;

                }


                // Solo inicia el proceso configurado si la accion NO es Unir
                if(acciones.AbrirVisor)
                {
                    using(var proceso = Process.Start(psi))
                    {
                        if(espera)
                        {
                            proceso.WaitForExit();

                            // Comprueba el código de salida
                            if(proceso.ExitCode != 0)
                            {
                                throw new InvalidOperationException($"La impresión del PDF falló. Código de salida: {proceso.ExitCode}");
                            }
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Se ha producido un error con el visualizador del PDF. Mensaje: {ex.Message}");
            }
        }


        // Este metodo es copia del que habia en el Program que tengo que revisar si es necesario o hay que cambiar algo en los demas
        private void FusionArchivos(ConfiguracionGeneral parametros, ConfiguracionAcciones acciones)
        {
            // Proceso por lotes en caso de haber pasado una carpeta; el proceso de unir ficheros se gestiona despues de la gestion del QR y la marca de agua
            if(parametros.ProcesarCarpeta && acciones.AccionPDF != Enums.AccionesPDF.Unir)
            {
                StringBuilder resultadoLote = new StringBuilder();

                GestionLotes gestorLotes = new GestionLotes();

                // Asigna la carpeta de salida a la misma de entrada si no se ha pasado
                parametros.CarpetaSalida = string.IsNullOrWhiteSpace(parametros.CarpetaSalida) ?
                    parametros.CarpetaEntrada : parametros.CarpetaSalida;

                // Lista con los documentos a procesar
                List<DocumentoLoteQR> ficherosLote = new List<DocumentoLoteQR>();
                ficherosLote = gestorLotes.CargarFicheros(parametros.CarpetaEntrada);

                // Procesar cada fichero
                foreach(var fichero in ficherosLote)
                {
                    // Controla si se han pasado todos los datos necesarios antes de procesarlo
                    if(fichero.EsValido)
                    {
                        gestorLotes.ProcesarFicheroLote(parametros, fichero, resultadoLote);
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
        }

        public void EjecutarAcciones(ConfiguracionGeneral parametros, ConfiguracionAcciones acciones, ContextoEjecucion contexto)
        {
            // 1. Validaciones básicas
            if(parametros == null)
            {
                throw new ArgumentNullException(nameof(parametros));
            }

            if(acciones == null || acciones.AccionesPDF == null || acciones.AccionesPDF.Count == 0)
            {
                return;
            }

            // 2. Ejecución secuencial de acciones
            foreach(var accion in acciones.AccionesPDF)
            {
                EjecutarAccion(accion, parametros, contexto);
            }
        }


        private void EjecutarAccion(Enums.AccionesPDF accion, ConfiguracionGeneral parametros,     ContextoEjecucion contexto)
        {
            if(contexto == null)
            {
                throw new ArgumentNullException(nameof(contexto));
            }

            switch(accion)
            {
                case Enums.AccionesPDF.Imprimir:
                    EjecutarImpresion(parametros, contexto);
                    break;

                case Enums.AccionesPDF.Abrir:
                    EjecutarApertura(contexto, esperarCierre: true);
                    break;

                case Enums.AccionesPDF.Visualizar:
                    EjecutarApertura(contexto, esperarCierre: false);
                    break;

                case Enums.AccionesPDF.Unir:
                    EjecutarFusion(parametros, contexto);
                    break;

                case Enums.AccionesPDF.Ninguna:
                default:
                    // No se realiza ninguna acción
                    break;
            }
        }

        private void EjecutarImpresion(ConfiguracionGeneral parametros, ContextoEjecucion contexto)
        {
            if(string.IsNullOrWhiteSpace(contexto.PdfActual))
            {
                throw new InvalidOperationException("No hay un PDF válido para imprimir.");
            }

            // Configuración del proceso de impresión con SumatraPDF
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = contexto.RutaSumatra,
                WorkingDirectory = Path.GetDirectoryName(contexto.RutaSumatra),
                Arguments = $"-print-to-default -silent \"{contexto.PdfActual}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };

            // La impresión siempre debe esperar a que finalice
            contexto.EsperarCierreVisor = true;

            using(var proceso = Process.Start(psi))
            {
                if(contexto.EsperarCierreVisor)
                {
                    proceso.WaitForExit();

                    if(proceso.ExitCode != 0)
                    {
                        throw new InvalidOperationException(
                            $"La impresión del PDF falló. Código de salida: {proceso.ExitCode}");
                    }
                }
            }
        }

        private void EjecutarApertura(ContextoEjecucion contexto, bool esperarCierre)
        {
            if(string.IsNullOrWhiteSpace(contexto.PdfActual))
            {
                throw new InvalidOperationException("No hay un PDF válido para abrir.");
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = contexto.RutaSumatra,
                WorkingDirectory = Path.GetDirectoryName(contexto.RutaSumatra),
                Arguments = $"\"{contexto.PdfActual}\"",
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = true
            };

            contexto.EsperarCierreVisor = esperarCierre;

            using(var proceso = Process.Start(psi))
            {
                if(contexto.EsperarCierreVisor)
                {
                    proceso.WaitForExit();
                }
            }
        }

        private void EjecutarFusion(ConfiguracionGeneral parametros, ContextoEjecucion contexto)
        {
            if(parametros == null)
            {
                throw new ArgumentNullException(nameof(parametros));
            }

            if(contexto == null)
            {
                throw new ArgumentNullException(nameof(contexto));
            }

            // TODO revisar esta parte porque el nombre del PDF de salida si no se ha pasado, habra que formarlo manualmente no se puede usar el de entrada
            // Determina la ruta final del PDF fusionado
            string pdfFusionado = string.IsNullOrWhiteSpace(parametros.PdfSalida)
                ? parametros.PdfEntrada
                : parametros.PdfSalida;

            PdfDocument fusionPDFs = null;

            try
            {
                // Si se procesa una carpeta, fusiona todos los PDFs en ella
                if(parametros.ProcesarCarpeta)
                {
                    // Asegurarse de que hay PDFs en la carpeta
                    if(!Directory.Exists(parametros.CarpetaEntrada))
                        throw new InvalidOperationException("La carpeta de entrada no existe.");

                    // Fusiona los PDFs usando el gestor del contexto
                    fusionPDFs = contexto.GestorFusion.ProcesarFicheros(parametros);

                    if(fusionPDFs == null || fusionPDFs.PageCount == 0)
                    {
                        throw new InvalidOperationException("No se encontraron PDFs para fusionar.");
                    }

                    // Guardar el PDF fusionado
                    fusionPDFs.Save(pdfFusionado);
                }

                // Actualiza el contexto con el PDF resultante
                contexto.PdfActual = pdfFusionado;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Error durante la fusión de PDFs: {ex.Message}");
            }
        }

    }

    public class ContextoEjecucion
    {
        // Ruta del PDF sobre el que se está trabajando actualmente
        public string PdfActual { get; set; }

        // Ruta del ejecutable de SumatraPDF
        public string RutaSumatra { get; set; }

        // Carpeta de cache de SumatraPDF
        public string CacheSumatra { get; set; }

        // Indica si hay que esperar al cierre del visor
        public bool EsperarCierreVisor { get; set; }

        // Gestor reutilizable para la unión de PDFs
        public UnirPDFs GestorFusion { get; set; }
    }

}
