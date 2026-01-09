using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfTools.Datos;
using PdfTools.Logica;



namespace PdfTools
{
    public static class Utilidades
    {
        
        // Carga los parámetros desde el archivo de guion
        public static ConfiguracionQR CargarParametros(ConfiguracionGeneral parametros, ConfiguracionQR datosQR, ConfiguracionAcciones acciones, string guion)
        {
            // Instancia para acceso a los metodos de la gestion de parametros
            Logica.GestionParametros gestor = new Logica.GestionParametros();

            // Leer el archivo de guion y asignar los parámetros
            foreach(string linea in File.ReadAllLines(guion))
            {
                // Salta las lineas vacias
                if(string.IsNullOrWhiteSpace(linea))
                {
                    continue;
                }

                // Separa las lineas del guion en clave y valor
                string[] partes = linea
                    .Split(new char[] { '=' }, 2)
                    .Select(p => p.Trim())
                    .ToArray();

                // Chequea que tenga dos partes (clave y valor) antes de asignar los parametros
                if(partes.Length == 2)
                {
                    gestor.AsignaParametros(partes[0].ToLower(), partes[1], parametros, datosQR, acciones); // La clave se pasa a minusculas para unificar textos
                }
                else if(string.Equals(partes[0], "cerrarvisor", StringComparison.OrdinalIgnoreCase))
                {
                    // El parametro 'cerrarvisor' no tiene dos partes y se trata de forma independiente
                    acciones.CerrarVisor = true;
                }
            }

            return datosQR;
        }


        // Establece la ruta para insertar el QR en funcion del entorno y si aplica Verifactu
        public static string ObtenerUrl(ConfiguracionQR datosQR)
        {
            string urlBase = datosQR.DatosUrl.EntornoProduccion ? datosQR.DatosUrl.UrlProduccionBase : datosQR.DatosUrl.UrlPruebasBase;

            if(datosQR.DatosUrl.VeriFactu)
            {
                return urlBase + "ValidarQR";
            }
            else
            {
                return urlBase + "ValidarQRNoVerifactu";
            }
        }

        // Crea el documento en el que se insertara el QR o los textos
        public static PdfDocument Generardocumento(string rutaPdfEntrada)
        {
            try
            {
                // Genera el documento PDF para luego poder insertar las imagenes
                PdfDocument documento = PdfReader.Open(rutaPdfEntrada, PdfDocumentOpenMode.Modify);
                return documento;
            }

            catch(Exception ex)
            {
                throw new InvalidOperationException($"Se ha producido un error al procesar el PDF. {ex.Message}");
            }

        }

        // Comprueba que el codigo de color sea valido
        public static bool ColorValido(string colorHex)
        {
            return Regex.IsMatch(colorHex, @"^#(?:[0-9a-fA-F]{6})$");
        }

        // Genera la URL con los parámetros del QR UTF-8
        public static void GenerarURL(ConfiguracionQR datosQR)
        {
            // Genera la URL con los parámetros del QR UTF-8
            StringBuilder urlCompleta = new StringBuilder();
            urlCompleta.Append(datosQR.DatosUrl.UrlEnvio).Append("?");
            urlCompleta.Append("nif=").Append(Uri.EscapeUriString(datosQR.DatosFactura.NifEmisor)).Append("&");
            urlCompleta.Append("numserie=").Append(Uri.EscapeUriString(datosQR.DatosFactura.NumeroFactura)).Append("&");
            urlCompleta.Append("fecha=").Append(datosQR.DatosFactura.FechaFactura.ToString("dd-MM-yyyy")).Append("&");
            urlCompleta.Append("importe=").Append(datosQR.DatosFactura.TotalFactura.ToString("F2").Replace(',', '.')); // Asegurar que el decimal es punto
            urlCompleta.Append("&idioma=").Append(datosQR.DatosUrl.IdiomaQR.ToString());

            // Construir la URL completa
            datosQR.DatosUrl.UrlEnvio = urlCompleta.ToString();
        }

        

        // Cierra todas las instancias del visor SumatraPDF que esten abiertas, matando la tarea del administrador de tareas (no implementado, lo dejo para futuras consultas)
        public static void ForzarCerrarVisor()
        {
            // Crea una lista con todos los procesos que hay abiertos de la aplicacion
            foreach(var proceso in Process.GetProcessesByName("SumatraPDF"))
            {
                // Forzar cierre si sigue activo
                proceso.Kill();
            }
        }

