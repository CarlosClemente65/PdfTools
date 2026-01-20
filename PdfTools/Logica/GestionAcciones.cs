using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class GestionAcciones
    {
        // Metodo de entrada generico para procesar las aciones adicionales a realizar
        public void EjecutarAcciones(ConfiguracionGeneral parametros, ConfiguracionAcciones acciones, ContextoEjecucion contexto)
        {
            // Si no hay acciones adcionales se vuelve al flujo de ejecucion
            if(acciones.AccionesPDF.Count == 0)
            {
                return;
            }

            // Ejecución secuencial de acciones
            foreach(var accion in acciones.AccionesPDF)
            {
                EjecutarAccion(accion, parametros, contexto);
            }
        }


        // Metodo para ejecutar cada accion de forma individual que se haya pasado por parametros
        private void EjecutarAccion(Enums.AccionesPDF accion, ConfiguracionGeneral parametros, ContextoEjecucion contexto)
        {
            // Ejecucion de cada una de las acciones
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


        // Metodo especifico para imprimir un documento
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

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = contexto.RutaSumatra,
                WorkingDirectory = Path.GetDirectoryName(contexto.RutaSumatra),
                Arguments = $"\"{contexto.PdfActual}\"", // Nombre del fichero a abrir en SumatraPDF
                CreateNoWindow = false, // Crea una ventana
                WindowStyle = ProcessWindowStyle.Normal, // Ventana con estado normal
                UseShellExecute = true // Utiliza el shell de windows para ejecutar
            };

            // Actualiza la propiedad del cierre del visor segun el parametro pasado
            contexto.EsperarCierreVisor = esperarCierre;

            using(var proceso = Process.Start(psi))
            {
                if(contexto.EsperarCierreVisor)
                {
                    proceso.WaitForExit();
                }
            }
        }


        // Metodo para fusionar los archivos PDF
        private void EjecutarFusion(ConfiguracionGeneral parametros, ContextoEjecucion contexto)
        {
            // Creacion del documento de fusion
            PdfDocument fusionPDFs = null;

            try
            {
                // Si se procesa una carpeta, fusiona todos los PDFs en ella
                if(parametros.ProcesarCarpeta)
                {
                    // Asegurarse de que existe la carpeta
                    if(!Directory.Exists(parametros.CarpetaEntrada))
                    {
                        throw new Exception($"La carpeta de entrada \"{parametros.CarpetaEntrada}\" no existe.");
                    }

                    // Asegurarse que hay al menos 2 ficheros para fusionar
                    if (Directory.GetFiles(parametros.CarpetaEntrada, "*.pdf").Length < 2)
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
                }

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
        public string RutaSumatra { get; set; }

        // Carpeta de cache de SumatraPDF
        public string CacheSumatra { get; set; }

        // Indica si hay que esperar al cierre del visor
        public bool EsperarCierreVisor { get; set; }

        // Gestor reutilizable para la unión de PDFs
        public UnirPDFs GestorFusion { get; set; }

        // Controla si se ejecuta una accion global
        public bool? AccionGlobal {  get; set; }
    }

}
