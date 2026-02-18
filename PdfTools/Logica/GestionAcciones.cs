using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class GestionAcciones
    {
        // Instancias de los gestores necesarios
        GestionParametros gestorParametros = new GestionParametros();
        GestionContenido gestorContenido = new GestionContenido();

        // Permite poner en primer plano el visor
        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int dwProcessId);

        // Establece la ventana del visor en primer plano
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        // Permite minimizar o restaurar una ventana de windows
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;


        // Metodo de entrada generico para procesar las aciones adicionales a realizar
        public void EjecutarAcciones(ContextoEjecucion contexto)
        {
            var acciones = contexto.Acciones;

            // Ejecución secuencial de acciones por el orden en el que se han pasado
            foreach(var accion in acciones.AccionesProceso)
            {
                // Verifica si ya se ha ejecutado la accion
                if(!acciones.AccionesEjecutadas.Contains(accion))
                {
                    switch(accion)
                    {
                        case Enums.AccionesProceso.InsertarQR:
                            EjecutarInsercionQR(contexto);
                            break;

                        case Enums.AccionesProceso.InsertarLoteQR:
                            EjecutarInsercionLoteQR(contexto);
                            break;

                        case Enums.AccionesProceso.Unir:
                            EjecutarFusion(contexto);
                            break;

                        case Enums.AccionesProceso.InsertarMarca:
                            EjecutarInsercionMarcaAgua(contexto);
                            break;

                        case Enums.AccionesProceso.Imprimir:
                            EjecutarImpresion(contexto);
                            break;

                        case Enums.AccionesProceso.Abrir:
                            EjecutarApertura(contexto, esperarCierre: true);
                            break;

                        case Enums.AccionesProceso.Visualizar:
                            EjecutarApertura(contexto, esperarCierre: false);
                            break;

                        case Enums.AccionesProceso.CerrarVisor:
                            Utilidades.CerrarVisor();
                            break;
                    }

                    // Marca la accion como ejecutada
                    acciones.AccionesEjecutadas.Add(accion);
                }
            }
        }

        // Metodo para insertar un QR en el PDF
        private void EjecutarInsercionQR(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            var datosQR = contexto.DatosQR;

            // Indica que se debe insertar el QR
            datosQR.InsertarQR = true;

            // Valida parametros obligatorios
            GestionParametros gestorParametros = new GestionParametros();
            gestorParametros.ValidarParametros(contexto);

            // Insertar QR si no hay errores de configuración
            if(Logger.EstaVacio())
            {
                // Proceso para insertar el QR en el documento
                var procesoPDF = new InsertaQR();

                PdfDocument documento = gestorContenido.AgregarQR(contexto);

                documento.Save(parametros.PdfSalida);

                // Se actualiza el fichero por si hay que ejecutar acciones adicionales
                contexto.PdfActual = parametros.PdfSalida;
            }
            else
            {
                throw new Exception("Error al insertar el QR");
            }
        }

        private void EjecutarInsercionLoteQR(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;

            // Se chequea que exista la carpeta de entrada antes de procesar nada
            if(parametros.ProcesarCarpeta)
            {
                // Valida si la carpeta de ficheros existe
                if(!Directory.Exists(parametros.CarpetaEntrada))
                {
                    throw new Exception($"La carpeta de entrada \"{parametros.CarpetaEntrada}\" no existe");
                }
            }
            // Instancia del gestor de lotes
            GestionLotes gestorLotes = new GestionLotes();

            // Llamada al método que procesa el lote de ficheros
            gestorLotes.ProcesarLoteQR(contexto);
        }

        private void EjecutarInsercionMarcaAgua(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            var datosQR = contexto.DatosQR;

            // Insertar unicamente la marca de agua a un PDF individual 
            string textoMarcaAgua = parametros.TextoMarcaAgua;
            if(textoMarcaAgua.Length > 0)
            {
                PdfDocument documento = gestorContenido.AgregarMarcaAgua(contexto);

                // Si no se pasa el fichero de salida se genera uno con el mismo nombre del de entrada y un sufijo para no machacarlo
                parametros.PdfSalida = string.IsNullOrWhiteSpace(parametros.PdfSalida)
                        ? Path.Combine(parametros.RutaFicheros, Path.GetFileNameWithoutExtension(parametros.PdfEntrada) + "_salida.pdf")
                        : parametros.PdfSalida;

                documento.Save(parametros.PdfSalida);

                // Se actualiza el fichero por si hay que ejecutar acciones adicionales
                contexto.PdfActual = parametros.PdfSalida;
            }
        }


        // Metodo especifico para imprimir un documento
        private void EjecutarImpresion(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            if(string.IsNullOrWhiteSpace(contexto.PdfActual))
            {
                throw new InvalidOperationException("No hay un PDF válido para imprimir.");
            }

            // Configuración del proceso de impresión con SumatraPDF
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = contexto.RutaVisorPdf,
                WorkingDirectory = Path.GetDirectoryName(contexto.RutaVisorPdf),
                Arguments = $"-print-to-default -silent \"{contexto.PdfActual}\"", // Impresora predeterminada y en modo silencioso
                CreateNoWindow = true, // No crea una ventana con el programa
                WindowStyle = ProcessWindowStyle.Hidden, // Se ejecuta de forma oculta
                UseShellExecute = false // No utiliza el shell de windows para ejecutar
            };

            // La impresión siempre debe esperar a que finalice
            contexto.EsperarCierreVisor = true;

            using(var proceso = Process.Start(psi)) // Lanza el proceso configurado
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


        // Metodo para abrir o visualizar el PDF
        private void EjecutarApertura(ContextoEjecucion contexto, bool esperarCierre)
        {
            if(string.IsNullOrWhiteSpace(contexto.PdfActual))
            {
                throw new InvalidOperationException("No hay un PDF válido para abrir.");
            }

            // Parametros para iniciar el proceso del visor
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = contexto.RutaVisorPdf, // Nombre del ejecutable del visor
                WorkingDirectory = Path.GetDirectoryName(contexto.RutaVisorPdf), 
                Arguments = $"-new-window \"{contexto.PdfActual}\"", // Nombre del fichero a abrir en el visor que se abre en una ventana nueva (forzado para que funcione AllowSetForegroundWindow)
                CreateNoWindow = false, // Crea una ventana
                WindowStyle = ProcessWindowStyle.Normal, // Ventana con estado normal
                UseShellExecute = true // Utiliza el shell de windows para ejecutar
            };

            // Actualiza la propiedad del cierre del visor segun el parametro pasado
            contexto.EsperarCierreVisor = esperarCierre;

            // Lanza el proceso para abrir el PDF
            using(var proceso = Process.Start(psi))
            {
                if(proceso != null)
                {
                    // Permite que el nuevo proceso tome el foco
                    AllowSetForegroundWindow(proceso.Id);

                    // Identificador de la ventana que se ha creado en el proceso
                    IntPtr handle = proceso.MainWindowHandle;

                    // Espera a que la ventana se cree
                    for(int i = 0; i < 10 && handle != IntPtr.Zero; i++)
                    {
                        proceso.Refresh(); // Refresca el estado del proceso para obtener el handle actualizado
                        handle = proceso.MainWindowHandle;
                        if(handle == IntPtr.Zero)
                        {
                            System.Threading.Thread.Sleep(200);
                        }
                    }

                    if(handle != IntPtr.Zero)
                    {
                        // Si la ventana está en segundo plano, Windows a veces ignora SetForegroundWindow.
                        // Al minimizar y restaurar, forzamos a Windows a re-evaluar el orden Z (capas).
                        ShowWindow(handle, 6); // 6 = SW_MINIMIZE
                        ShowWindow(handle, 9); // 9 = SW_RESTORE

                        // Forzamos a poner en primer plano la ventana
                        SetForegroundWindow(handle);
                    }
                    if(contexto.EsperarCierreVisor)
                    {
                        proceso.WaitForExit();
                    }
                }
            }
        }


        // Metodo para fusionar los archivos PDF
        private void EjecutarFusion(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;

            // Creacion del documento de fusion
            PdfDocument fusionPDFs = null;

            try
            {
                // Asegurarse de que existe la carpeta de entrada
                if(!Directory.Exists(parametros.CarpetaEntrada))
                {
                    throw new Exception($"La carpeta de entrada \"{parametros.CarpetaEntrada}\" no existe.");
                }

                // Asegurarse que hay al menos 2 ficheros para fusionar
                if(Directory.GetFiles(parametros.CarpetaEntrada, "*.pdf").Length < 2)
                {
                    throw new Exception($"No hay suficientes PDFs para unir en la carpeta \"{parametros.CarpetaEntrada}\".");
                }

                // Fusiona los PDFs usando el gestor del contexto
                fusionPDFs = contexto.GestorFusion.ProcesarFicheros(parametros);

                if(fusionPDFs == null || fusionPDFs.PageCount == 0)
                {
                    throw new Exception("No se encontraron PDFs para unir.");
                }

                // Guardar el PDF fusionado
                fusionPDFs.Save(parametros.PdfSalida);


                // Actualiza el contexto con el PDF resultante
                contexto.PdfActual = parametros.PdfSalida;
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
        public string RutaVisorPdf { get; set; }

        // Carpeta de cache de SumatraPDF
        public string CacheVisorPdf { get; set; }

        // Indica si hay que esperar al cierre del visor
        public bool EsperarCierreVisor { get; set; }

        // Parametros generales del proceso
        public ConfiguracionGeneral Parametros { get; set; }

        // Datos de configuración del QR
        public ConfiguracionQR DatosQR { get; set; }

        // Datos de configuración de las acciones
        public ConfiguracionAcciones Acciones { get; set; }

        // Gestor reutilizable para la unión de PDFs
        public UnirPDFs GestorFusion { get; set; }

    }

}
