namespace PdfTools.Datos
{
    public class Enums
    {
        /* Idioma de respuesta de la AEAT a QR con VeriFctu
            gl: gallego
            ca: catalán
            eu: euskera
            es: castellano
            va: valenciano
            en: inglés
        */
        public enum IdiomasQR
        {
            gl,
            ca,
            eu,
            es,
            va,
            en
        }

        // Control para saber el tipo de parametro del guion
        public enum tiposParametros
        {
            QR,
            General,
            Acciones,
            Desconocido
        }

        // Acciones de proceso que se pueden realizar 
        public enum AccionesProceso
        {
            InsertarQR,
            InsertarLoteQR,
            Unir,
            InsertarMarca,
            Imprimir,
            Abrir,
            Visualizar,
            CerrarVisor,
            Proteger,
            ProtegerLote
        }
    }
}
