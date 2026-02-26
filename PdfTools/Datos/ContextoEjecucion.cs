using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfTools.Logica;

namespace PdfTools.Datos
{
    public class ContextoEjecucion
    {
        // Ruta del PDF sobre el que se está trabajando actualmente
        public string PdfActual { get; set; }

        // Ruta del ejecutable de SumatraPDF
        public string RutaVisorPdf { get; set; }

        // Carpeta de cache de SumatraPDF
        public string CacheVisorPdf { get; set; }

        // Indica si hay que esperar al cierre del visor
        public bool EsperarCierreVisor { get; set; }

        // Parametros generales del proceso
        public ConfiguracionGeneral Parametros { get; set; }

        // Datos de configuración del QR
        public ConfiguracionQR DatosQR { get; set; }

        // Datos de configuración de las acciones
        public ConfiguracionAcciones Acciones { get; set; }

        // Gestor reutilizable para la unión de PDFs
        public UnirPDFs GestorFusion { get; set; }

    }
}
