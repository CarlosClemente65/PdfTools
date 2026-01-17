using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class GestionAcciones
    {
        // Gestiona las acciones de abrir, imprimir o visualizar el PDF con SumatraPDF
        public void ProcesarAccion(ConfiguracionGeneral parametros, ConfiguracionAcciones acciones)
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

            //// Si se procesa una carpeta se fusionan los ficheros para la union, impresion o visualizacion
            //if(parametros.ProcesarCarpeta)
            //{
            //    fusionPDFs = gestorFusion.ProcesarFicheros(parametros);
            //    fusionPDFs.Save(parametros.PdfSalida);
            //}

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
    }
}
