using System;
using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;
using PdfTools.Metodos;
using QRCoder;


namespace PdfTools
{
    public class InsertaQR
    {
        private ConfiguracionQR _datosQR = null;

        // Proceso para insertar el codigo QR en el documento PDF
        public PdfDocument InsertarQR(PdfDocument documento, ConfiguracionQR datosQR)
        {
            _datosQR = datosQR; // Se asigna al objeto de clase porque se utiliza en otro metodo de la clase

            // Establece la pagina 1 para insertar el QR y las imagenes
            PdfPage pagina = documento.Pages[0];

            // Añade el recuadro a la pagina
            XGraphics gfx = XGraphics.FromPdfPage(pagina);

            // Configuracion de las propiedades del QR
            string textoQr = datosQR.DatosUrl.UrlEnvio ?? string.Empty;

            // Convierte las posiciones X e Y, y el tamaño del QR a unidades de punto (1/72 pulgadas)
            double posX = XUnit.FromMillimeter(datosQR.Posicion.PosX).Point;
            double posY = XUnit.FromMillimeter(datosQR.Posicion.PosY).Point;
            double ancho = XUnit.FromMillimeter(datosQR.Posicion.Ancho).Point;
            double alto = XUnit.FromMillimeter(datosQR.Posicion.Alto).Point;

            // Convierte el color hexadecimal para usarlo en el QR
            Color colorQR = ColorTranslator.FromHtml(datosQR.Posicion.ColorQR);

            try
            {
                // Genera el codigo QR segun si es una imagen o se han pasado los datos
                XImage qrImage = GenerarQR(textoQr);

                // Ajuste de la posicion del QR por si hay desbordamiento a la derecha
                double desbordaDerecha = posX + ancho - pagina.Width;
                if(desbordaDerecha > 0)
                {
                    posX -= desbordaDerecha + 10;
                }

                // Primero se inserta la marca de agua (si tiene contenido) para que quede debajo del todo
                string textoMarcaAgua = datosQR.MarcaAgua;

                GestionContenido gestorProceso = new GestionContenido();
                if(!string.IsNullOrEmpty(textoMarcaAgua))
                {
                    pagina = gestorProceso.InsertaMarcaAgua(pagina, gfx, datosQR);
                }

                double altoFuente = 8; // Altura aproximada del texto en puntos

                // Fuente para los textos
                XFont font = new XFont("Arial", altoFuente, XFontStyle.Bold);

                // Color a aplicar a los textos igual al del QR
                XBrush brocha = new XSolidBrush(XColor.FromArgb(colorQR.A, colorQR.R, colorQR.G, colorQR.B));

                // Primero se inserta el texto arriba del QR
                gfx.DrawString(datosQR.DatosAdicionales.TextoArriba, font, brocha, new XRect(posX, posY - altoFuente, ancho, altoFuente), XStringFormats.Center);

                // Despues se inserta el QR
                gfx.DrawImage(qrImage, posX, posY, ancho, alto);

                // Por ultimo se inserta el texto debajo del QR y centrado
                gfx.DrawString(datosQR.DatosAdicionales.TextoAbajo, font, brocha, new XRect(posX, posY + alto, ancho, altoFuente), XStringFormats.Center);

                // Libera la imagen del QR
                qrImage.Dispose();
            }

            // Captura de error si no esta diponible el programa de impresion
            catch(InvalidOperationException ex)
            {
                Logger.Agregar(ex.Message);
            }

            // Captura el error generico al insertar el QR
            catch(Exception ex)
            {
                Logger.Agregar($"Error al insertar el QR: {ex.Message}");
            }

            return documento;
        }

        // Metodo para generar la imagen del QR
        private XImage GenerarQR(string textoQr)
        {
            // Objeto para almacenar el código QR generado
            XImage qrGenerado;

            // Carga o genera el código QR
            if(_datosQR.UsarQrExterno == true)
            {
                // Si se pasa un fichero externo, se carga la imagen en el objeto QR
                qrGenerado = XImage.FromFile(_datosQR.NombreFicheroQR);
            }
            else
            {
                // En otro caso se genera el código QR a partir del texto proporcionado
                using(QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using(QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQr, QRCodeGenerator.ECCLevel.Q))
                using(QRCode qrCode = new QRCode(qrCodeData))
                using(Bitmap qrBitmap = qrCode.GetGraphic(20))
                using(var ms = new System.IO.MemoryStream())
                {
                    qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    qrGenerado = XImage.FromStream(ms);
                }

            }

            return qrGenerado;
        }
    }
}

