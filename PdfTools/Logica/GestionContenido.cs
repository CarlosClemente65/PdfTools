using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class GestionContenido
    {
        // Inserta la marca de agua pasando el documento PDF
        public PdfDocument InsertaMarcaAgua(PdfDocument documento, ConfiguracionQR datosQR)
        {
            // Establece la pagina 1 para insertar el QR y las imagenes
            PdfPage pagina = documento.Pages[0];

            // Añade el recuadro para insertar los graficos a la pagina
            XGraphics gfx = XGraphics.FromPdfPage(pagina);

            // Dibuja la marca de agua
            DibujarMarcaAgua(datosQR, pagina, gfx);

            return documento;
        }

        // Inserta la marca de agua pasando la pagina del documento (sobrecarga del metodo anterior)
        public PdfPage InsertaMarcaAgua(PdfPage pagina, XGraphics gfx, ConfiguracionQR datosQR)
        {
            // Se pasa por parametro el recuadro donde insertar los graficos porque ya esta creado fuera

            // Dibuja la marca de agua
            DibujarMarcaAgua(datosQR, pagina, gfx);

            return pagina;
        }

        // Proceso para dibujar la marca de agua en el recuadro grafico pasado por parametro
        private void DibujarMarcaAgua(ConfiguracionQR datosQR, PdfPage pagina, XGraphics gfx)
        {
            // Texto de error por defecto.
            string textoError = "Se ha producido un error al insertar la marca de agua.";

            string colorMarca = datosQR.ColorMarca;
            string textoMarca = datosQR.MarcaAgua;

            try
            {
                // Fuente y pincel para dibujar el texto
                XFont fuenteMarca = new XFont("Arial", 20, XFontStyle.BoldItalic);
                XBrush pincelMarca = new XSolidBrush(Utilidades.ConvierteColorAHex(colorMarca)); // Por defecto esta puest un gris claro.

                // Ajusta el texto en varias lineas si es necesario
                List<string> lineas = new List<string>();
                string[] bloques = textoMarca.Split(new string[] { "\n" }, StringSplitOptions.None);
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
            }

            catch(InvalidOperationException ex)
            {
                Logger.Agregar($"{textoError} \n{ex.Message}");
            }

            catch(ArgumentNullException ex)
            {
                Logger.Agregar($"{textoError} \n{ex.Message}");
            }

            catch(ArgumentOutOfRangeException ex)
            {
                Logger.Agregar($"{textoError} \n{ex.Message}");
            }
            catch(Exception ex)
            {
                Logger.Agregar($"{textoError} \n{ex.Message}");
            }
        }


        // Proceso para añadir el QR al documento PDF
        public PdfDocument AgregarQR(ContextoEjecucion contexto)
        {
            var parametros = contexto.Parametros;
            var datosQR = contexto.DatosQR;
            var acciones = contexto.Acciones;

            // Instancia para insertar el QR en el documento
            var procesoPDF = new InsertaQR();

            // Documento PDF para insertar el QR
            PdfDocument documento = new PdfDocument();

            try
            {
                // Genera el documento PDF para luego poder insertar las imagenes
                documento = PdfReader.Open(parametros.PdfEntrada, PdfDocumentOpenMode.Modify);

                // Se utiliza el mismo documento para añadir el QR
                documento = procesoPDF.InsertarQR(documento, datosQR, acciones);

                return documento;
            }
            catch(Exception ex)
            {
                throw new Exception($"Error al insertar el QR en el fichero {parametros.PdfEntrada}. Mensaje: {ex.Message}");
            }


        }


        // Proceso para añadir la marca de agua al documento PDF
        public PdfDocument AgregarMarcaAgua(ConfiguracionGeneral parametros, ConfiguracionQR datosQR)
        {
            // Creacion del documento para añadir la marcar de agua
            PdfDocument documento = null;

            // Comprueba si hay texto para añadir y no provocar una excepcion
            if(!string.IsNullOrEmpty(datosQR.MarcaAgua))
            {
                GestionContenido gestorProceso = new GestionContenido();

                // Carga en el documento el PDF de entrada
                documento = PdfReader.Open(parametros.PdfEntrada, PdfDocumentOpenMode.Modify);

                // Utiliza el mismo documento abierto para añadirle la marca de agua
                documento = gestorProceso.InsertaMarcaAgua(documento, datosQR);

            }

            return documento;
        }
    }
}
