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
        public static bool? UsarQrExterno = false; // Indica si se usa un fichero de QR externo
        public static bool? InsertarQR = false; // Control para incluir o no el QR en el PDF


        // Datos para generar el QR
        public static string NombreFicheroQR { get; set; }

        // Datos base de la URL para generar el QR
        public static string UrlPruebasBase { get; set; } = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/";
        public static string UrlProduccionBase { get; set; } = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/";
        public static string UrlEnvio { get; set; } // URL completa con parámetros


        // Datos de control para utilizar el entorno de pruebas o producción y el uso de VeriFactu
        public static bool EntornoProduccion { get; set; } = true; // Defecto entorno producción
        public static bool VeriFactu { get;  set; } = false; // Defecto sistema no VeriFactu


        // Datos de la factura que se insertarán en el QR
        public static string NifEmisor { get; set; }
        public static string NumeroFactura { get; set; }
        public static DateTime FechaFactura { get; set; }
        public static decimal TotalFactura { get; set; }


        // Texto adiconal a insertar en el QR
        public static string TextoArriba { get; set; } = "QR Tributario";
        public static string TextoAbajo { get; set; } = "";


        // Posición tamaño y color del QR
        public static double PosX { get; set; } = 10;
        public static double PosY { get; set; } = 10;
        public static double Ancho { get; set; } = 30;
        public static double Alto { get; set; } = 30;
        public static string ColorQR { get; set; } = "#000000"; // Por defecto negro


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

        public static IdiomasQR IdiomaQR { get; set; } = IdiomasQR.es;
    }
}
