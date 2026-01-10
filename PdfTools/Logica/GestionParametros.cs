using System;
using System.Collections.Generic;
using System.IO;
using PdfTools.Datos;

namespace PdfTools.Logica
{
    public class GestionParametros
    {
        // Asigna los parámetros según la clave y valor proporcionados
        public void AsignaParametros(string clave, string valor, ConfiguracionGeneral parametros, ConfiguracionQR configQR, ConfiguracionAcciones acciones)
        {
            Enums.tiposParametros tipoParametro = DetectaTipoParametro(clave);
            switch(tipoParametro)
            {
                case Enums.tiposParametros.QR:
                    AsignaParametrosQR(clave, valor, configQR);
                    break;

                case Enums.tiposParametros.General:
                    AsignaParametrosGenerales(clave, valor, parametros);

                    break;

                case Enums.tiposParametros.Accion:
                    AsignaParametrosAcciones(clave, valor, acciones);
                    break;

                case Enums.tiposParametros.Desconocido:
                    // No implementado, pero puede servir para controlar que los parametros pasados sean correctos
                    // Logger.Agregar($"El parametro {clave} es desconocido");
                    break;
            }
        }

        // Asignacion de parametros del QR
        public ConfiguracionQR AsignaParametrosQR(string clave, string valor, ConfiguracionQR datosQR)
        {
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
                    if(string.Equals(valor, "si", StringComparison.OrdinalIgnoreCase))
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
                    if(!decimal.TryParse(valor, out decimal total)) // Evita una excepcion si no se pasa el total correcto
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
                    datosQR.Posicion.ColorQR = valor;
                    break;

                case "marcaagua":
                    // Asigna la marca de agua, reemplazando \n por saltos de línea
                    datosQR.MarcaAgua = valor.Replace("\\n", "\n");
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

            return datosQR;
        }

        public ConfiguracionGeneral AsignaParametrosGenerales(string clave, string valor, ConfiguracionGeneral parametros)
        {
            switch(clave)
            {
                case "pdfentrada":
                    parametros.PdfEntrada = Path.GetFullPath(valor.Trim('"'));

                    // Chequea si el fichero existe para asignar la ruta de ficheros
                    if(File.Exists(parametros.PdfEntrada))
                    {
                        parametros.RutaFicheros = Path.GetDirectoryName(parametros.PdfEntrada);
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
                    parametros.ProcesarCarpeta = true;

                    break;

                case "carpetasalida":
                    parametros.CarpetaSalida = Path.GetFullPath(valor.Trim('"'));
                    if(!Directory.Exists(parametros.CarpetaSalida))
                    {
                        Directory.CreateDirectory(parametros.CarpetaSalida);
                    }
                    break;

            }

            return parametros;

        }

        public ConfiguracionAcciones AsignaParametrosAcciones(string clave, string valor, ConfiguracionAcciones acciones)
        {
            // Define distintas acciones a realizar con el visor SumatraPDF que permite imprimir, abrir o visualizar el PDF
            switch(clave)
            {
                case "accionpdf":
                    switch(valor.ToLower())
                    {
                        case "imprimir":
                            acciones.AccionPDF = Enums.AccionesPDF.Imprimir;
                            acciones.EjecutarAcciones = true;
                            break;

                        case "abrir":
                            acciones.AccionPDF = Enums.AccionesPDF.Abrir;
                            acciones.EjecutarAcciones = true;
                            break;

                        case "visualizar":
                            acciones.AccionPDF = Enums.AccionesPDF.Visualizar;
                            acciones.EjecutarAcciones = true;
                            break;

                    }
                    break;
            }

            return acciones;
        }

        public void ValidarParametros(ConfiguracionGeneral parametros, ConfiguracionQR datosQR)
        {
            // Valida si existe la carpeta de entrada en caso de procesar una carpeta
            if(parametros.ProcesarCarpeta)
            {
                // Valida si la carpeta de ficheros existe
                if(!Directory.Exists(parametros.CarpetaEntrada))
                {
                    Logger.Agregar("La carpeta de entrada con los ficheros no existe.");
                    return;
                }
            }
            else
            {
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
                    datosQR.DatosUrl.UrlEnvio = Utilidades.ObtenerUrl(datosQR);
                }

                // Validaciones de los datos de la factura para generar el QR
                ValidarPropiedad(datosQR.DatosFactura.NumeroFactura, "numeroFactura");
                ValidarPropiedad(datosQR.DatosFactura.FechaFactura != DateTime.MinValue, "fechaFactura");
                ValidarPropiedad(!string.IsNullOrEmpty(datosQR.DatosFactura.NifEmisor), "nifEmisor");
                ValidarPropiedad(datosQR.DatosFactura.TotalFactura != 0, "totalFactura");

                // Valida si el color pasado es valido
                if(!Utilidades.ColorValido(datosQR.Posicion.ColorQR))
                {
                    Logger.Agregar("El codigo de color del QR no es valido");
                }

                // Solo se generan la URL si no hay errores en los datos
                if(!Logger.TieneErrores())
                {
                    Utilidades.GenerarURL(datosQR);
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

        private void ValidarPropiedad(string valor, string nombrePropiedad)
        {
            if(string.IsNullOrEmpty(valor))
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
            "nifemisor",
            "numerofactura",
            "fechafactura",
            "totalfactura",
            "posicionx",
            "posiciony",
            "ancho",
            "color",
            "marcaagua",
            "idioma"

        };

        // Campo con los valores que pueden tener los parametros generales
        private readonly HashSet<string> ParametrosGenerales = new HashSet<string>
        {
            "pdfentrada",
            "pdfsalida",
            "carpetaentrada",
            "carpetasalida",
            "ficherosalida"
        };

        // Campo con los valores que pueden tener los parametros de acciones
        private readonly HashSet<string> ParametrosAcciones = new HashSet<string>
        {
            "accionpdf"
        };

        // Detecta el tipo de parametro que se esta procesando
        private Enums.tiposParametros DetectaTipoParametro(string clave)
        {
            Enums.tiposParametros tipoParametro = Enums.tiposParametros.Desconocido;
            if (clave == "accionpdf")
            {
                tipoParametro = Enums.tiposParametros.Accion;
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
