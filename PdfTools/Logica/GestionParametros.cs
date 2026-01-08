using System;
using System.IO;
using System.Text;
using PdfTools.Datos;

namespace PdfTools.Logica
{
    public class GestionParametros
    {
        public ConfiguracionAcciones Acciones { get; private set; }
        public ConfiguracionGeneral Parametros { get; private set; }
        public ConfiguracionQR DatosQR { get; private set; }


        public GestionParametros()
        {
            // Asigna las referencias a las instancias
            Acciones = Datos.Instancias.Acciones;
            Parametros = Datos.Instancias.ConfiguracionGeneral;
            DatosQR = Datos.Instancias.ConfiguracionQR;
        }

        // Asigna los parámetros según la clave y valor proporcionados
        public void AsignaParametros(string clave, string valor)
        {
            switch(clave.ToLower())
            {
                case "pdfentrada":
                    Parametros.PdfEntrada = Path.GetFullPath(valor.Trim('"'));

                    // Chequea si el fichero existe para asignar la ruta de ficheros
                    if(File.Exists(Parametros.PdfEntrada))
                    {
                        Parametros.RutaFicheros = Path.GetDirectoryName(Parametros.PdfEntrada);

                        // Asigna el nombre del fichero de salida por defecto
                        Parametros.PdfSalida = Path.Combine(Parametros.RutaFicheros, Path.GetFileNameWithoutExtension(Parametros.PdfEntrada) + "_salida.pdf");
                    }
                    break;

                case "pdfsalida":
                    // Asigna el PDF de salida eliminando las comillas si las tiene
                    if(!string.IsNullOrEmpty(valor))
                    {
                        Parametros.PdfSalida = Path.GetFullPath(valor.Trim('"'));
                    }
                    break;

                case "url":
                    // Si se pasa la URL, se usa esa directamente
                    DatosQR.DatosUrl.UrlEnvio = valor;
                    DatosQR.InsertarQR = true; // Al pasar la url hay que insertar el QR en el PDF
                    break;

                case "ficheroqr":
                    // Si se pasa un fichero de QR, se usa ese directamente
                    if(!string.IsNullOrEmpty(valor))
                    {
                        DatosQR.NombreFicheroQR = Path.GetFullPath(valor.Trim('"'));
                        DatosQR.UsarQrExterno = true; // Se indica que se usará un fichero externo
                        DatosQR.InsertarQR = true; // Si se pasa un fichero con el QR hay que insertarlo en el PDF
                    }
                    break;

                case "entorno":
                    // Define el entorno de pruebas o producción
                    if(string.Equals(valor, "pruebas", StringComparison.OrdinalIgnoreCase))
                    {
                        DatosQR.DatosUrl.EntornoProduccion = false;
                    }
                    break;

                case "verifactu":
                    // Define si se usa el sistema VeriFactu
                    if(string.Equals(valor, "si", StringComparison.OrdinalIgnoreCase))
                    {
                        DatosQR.DatosUrl.VeriFactu = true;
                        DatosQR.DatosAdicionales.TextoAbajo = "VERI*FACTU"; // Si es VeriFactu, se pone el texto abajo
                    }
                    break;

                case "nifemisor":
                    // Asigna el NIF del emisor
                    DatosQR.DatosFactura.NifEmisor = valor;
                    if(!string.IsNullOrEmpty(DatosQR.DatosFactura.NifEmisor))
                    {
                        // Si se ha pasado el NIF del emisor, se insertara el QR
                        DatosQR.InsertarQR = true;
                    }
                    break;

                case "numerofactura":
                    // Asigna el número de la factura
                    DatosQR.DatosFactura.NumeroFactura = valor;
                    break;

                case "fechafactura":
                    // Define los formatos de fecha válidos
                    string[] formatosValidos = { "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy" };

                    // Intentar parsear la fecha con los formatos válidos
                    if(DateTime.TryParseExact(valor, formatosValidos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime fecha))
                    {
                        DatosQR.DatosFactura.FechaFactura = fecha;
                    }
                    else
                    {
                        DatosQR.DatosFactura.FechaFactura = DateTime.MinValue; // Valor inválido
                    }
                    break;

                case "totalfactura":
                    // Asigna el total de la factura
                    if(!decimal.TryParse(valor, out decimal total)) // Evita una excepcion si no se pasa el total correcto
                    {
                        total = 0m;
                    }
                    DatosQR.DatosFactura.TotalFactura = total;
                    break;

                case "posicionx":
                    // Asigna la posición X del QR
                    DatosQR.Posicion.PosX = double.Parse(valor);
                    break;

                case "posiciony":
                    // Asigna la posición Y del QR
                    DatosQR.Posicion.PosY = double.Parse(valor);
                    break;

                case "ancho":
                    // Asigna el ancho y alto del QR
                    DatosQR.Posicion.Ancho = double.Parse(valor);
                    DatosQR.Posicion.Alto = DatosQR.Posicion.Ancho; // Mantener proporción cuadrada
                    break;

                case "color":
                    // Asigna el color del QR
                    DatosQR.Posicion.ColorQR = valor;
                    break;

                case "marcaagua":
                    // Asigna la marca de agua, reemplazando \n por saltos de línea
                    DatosQR.MarcaAgua = valor.Replace("\\n", "\n");
                    break;

                case "accionpdf":
                    // Define distintas acciones a realizar con el visor SumatraPDF que permite imprimir, abrir o visualizar el PDF
                    switch(valor.ToLower())
                    {
                        case "imprimir":
                            Acciones.AccionPDF = ConfiguracionAcciones.AccionesPDF.Imprimir;
                            Acciones.EjecutarAcciones = true;
                            break;

                        case "abrir":
                            Acciones.AccionPDF = ConfiguracionAcciones.AccionesPDF.Abrir;
                            Acciones.EjecutarAcciones = true;
                            break;

                        case "visualizar":
                            Acciones.AccionPDF = ConfiguracionAcciones.AccionesPDF.Visualizar;
                            Acciones.EjecutarAcciones = true;
                            break;

                    }
                    break;

                case "ficherosalida":
                    // Fichero para controlar si se ha terminado el proceso
                    Parametros.FicheroSalida = valor;

                    // Revisa si existe el fichero para borrarlo antes
                    if(File.Exists(Parametros.FicheroSalida))
                    {
                        File.Delete(Parametros.FicheroSalida);
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
                        DatosQR.DatosUrl.IdiomaQR = idiomaQR;
                    }
                    break;

                case "carpetaentrada":
                    Parametros.CarpetaEntrada = Path.GetFullPath(valor.Trim('"'));
                    Parametros.ProcesarCarpeta = true;

                    break;


                case "carpetasalida":
                    Parametros.CarpetaSalida = Path.GetFullPath(valor.Trim('"'));
                    if(!Directory.Exists(Parametros.CarpetaSalida))
                    {
                        Directory.CreateDirectory(Parametros.CarpetaSalida);
                    }
                    break;
            }

        }

