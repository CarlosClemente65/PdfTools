using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PdfTools.Datos
{
    public class ConfiguracionGeneral
    {
        public string PdfEntrada { get; set; }
        public string PdfSalida { get; set; }
        public string RutaFicheros { get; set; } = Directory.GetCurrentDirectory();
        public string FicheroSalida { get; set; } // Fichero de control para gestionar cuando termina el programa.
        public string[] ListaArchivos { get; set; } // Lista de archivos para procesar si se pasa una carpeta


        // Texto de la marca de agua
        public string MarcaAgua { get; set; }

    }
}
