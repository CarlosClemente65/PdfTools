using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using PdfTools.Datos;
using PdfTools.Metodos;

namespace PdfTools.Logica
{
    // Clase para gestionar los parametros del guion
    public class GestionParametros
    {
        // Los parametros se asignan segun el tipo que sean (QR, general o acciones)
        public void AsignaParametros(string clave, string valor, ContextoEjecucion contexto)
        {
            Enums.tiposParametros tipoParametro = DetectaTipoParametro(clave);
            switch(tipoParametro)
            {
                case Enums.tiposParametros.QR:
                    AsignaParametrosQR(clave, valor, contexto);
                    break;

                case Enums.tiposParametros.General:
                    AsignaParametrosGenerales(clave, valor, contexto);
                    break;

                case Enums.tiposParametros.Acciones:
                    AsignaParametrosAcciones(clave, valor, contexto);
                    break;

                case Enums.tiposParametros.Desconocido:
                    // No implementado, pero puede servir para controlar que los parametros pasados sean correctos
                    // Logger.Agregar($"El parametro {clave} es desconocido");
                    break;
            }
        }

        // Asignacion de parametros del QR
        public void AsignaParametrosQR(string clave, string valor, ContextoEjecucion contexto)
        {
            var datosQR = contexto.DatosQR;
            var acciones = contexto.Acciones;
            switch(clave)
            {
                case "entorno":
                    // Define el entorno de pruebas o producción
                    if(string.Equals(valor, "pruebas", StringComparison.OrdinalIgnoreCase))
                    {
                        datosQR.DatosUrl.EntornoProduccion = false;
                    }
                    break;

                case "verifactu":
                    // Define si se usa el sistema VeriFactu
                    var valoresValidos = new[] { "si", "s", "true" };
                    if(valoresValidos.Contains(valor.ToLower()))
                    {
                        datosQR.DatosUrl.VeriFactu = true;
                        datosQR.DatosAdicionales.TextoAbajo = "VERI*FACTU"; // Si es VeriFactu, se pone el texto abajo
                    }
                    break;

                case "ficheroqr":
                    // Si se pasa un fichero de QR, se usa ese directamente
                    if(!string.IsNullOrEmpty(valor))
                    {
                        datosQR.NombreFicheroQR = Path.GetFullPath(valor.Trim('"'));
                        datosQR.UsarQrExterno = true; // Se indica que se usará un fichero externo
                        datosQR.InsertarQR = true; // Si se pasa un fichero con el QR hay que insertarlo en el PDF
                    }
                    break;

                case "url":
                    // Si se pasa la URL, se usa esa directamente
                    datosQR.DatosUrl.UrlEnvio = valor;
                    datosQR.InsertarQR = true; // Al pasar la url hay que insertar el QR en el PDF
                    break;

                case "omitirqr":
                    // En el caso de no querer inserte el QR, se fuerza la no inserción aunque se pasen otros parametros para el QR
                    var opcionesValidas = new[] { "si", "s", "true" };
                    if(opcionesValidas.Contains(valor.ToLower()))
                    {
                        datosQR.OmitirQR = true;
                    }
                    break;

                case "nifemisor":
                    // Asigna el NIF del emisor
                    datosQR.DatosFactura.NifEmisor = valor;
                    if(!string.IsNullOrEmpty(datosQR.DatosFactura.NifEmisor))
                    {
                        // Si se ha pasado el NIF del emisor, se insertara el QR
                        datosQR.InsertarQR = true;
                    }
                    break;

                case "numerofactura":
                    // Asigna el número de la factura
                    datosQR.DatosFactura.NumeroFactura = valor;
                    break;

                case "fechafactura":
                    // Define los formatos de fecha válidos
                    string[] formatosValidos = { "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy" };

                    // Intentar parsear la fecha con los formatos válidos
                    if(DateTime.TryParseExact(valor, formatosValidos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime fecha))
                    {
                        datosQR.DatosFactura.FechaFactura = fecha;
                    }
                    else
                    {
                        datosQR.DatosFactura.FechaFactura = DateTime.MinValue; // Valor inválido
                    }
                    break;

                case "totalfactura":
                    // Asigna el total de la factura

                    // Detecta el separador decimal
                    int ultimoPunto = valor.LastIndexOf('.');
                    int ultimaComa = valor.LastIndexOf(',');

                    // Asigna el separador usado
                    char separadorDecimal;
                    if(ultimoPunto > ultimaComa)
                    {
                        separadorDecimal = '.';
                    }
                    else if(ultimaComa > ultimoPunto)
                    {
                        separadorDecimal = ',';
                    }
                    else
                    {
                        separadorDecimal = '\0'; // No hay separador decimal
                    }

                    if(separadorDecimal != '\0')
                    {
                        char separadorMiles = separadorDecimal == '.' ? ',' : '.'; // Si el separador decimal es un punto, el de millares sera una coma

                        valor = valor.Replace(separadorMiles.ToString(), ""); // Quita el separador de millares
                        valor = valor.Replace(separadorDecimal, ','); // Fija el separador decimal a una coma
                    }

                    // Intenta convertir el valor a decimal
                    if(!decimal.TryParse(valor, out decimal total))
                    {
                        total = 0m;
                    }
                    datosQR.DatosFactura.TotalFactura = total;
                    break;

                case "posicionx":
                    // Asigna la posición X del QR
                    datosQR.Posicion.PosX = double.Parse(valor);
                    break;

                case "posiciony":
                    // Asigna la posición Y del QR
                    datosQR.Posicion.PosY = double.Parse(valor);
                    break;

                case "ancho":
                    // Asigna el ancho y alto del QR
                    datosQR.Posicion.Ancho = double.Parse(valor);
                    datosQR.Posicion.Alto = datosQR.Posicion.Ancho; // Mantener proporción cuadrada
                    break;

                case "color":
                    // Asigna el color del QR
                    if(Utilidades.ValidaColor(valor))
                    {
                        datosQR.DatosAdicionales.ColorQR = valor;
                    }
                    break;

                case "idioma":
                    // Codigo de idioma en la respuesta de la AEAT al cotejo del QR
                    Enums.IdiomasQR idiomaQR;
                    bool esValido = Enum.TryParse(
                        valor,
                        ignoreCase: true,
                        out idiomaQR
                        );

                    if(esValido && Enum.IsDefined(typeof(Enums.IdiomasQR), idiomaQR))
                    {
                        datosQR.DatosUrl.IdiomaQR = idiomaQR;
                    }
                    break;
            }

            if(datosQR.OmitirQR)
            {
                datosQR.InsertarQR = false; // Si se ha indicado omitir el QR, se fuerza a que no se inserte aunque se pasen otros parametros para el QR
            }
        }

