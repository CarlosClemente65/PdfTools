using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfTools.Datos
{
    public class ConfiguracionQR
    {
        // Datos de control para generar el QR
        public bool? UsarQrExterno = false; // Indica si se usa un fichero de QR externo
        public bool? InsertarQR = false; // Control para incluir o no el QR en el PDF

        // Datos para generar el QR
        public string NombreFicheroQR { get; set; }


        public Posicion Posicion = null;

        public DatosFactura DatosFactura;

        public DatosAdicionales DatosAdicionales = null;

        public DatosUrl DatosUrl;


    }

    public class Posicion
    {
        // Posición tamaño y color del QR
        public double PosX { get; set; } = 10;
        public double PosY { get; set; } = 10;
        public double Ancho { get; set; } = 30;
        public double Alto { get; set; } = 30;
        public string ColorQR { get; set; } = "#000000"; // Por defecto negro
    }

    public class DatosFactura
    {
        // Datos de la factura que se insertarán en el QR
        public string NifEmisor { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal TotalFactura { get; set; }

    }

    public class DatosAdicionales
    {
        // Texto adiconal a insertar en el QR
        public string TextoAbajo { get; set; } = "";

        public string TextoArriba { get; set; } = "QR Tributario";
    }

    public class DatosUrl
    {
        // Datos base de la URL para generar el QR
        public string UrlPruebasBase { get; set; } = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/";
        public string UrlProduccionBase { get; set; } = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/";
        public string UrlEnvio { get; set; } // URL completa con parámetros

        public bool EntornoProduccion { get; set; } = true; // Defecto entorno producción
        public bool VeriFactu { get; set; } = false; // Defecto sistema no VeriFactu

        public static IdiomasQR IdiomaQR { get; set; } = IdiomasQR.es;
    }

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
}
