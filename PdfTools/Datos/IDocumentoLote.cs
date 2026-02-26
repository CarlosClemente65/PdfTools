using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfTools.Datos
{
    public interface IDocumentoLote
    {
        string NombreBase { get; set; }

        string RutaPdf { get; set; }

        // Ruta del guion
        string RutaGuion { get; set; }

        // Control de si tiene todos los ficheros minimos
        bool EsValido { get; }
    }
}
