using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;
using PdfTools.Metodos;

namespace PdfTools
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Inicializa el log de resultados
            Logger.Limpiar();

            // Crear el contexto de ejecución
            var contexto = new ContextoEjecucion
            {
                PdfActual = null,                               // PDF inicial
                RutaVisorPdf = Utilidades.rutaSumatra,          // Ruta del ejecutable SumatraPDF
                CacheVisorPdf = Utilidades.cacheSumatra,        // Ruta de la cache de SumatraPDF
                EsperarCierreVisor = true,                      // Valor por defecto
                Parametros = new Datos.ConfiguracionGeneral(),  // Instancia de configuración general
                DatosQR = new Datos.ConfiguracionQR(),          // Instancia de configuración de QR
                Acciones = new Datos.ConfiguracionAcciones(),   // Instancia de configuración de acciones
                GestorFusion = new UnirPDFs()                   // Instancia del gestor de fusión
            };

            var parametros = contexto.Parametros;
            var datosQR = contexto.DatosQR;
            var acciones = contexto.Acciones;

            try
            {
                // Validar los parámetros de entrada (si fallan se registran en el log y no se continua)
                if(args.Length < 2)
                {
                    Logger.Agregar("Parámetros insuficientes.");
                    return;
                }

                if(args[0] != "ds123456")
                {
                    Logger.Agregar("Clave de inicio incorrecta.");
                    return;
                }

                string guion = args[1];
                // Control si no existe el archivo del guion
                if(!File.Exists(guion))
                {
                    Logger.Agregar("El archivo de guion no existe.");
                    return;
                }

                // Cargar configuración
                Utilidades.CargarParametros(contexto, guion);

                // Si se ha generado algun error, no continua
                if(Logger.TieneContenido())
                {
                    Logger.Guardar(parametros.FicheroSalida);
                    return;
                }

                //// Si se ha solicitado cerrar el visor, se cierra antes de iniciar el proceso
                //if(acciones.AccionesProceso.Contains(Enums.AccionesProceso.CerrarVisor))
                //{
                //   Utilidades.CerrarVisor();
                //   acciones.AccionesEjecutadas.Add(Enums.AccionesProceso.CerrarVisor);
                //}

                // Lista de acciones globales que pueden realizarse
                HashSet<Enums.AccionesProceso> accionesGlobales = new HashSet<Enums.AccionesProceso>
                {
                    Enums.AccionesProceso.InsertarMarca,
                    Enums.AccionesProceso.Imprimir,
                    Enums.AccionesProceso.Abrir,
                    Enums.AccionesProceso.Visualizar,
                    Enums.AccionesProceso.Proteger,
                    Enums.AccionesProceso.ProtegerLote
                };


                // Marcar si se va a realizar alguna acción global
                if(acciones.AccionesProceso.Any(a => accionesGlobales.Contains(a)))
                {
                    parametros.AccionGlobal = true;
                }

                // Si se ha pasado en los parametros el 'Textomarca' se asigna la accion
                if(contexto.Parametros.TextoMarcaAgua.Trim() != string.Empty &&
                   !acciones.AccionesProceso.Contains(Enums.AccionesProceso.InsertarMarca))
                {
                    acciones.AccionesProceso.Add(Enums.AccionesProceso.InsertarMarca);
                }

                // Procesado de acciones
                if(acciones.AccionesProceso.Count > 0)
                {
                    var gestorAcciones = new GestionAcciones();

                    // Llamada al nuevo método que ejecuta la lista de acciones
                    gestorAcciones.EjecutarAcciones(contexto);
                }
            }

            catch(Exception ex)
            {
                Logger.Agregar($"Se ha producido un error en el proceso. Mensaje: {ex.Message}");
            }

            finally
            {
                // Al finalizar, genera el fichero del log con el resultado
                Logger.Guardar(parametros.FicheroSalida);
            }
        }
    }
}
