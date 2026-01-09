using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfTools.Datos;
using PdfTools.Logica;

namespace PdfTools.Metodos
{
    public class InsertarMarcaAgua
    {
        public PdfDocument InsertaMarcaAgua(PdfDocument documento, ConfiguracionGeneral parametros, ConfiguracionQR datosQR)
        {
            try
            {
                // Carga en el documento el PDF de entrada
                documento = Utilidades.Generardocumento(parametros.PdfEntrada);

                // Establece la pagina 1 para insertar el QR y las imagenes
                PdfPage pagina = documento.Pages[0];

                // Añade el recuadro a la pagina
                XGraphics gfx = XGraphics.FromPdfPage(pagina);

                // Inserta la marca de agua en el PDF
                Utilidades.InsertaMarcaAgua(pagina, gfx, datosQR.MarcaAgua);
            }
            catch(Exception ex)
            {
                Logger.Agregar($"Se ha producido un error al insertar la marca de agua.\n{ex.Message}");
            }

            return documento;
        }
    }
}