        // Cierra todas las instancias del visor SumatraPDF que esten abiertas, mandando un comando al propio programa para cerrarse.
        public static void CerrarVisor()
        {
            // Crea una lista con todos los procesos que hay abiertos de la aplicacion
            foreach(var proceso in Process.GetProcessesByName("SumatraPDF"))
            {
                string argumentos = $"-dde [CmdExit]"; // Comando para cerrar la aplicacion
                var psi = new ProcessStartInfo(rutaSumatra, argumentos); // Crea el proceso
                psi.CreateNoWindow = true; // No crea una ventana 
                psi.UseShellExecute = false; // No se manda como un comando de la Shell
                Process.Start(psi); // Lanza el comando
            }

        }

        // Inserta una marca de agua en la pagina PDF indicada
        public static PdfPage InsertaMarcaAgua(PdfPage pagina, XGraphics gfx, string marcaAgua)
        {
            try
            {
                // Fuente y pincel para dibujar el texto
                XFont fuenteMarca = new XFont("Arial", 20, XFontStyle.BoldItalic);
                XBrush pincelMarca = new XSolidBrush(XColor.FromArgb(0, 225, 225, 225)); // Gris muy claro (el primer cero es la transparencia pero no se puede aplicar a un PDF)

                // Ajusta el texto en varias lineas si es necesario
                List<string> lineas = new List<string>();
                string[] bloques = marcaAgua.Split(new string[] { "\n" }, StringSplitOptions.None);
                string linea = "";

                // Se define un cuadrado seguro de 210x210 mm para insertar la marca de agua
                double margenMm = 10;
                double margen = XUnit.FromMillimeter(margenMm).Point;
                double ladoCuadradoMm = 210;
                double ladoCuadrado = XUnit.FromMillimeter(ladoCuadradoMm).Point;

                // Calcula el centro del cuadrado
                double xInicioCuadrado = margen;
                double yInicioCuadrado = (pagina.Height.Point - ladoCuadrado) / 2;
                double centroX = xInicioCuadrado + ladoCuadrado / 2;
                double centroY = yInicioCuadrado + ladoCuadrado / 2;

                // Calculo del ancho maximo de la marca de agua aproximado a la diagonal del cuadrado seguro)
                double anchoMaximo = ladoCuadrado;

                // Se divide el texto en lineas que no sobrepasen el ancho maximo
                foreach(var bloque in bloques)
                {
                    foreach(var palabra in bloque.Split(' ')) // Separa por palabras
                    {
                        // Primera parte, añadir a la linea actual
                        string textoLinea = string.IsNullOrEmpty(linea) ? palabra : linea + " " + palabra;
                        XSize size = gfx.MeasureString(textoLinea, fuenteMarca);

                        // Si sobrepasa el ancho maximo, se guarda la linea actual y se inicia una nueva
                        if(size.Width > anchoMaximo)
                        {
                            if(!string.IsNullOrEmpty(linea))
                            {
                                lineas.Add(linea);
                            }
                            linea = palabra;
                        }
                        else
                        {
                            linea = textoLinea;
                        }
                    }

                    // Se añade la ultima linea calculada
                    if(!string.IsNullOrEmpty(linea))
                    {
                        lineas.Add(linea);
                        linea = "";
                    }
                }

                // Se guarda la configuracion para aplicarla solo a la marca de agua
                gfx.Save();

                // Rotacion 45 grados a la izquierda para poner la marca de agua
                gfx.RotateAtTransform(-45, new XPoint(centroX, centroY));

                // Posicion inicial del texto (centrado en el cuadro)
                double x = centroX;
                double y = centroY - (lineas.Count * fuenteMarca.Size / 2);


                // Se dibujan una a una las lineas de la marca de agua
                foreach(var l in lineas)
                {
                    gfx.DrawString(l, fuenteMarca, pincelMarca, new XPoint(x, y), XStringFormats.Center);

                    // Se recalcula la posicion del margen Y segun el tamaño de la fuente para desplazarlo hacia abajo
                    y += fuenteMarca.Size;
                }

                // Se restaura la configuracion para aplicar al resto del texto
                gfx.Restore();

                return pagina;
            }

            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Se ha producido un error al insertar la marca de agua. {ex.Message}");
            }
        }
    }
}