        public void ValidarParametros()
        {
            // Valida si existe la carpeta de entrada en caso de procesar una carpeta
            if(Parametros.ProcesarCarpeta)
            {
                // Valida si la carpeta de ficheros existe
                if(!Directory.Exists(Parametros.CarpetaEntrada))
                {
                    Logger.Agregar("La carpeta de entrada con los ficheros no existe.");
                    return;
                }
            }
            else
            {
                // Validaciones para el PDF de entrada
                if(string.IsNullOrEmpty(Parametros.PdfEntrada))
                {
                    Logger.Agregar("El parámetro 'pdfEntrada' es obligatorio.");
                    return;
                }

                if(!File.Exists(Parametros.PdfEntrada))
                {
                    Logger.Agregar("El PDF de entrada no existe.");
                    return;
                }
            }


            // ---------- Validaciones para el QR ------------
            if(DatosQR.UsarQrExterno)
            {
                // En caso de que se pase un fichero con el QR, valida que exista
                if(!File.Exists(DatosQR.NombreFicheroQR))
                {
                    Logger.Agregar("El fichero del código QR no existe.");
                }
            }
            else
            {

                // Genera la URL de envío del QR si no se ha pasado segun el resto de parametros 
                if(string.IsNullOrEmpty(DatosQR.DatosUrl.UrlEnvio))
                {
                    DatosQR.DatosUrl.UrlEnvio = Utilidades.ObtenerUrl(DatosQR.DatosUrl.EntornoProduccion, DatosQR.DatosUrl.VeriFactu);
                }

                // Validacones de los datos de la factura para generar el QR
                ValidarPropiedad(DatosQR.DatosFactura.NumeroFactura, "numeroFactura");
                ValidarPropiedad(DatosQR.DatosFactura.FechaFactura != DateTime.MinValue, "fechaFactura");
                ValidarPropiedad(!string.IsNullOrEmpty(DatosQR.DatosFactura.NifEmisor), "nifEmisor");
                ValidarPropiedad(DatosQR.DatosFactura.TotalFactura != 0, "totalFactura");

                // Valida si el color pasado es valido
                if(!Utilidades.ColorValido(DatosQR.Posicion.ColorQR))
                {
                    Logger.Agregar("El codigo de color del QR no es valido");
                }


                // Solo se generan la URL si no hay errores en los datos
                if(!Logger.TieneErrores())
                {
                    Utilidades.GenerarURL();
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

    }
}
