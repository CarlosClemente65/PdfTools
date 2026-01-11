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
        public PdfDocument InsertaMarcaAgua(PdfDocument documento, string textoMarcaAgua)
        {
            // Establece la pagina 1 para insertar el QR y las imagenes
            PdfPage pagina = documento.Pages[0];

            // Añade el recuadro a la pagina
            XGraphics gfx = XGraphics.FromPdfPage(pagina);

            // Dibuja la marca de agua
            DibujarMarcaAgua(textoMarcaAgua, pagina, gfx);

            return documento;
        }

        public PdfPage InsertaMarcaAgua(PdfPage pagina, string textoMarcaAgua)
        {
            // Añade el recuadro a la pagina
            XGraphics gfx = XGraphics.FromPdfPage(pagina);

            // Dibuja la marca de agua
            DibujarMarcaAgua(textoMarcaAgua, pagina, gfx);

            return pagina;
        }

        private void DibujarMarcaAgua(string textoMarcaAgua, PdfPage pagina, XGraphics gfx)
        {
            string textoError = "Se ha producido un error al insertar la marca de agua.";
            try
            {
                // Fuente y pincel para dibujar el texto
                XFont fuenteMarca = new XFont("Arial", 20, XFontStyle.BoldItalic);
                XBrush pincelMarca = new XSolidBrush(XColor.FromArgb(0, 225, 225, 225)); // Gris muy claro (el primer cero es la transparencia pero no se puede aplicar a un PDF)

                // Ajusta el texto en varias lineas si es necesario
                List<string> lineas = new List<string>();
                string[] bloques = textoMarcaAgua.Split(new string[] { "\n" }, StringSplitOptions.None);
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

        public void AgregarQR(ConfiguracionGeneral parametros, ConfiguracionQR datosQR)
        {
            // Proceso para insertar el QR en el documento
            var procesoPDF = new InsertaQR();

            // Genera el documento PDF para luego poder insertar las imagenes
            PdfDocument documento = PdfReader.Open(parametros.PdfEntrada, PdfDocumentOpenMode.Modify);

            // Se utiliza el mismo documento para añadir el QR
            documento = procesoPDF.InsertarQR(documento, datosQR);

            if(!Logger.TieneErrores())
            {
                // Si no se ha pasado el fichero de salida se asigna un nombre por defecto
                var ficheroPDF = string.IsNullOrWhiteSpace(parametros.PdfSalida)
                    ? Path.Combine(parametros.RutaFicheros, Path.GetFileNameWithoutExtension(parametros.PdfEntrada) + "_salida.pdf")
                    : parametros.PdfSalida;

                documento.Save(ficheroPDF);
            }
        }
    }
}
