using System;
using System.IO;
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
            // Crea las instancias de las clases para poder acceder a las propiedades de las clases
            var Parametros = new Datos.ConfiguracionGeneral();
            var DatosQR = new Datos.ConfiguracionQR();
            var Acciones = new Datos.ConfiguracionAcciones();
            var gestor = new GestionParametros();

            // Inicializa el log de resultados
            Logger.Limpiar();

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
                Utilidades.CargarParametros(Parametros, DatosQR, Acciones, guion);

                // Si se ha generado algun error, no continua
                if(Logger.TieneContenido())
                {
                    Logger.Guardar(Parametros.FicheroSalida);
                    return;
                }

                // Si se ha solicitado cerrar el visor, se cierra antes de iniciar el proceso
                if(Acciones.CerrarVisor)
                {
                    Utilidades.CerrarVisor();
                }

                // Crear el contexto de ejecución
                var contexto = new ContextoEjecucion
                {
                    PdfActual = Parametros.PdfEntrada,          // PDF inicial
                    RutaSumatra = Utilidades.rutaSumatra,       // Ruta del ejecutable SumatraPDF
                    CacheSumatra = Utilidades.cacheSumatra,     // Ruta de la cache de SumatraPDF
                    GestorFusion = new UnirPDFs(),              // Instancia del gestor de fusión
                    EsperarCierreVisor = true,                  // Valor por defecto
                    AccionGlobal = null                         // No se fija ninguna accion global por defeccto
                };


                // Controla si se ha pasado una accion global
                if(Acciones.AccionesPDF.Count > 0)
                {
                    contexto.AccionGlobal = true;
                }

                // Instancia para gestionar el contenido del PDF
                GestionContenido gestorContenido = new GestionContenido();

                /* --------------- Procesado del guion ------------------- */
                // Insertar el QR a un PDF individual (tambien inserta la marca de agua en caso de ser necesario)
                if(DatosQR.InsertarQR)
                {
                    // Valida parametros obligatorios
                    gestor.ValidarParametros(Parametros, DatosQR);

                    // Insertar QR si no hay errores de configuración
                    if(Logger.EstaVacio())
                    {
                        // Proceso para insertar el QR en el documento
                        var procesoPDF = new InsertaQR();

                        PdfDocument documento = gestorContenido.AgregarQR(Parametros, DatosQR);

                        documento.Save(Parametros.PdfSalida);

                        // Se actualiza el fichero por si hay que ejecutar acciones adicionales
                        contexto.PdfActual = Parametros.PdfSalida;
                    }
                }

                // Procesado de una carpeta para añadir QR (la accion de unir PDFs se gestiona desde las acciones)
                else if(Parametros.ProcesarCarpeta && !Acciones.AccionesPDF.Contains(Enums.AccionesPDF.Unir))
                {
                    GestionLotes gestorLotes = new GestionLotes();

                    gestorLotes.ProcesarLoteQR(Parametros, Acciones, contexto);
                }
                else
                {
                    // Insertar unicamente la marca de agua a un PDF individual 
                    string textoMarcaAgua = DatosQR.MarcaAgua;
                    if(textoMarcaAgua.Length > 0)
                    {
                        PdfDocument documento = gestorContenido.AgregarMarcaAgua(Parametros, DatosQR);

                        // Si no se pasa el fichero de salida se genera uno con el mismo nombre del de entrada y un sufijo para no machacarlo
                        Parametros.PdfSalida = string.IsNullOrWhiteSpace(Parametros.PdfSalida)
                                ? Path.Combine(Parametros.RutaFicheros, Path.GetFileNameWithoutExtension(Parametros.PdfEntrada) + "_salida.pdf")
                                : Parametros.PdfSalida;

                        documento.Save(Parametros.PdfSalida);

                        // Se actualiza el fichero por si hay que ejecutar acciones adicionales
                        contexto.PdfActual = Parametros.PdfSalida;
                    }
                }

                // Procesado de acciones adicionales con la nueva arquitectura
                if(Acciones.EjecutarAcciones && Acciones.AccionesPDF != null && Acciones.AccionesPDF.Count > 0)
                {
                    var gestorAcciones = new GestionAcciones();

                    // Llamada al nuevo método que ejecuta la lista de acciones
                    gestorAcciones.EjecutarAcciones(Parametros, Acciones, contexto);
                }
            }

            catch(Exception ex)
            {
                Logger.Agregar($"Se ha producido un error en el proceso. Mensaje: {ex.Message}");
            }

            finally
            {
                // Al finalizar, genera el fichero del log con el resultado
                Logger.Guardar(Parametros.FicheroSalida);
            }
        }
    }
}
