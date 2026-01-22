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
        // Instancias de los gestores necesarios
        GestionParametros gestorParametros = new GestionParametros();
        GestionContenido gestorContenido = new GestionContenido();

        // Metodo de entrada generico para procesar las aciones adicionales a realizar
        public void EjecutarAcciones(ContextoEjecucion contexto)
        {
            var acciones = contexto.Acciones;

            // Se crea un HashSet con las acciones a realizar para optimizar las busquedas
            var accionesSet = new HashSet<Enums.AccionesProceso>(acciones.AccionesProceso);

            // Ejecución secuencial de acciones por el orden en el que estan las acciones del Enum
            foreach(var accion in Enum.GetValues(typeof(Enums.AccionesProceso)).Cast<Enums.AccionesProceso>())
            {
                if(accionesSet.Contains(accion))
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

                        case Enums.AccionesProceso.MarcaAgua:
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
        }

        private void EjecutarInsercionLoteQR(ContextoEjecucion contexto)
        {
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
            string textoMarcaAgua = datosQR.MarcaAgua;
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
        public string RutaSumatra { get; set; }

        // Carpeta de cache de SumatraPDF
        public string CacheSumatra { get; set; }

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
