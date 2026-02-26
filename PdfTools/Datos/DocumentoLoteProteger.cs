using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfTools.Datos
{
    public class DocumentoLoteProteger : IDocumentoLote
    {
        public string NombreBase { get; set; }
        public string RutaPdf { get; set; }
        public string RutaGuion { get; set; }
        public string PassworApertura { get; set; }
        public string PasswordEdicion { get; set; }
        public bool EsValido
        {
            get
            {
                return !string.IsNullOrEmpty(RutaPdf) && !string.IsNullOrEmpty(RutaGuion);
            }
        }
    }
}
