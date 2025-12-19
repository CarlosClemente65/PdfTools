using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatosQR = PdfTools.Datos.ConfiguracionQR;
using Acciones = PdfTools.Datos.ConfiguracionAcciones;


namespace PdfTools.Datos
{
    public class ConfiguracionGeneral
    {
        // Rutas de los ficheros
        public static string PdfEntrada { get; set; }
        public static string PdfSalida { get; set; }
        public static string RutaFicheros { get; set; } = Directory.GetCurrentDirectory();
        public static string FicheroSalida { get; set; } // Fichero de control para gestionar cuando termina el programa.
        public static string[] ListaArchivos { get; set; } // Lista de archivos para procesar si se pasa una carpeta


        // Texto de la marca de agua
        public static string MarcaAgua { get; set; }


        // Asigna los parámetros según la clave y valor proporcionados
        public static void AsignaParametros(string clave, string valor)
        {
            switch(clave.ToLower())
            {
                case "pdfentrada":
                    PdfEntrada = Path.GetFullPath(valor.Trim('"'));

                    // Chequea si el fichero existe para asignar la ruta de ficheros
                    if(File.Exists(PdfEntrada))
                    {
                        RutaFicheros = Path.GetDirectoryName(PdfEntrada);
                    }
                    break;

                case "pdfsalida":
                    // Asigna el PDF de salida eliminando las comillas si las tiene
                    if(!string.IsNullOrEmpty(valor))
                    {
                        PdfSalida = Path.GetFullPath(valor.Trim('"'));
                    }
                    break;

                case "url":
                    // Si se pasa la URL, se usa esa directamente
                    DatosQR.UrlEnvio = valor;
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
                        DatosQR.EntornoProduccion = false;
                    }
                    break;

                case "verifactu":
                    // Define si se usa el sistema VeriFactu
                    if(string.Equals(valor, "si", StringComparison.OrdinalIgnoreCase))
                    {
                        DatosQR.VeriFactu = true;
                        DatosQR.TextoAbajo = "VERI*FACTU"; // Si es VeriFactu, se pone el texto abajo
                    }
                    break;

                case "nifemisor":
                    // Asigna el NIF del emisor
                    DatosQR.NifEmisor = valor;
                    if(!string.IsNullOrEmpty(DatosQR.NifEmisor))
                    {
                        // Si se ha pasado el NIF del emisor, se insertara el QR
                        DatosQR.InsertarQR = true;
                    }
                    break;

                case "numerofactura":
                    // Asigna el número de la factura
                    DatosQR.NumeroFactura = valor;
                    break;

                case "fechafactura":
                    // Define los formatos de fecha válidos
                    string[] formatosValidos = { "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy" };

                    // Intentar parsear la fecha con los formatos válidos
                    if(DateTime.TryParseExact(valor, formatosValidos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime fecha))
                    {
                        DatosQR.FechaFactura = fecha;
                    }
                    else
                    {
                        DatosQR.FechaFactura = DateTime.MinValue; // Valor inválido
                    }
                    break;

                case "totalfactura":
                    // Asigna el total de la factura
                    if(!decimal.TryParse(valor, out decimal total)) // Evita una excepcion si no se pasa el total correcto
                    {
                        total = 0m;
                    }
                    DatosQR.TotalFactura = total;
                    break;

                case "posicionx":
                    // Asigna la posición X del QR
                    DatosQR.PosX = double.Parse(valor);
                    break;

                case "posiciony":
                    // Asigna la posición Y del QR
                    DatosQR.PosY = double.Parse(valor);
                    break;

                case "ancho":
                    // Asigna el ancho y alto del QR
                    DatosQR.Ancho = double.Parse(valor);
                    DatosQR.Alto = DatosQR.Ancho; // Mantener proporción cuadrada
                    break;

                case "color":
                    // Asigna el color del QR
                    DatosQR.ColorQR = valor;
                    break;

                case "marcaagua":
                    // Asigna la marca de agua, reemplazando \n por saltos de línea
                    MarcaAgua = valor.Replace("\\n", "\n");
                    break;

                case "accionpdf":
                    // Define distintas acciones a realizar con el visor SumatraPDF que permite imprimir, abrir o visualizar el PDF
                    switch(valor.ToLower())
                    {
                        case "imprimir":
                            Acciones.AccionPDF = Acciones.AccionesPDF.Imprimir;
                            Acciones.EjecutarAcciones = true;
                            break;

                        case "abrir":
                            Acciones.AccionPDF = Acciones.AccionesPDF.Abrir;
                            Acciones.EjecutarAcciones = true;
                            break;

                        case "visualizar":
                            Acciones.AccionPDF = Acciones.AccionesPDF.Visualizar;
                            Acciones.EjecutarAcciones = true;
                            break;

                    }
                    break;

                case "ficherosalida":
                    // Fichero para controlar si se ha terminado el proceso
                    FicheroSalida = valor;
                    // Revisa si existe el fichero para borrarlo antes
                    if(File.Exists(FicheroSalida))
                    {
                        File.Delete(FicheroSalida);
                    }
                    break;

                case "idioma":
                    // Codigo de idioma en la respuesta de la AEAT al cotejo del QR
                    DatosQR.IdiomasQR idiomaQR;
                    bool esValido = Enum.TryParse(
                        valor,
                        ignoreCase: true,
                        out idiomaQR
                        );

                    if(esValido && Enum.IsDefined(typeof(DatosQR.IdiomasQR), idiomaQR))
                    {
                        DatosQR.IdiomaQR = idiomaQR;
                    }
                    break;
            }
        }