        // Asignacion de parametros generales
        public void AsignaParametrosGenerales(string clave, string valor, ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            var acciones = contexto.Acciones;
            switch(clave)
            {
                case "pdfentrada":
                    parametros.PdfEntrada = Path.GetFullPath(valor.Trim('"'));

                    // Chequea si el fichero existe para asignar la ruta de ficheros
                    if(File.Exists(parametros.PdfEntrada))
                    {
                        parametros.RutaFicheros = Path.GetDirectoryName(parametros.PdfEntrada);
                        contexto.PdfActual = parametros.PdfEntrada;
                    }
                    break;

                case "pdfsalida":
                    // Asigna el PDF de salida eliminando las comillas si las tiene
                    if(!string.IsNullOrEmpty(valor))
                    {
                        parametros.PdfSalida = Path.GetFullPath(valor.Trim('"'));
                    }
                    break;

                case "ficherosalida":
                    // Fichero para controlar si se ha terminado el proceso
                    parametros.FicheroSalida = valor;

                    // Revisa si existe el fichero para borrarlo antes
                    if(File.Exists(parametros.FicheroSalida))
                    {
                        File.Delete(parametros.FicheroSalida);
                    }
                    break;

                case "carpetaentrada":
                    parametros.CarpetaEntrada = Path.GetFullPath(valor.Trim('"'));
                    parametros.ProcesarCarpeta = true; // Indica que se procesará una carpeta completa
                    break;

                case "carpetasalida":
                    parametros.CarpetaSalida = Path.GetFullPath(valor.Trim('"'));
                    if(!Directory.Exists(parametros.CarpetaSalida))
                    {
                        Directory.CreateDirectory(parametros.CarpetaSalida);
                    }
                    break;

                case "listaficheros":
                    // Separa los ficheros de la lista quitando espacios
                    string[] listaPdfs = valor
                        .Split(',')
                        .Select(p => Path.GetFileNameWithoutExtension(p.Trim()))
                        .ToArray();

                    // Añade los ficheros recibidos por orden a la lista para procesar despues
                    parametros.ListaArchivos.AddRange(listaPdfs);

                    break;

                case "textomarca":
                    // Asigna la marca de agua, reemplazando \n por saltos de línea
                    parametros.TextoMarcaAgua = (parametros.ProcesarCarpeta && !string.IsNullOrEmpty(parametros.TextoMarcaAgua)) ? parametros.TextoMarcaAgua : valor.Replace("\\n", "\n");

                    break;

                case "colormarca":
                    // Asigna el color de la marca de agua
                    if(Utilidades.ValidaColor(valor))
                    {
                        parametros.ColorMarca = (parametros.ProcesarCarpeta && parametros.ColorMarca == "#E1E1E1") ? valor : parametros.ColorMarca;
                    }
                    break;
            }
        }

