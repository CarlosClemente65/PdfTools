using System.Collections.Generic;
using System.Windows.Documents;

namespace PdfTools.Datos
{
    // Clase para control de las acciones a realizar
    public class ConfiguracionAcciones
    {
        // Lista de acciones a realizar
        public List<Enums.AccionesProceso> AccionesProceso { get; set; }

        // Control para si es necesario abrir el visor
        public bool AbrirVisor {  get; set; }

        // Control para cerrar el visor
        public bool CerrarVisor { get; set; }

        public ConfiguracionAcciones()
        {
            AbrirVisor = false;
            CerrarVisor = false;
            AccionesProceso = new List<Enums.AccionesProceso>();
        }
    }
}