        public static StringBuilder ValidarParametros(StringBuilder resultado)
        {
            // Validar parámetros obligatorios
            if(string.IsNullOrEmpty(PdfEntrada))
            {
                resultado.AppendLine("El parámetro 'pdfEntrada' es obligatorio.");
            }

            if(!File.Exists(PdfEntrada))
            {
                resultado.AppendLine("El PDF de entrada no existe.");
            }

            // Chequea si se no ha pasado un fichero QR externo para validar los parametros necesarios para generarlo
            if(DatosQR.UsarQrExterno == false)
            {
                // Genera la URL de envío del QR si no se ha pasado segun el resto de parametros 
                if(string.IsNullOrEmpty(DatosQR.UrlEnvio))
                {
                    DatosQR.UrlEnvio = Utilidades.ObtenerUrl(DatosQR.EntornoProduccion, DatosQR.VeriFactu);
                }

                // Valida que se haya pasado el numero de factura
                if(string.IsNullOrEmpty(DatosQR.NumeroFactura))
                {
                    resultado.AppendLine("El parámetro 'numeroFactura' es obligatorio.");
                }

                // Valida que se haya pasado la fecha de la factura
                if(DatosQR.FechaFactura == DateTime.MinValue)
                {
                    resultado.AppendLine("El parámetro 'fechaFactura' es obligatorio.");
                }

                // Valida que se haya pasado el total de la factura
                if(DatosQR.TotalFactura == 0)
                {
                    resultado.AppendLine("El parámetro 'totalFactura' es obligatorio.");
                }

                // Valida si el color pasado es valido
                if(!Utilidades.ColorValido(DatosQR.ColorQR))
                {
                    resultado.AppendLine("El codigo de color del QR no es valido");
                }

                // Con los datos anteriores correctos se genera la URL de envio del QR
                Utilidades.GenerarURL();
            }

            // En caso de que se pase un fichero con el QR, valida que exista
            else
            {
                // Chequea que el fichero del QR existe
                if(!File.Exists(DatosQR.NombreFicheroQR))
                {
                    resultado.AppendLine("El fichero del código QR no existe.");
                }
            }

            return resultado;
        }
    }
}