        // Asignacion de parametros de acciones
        public void AsignaParametrosAcciones(string clave, string valor, ContextoEjecucion contexto)
        {
            // Si se han pasado acciones globales, no se asignan las del fichero
            if(contexto.Parametros.AccionGlobal)
            {
                return;
            }

            // Separa las acciones a realizar segun el valor recibido
            string[] listadoAcciones = valor
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(texto => texto.Trim())
                .ToArray();

            foreach(string accion in listadoAcciones)
            {
                // Define distintas acciones a realizar en el proceso
                switch(accion.ToLower())
                {
                    case "insertarqr":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.InsertarQR);
                        break;

                    case "insertarloteqr":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.InsertarLoteQR);
                        break;

                    case "insertarmarca":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.InsertarMarca);
                        break;

                    case "imprimir":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.Imprimir);
                        break;

                    case "abrir":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.Abrir);
                        break;

                    case "visualizar":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.Visualizar);
                        break;

                    case "unir":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.Unir);
                        break;

                    case "cerrarvisor":
                        contexto.Acciones.AccionesProceso.Add(Enums.AccionesProceso.CerrarVisor);
                        contexto.Acciones.CerrarVisor = true;
                        break;
                }
            }
        }


        // Metodo para validacion de parametros obligatorios
        public void ValidarParametros(ContextoEjecucion contexto)
        {
            // Accede a los distintos elementos del contexto
            var parametros = contexto.Parametros;
            var datosQR = contexto.DatosQR;
            var acciones = contexto.Acciones;

            // Validaciones para el PDF de entrada
            if(string.IsNullOrEmpty(parametros.PdfEntrada))
            {
                Logger.Agregar("El parámetro 'pdfEntrada' es obligatorio.");
                return;
            }

            if(!File.Exists(parametros.PdfEntrada))
            {
                Logger.Agregar("El PDF de entrada no existe.");
                return;
            }

            // ---------- Validaciones para el QR ------------
            if(datosQR.UsarQrExterno)
            {
                // En caso de que se pase un fichero con el QR, valida que exista
                if(!File.Exists(datosQR.NombreFicheroQR))
                {
                    Logger.Agregar("El fichero del código QR no existe.");
                }
            }
            else
            {
                // Genera la URL de envío del QR si no se ha pasado segun el resto de parametros 
                if(string.IsNullOrEmpty(datosQR.DatosUrl.UrlEnvio))
                {
                    datosQR.DatosUrl.UrlEnvio = Utilidades.ObtenerUrl(contexto);
                }

                // Validaciones de los datos de la factura para generar el QR
                ValidarPropiedad(!string.IsNullOrEmpty(datosQR.DatosFactura.NifEmisor), "nifEmisor");
                ValidarPropiedad(!string.IsNullOrEmpty(datosQR.DatosFactura.NumeroFactura), "numeroFactura");
                ValidarPropiedad(datosQR.DatosFactura.FechaFactura != DateTime.MinValue, "fechaFactura");
                ValidarPropiedad(datosQR.DatosFactura.TotalFactura != 0, "totalFactura");

                // Valida si el color pasado es valido
                if(!Utilidades.ColorValido(datosQR.DatosAdicionales.ColorQR))
                {
                    Logger.Agregar("El codigo de color del QR no es valido");
                }

                // Solo se generan la URL si no hay errores en los datos
                if(Logger.EstaVacio())
                {
                    Utilidades.GenerarURL(contexto);
                }
            }
        }

        // Método auxiliar para validar propiedades obligatorias y registrar error
        private void ValidarPropiedad(bool condicion, string nombrePropiedad)
        {
            if(!condicion)
            {
                Logger.Agregar($"El parámetro '{nombrePropiedad}' es obligatorio.");
            }
        }

        // Campo con los valores que pueden tener los parametros del QR
        private readonly HashSet<string> ParametrosQR = new HashSet<string>
        {
            "entorno",
            "verifactu",
            "ficheroqr",
            "url",
            "omitirqr",
            "nifemisor",
            "numerofactura",
            "fechafactura",
            "totalfactura",
            "posicionx",
            "posiciony",
            "ancho",
            "color",
            "idioma"

        };

        // Campo con los valores que pueden tener los parametros generales
        private readonly HashSet<string> ParametrosGenerales = new HashSet<string>
        {
            "pdfentrada",
            "pdfsalida",
            "ficherosalida",
            "carpetaentrada",
            "carpetasalida",
            "listaficheros",
            "textomarca",
            "colormarca",
        };

        // Campo con los valores que pueden tener los parametros de acciones
        private readonly HashSet<string> ParametrosAcciones = new HashSet<string>
        {
            "acciones"
        };

        // Detecta el tipo de parametro que se esta procesando
        private Enums.tiposParametros DetectaTipoParametro(string clave)
        {
            Enums.tiposParametros tipoParametro = Enums.tiposParametros.Desconocido;
            if(ParametrosAcciones.Contains(clave))
            {
                tipoParametro = Enums.tiposParametros.Acciones;
            }
            else if(ParametrosQR.Contains(clave))
            {
                tipoParametro = Enums.tiposParametros.QR;
            }
            else if(ParametrosGenerales.Contains(clave))
            {
                tipoParametro = Enums.tiposParametros.General;
            }

            return tipoParametro;
        }
    }
}
