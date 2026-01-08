using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfTools.Datos
{
    public class ConfiguracionQR
    {
        public bool? UsarQrExterno = false; // Indica si se usa un fichero de QR externo

        public bool? InsertarQR = false; // Control para incluir o no el QR en el PDF
        public string MarcaAgua { get; set; } // Texto de la marca de agua en caso de que se use
        public string NombreFicheroQR { get; set; } // Fichero de imagen del QR externo


        // Propiedades privadas para instancias internas
        private DatosFactura _datosFactura;
        private DatosAdicionales _datosAdicionales;
        private DatosUrl _datosUrl;
        private Posicion _posicion;

        // Datos para generar el QR
        // Datos de la factura para generar el QR
        public DatosFactura DatosFactura
        {
            get
            {
                if (_datosFactura == null)
                {
                    _datosFactura = new DatosFactura();
                }
                return _datosFactura;
            }
        }

        // Datos adicionales para el QR
        public DatosAdicionales DatosAdicionales
        {
            get
            {
                if (_datosAdicionales == null)
                {
                    _datosAdicionales = new DatosAdicionales();
                }
                return _datosAdicionales;
            }
        }

        // Datos de la URL para generar el QR
        public DatosUrl DatosUrl
        {
            get
            {
                if (_datosUrl == null)
                {
                    _datosUrl = new DatosUrl();
                }
                return _datosUrl;
            }
        }

        // Posición del QR en el PDF
        public Posicion Posicion
        {
            get
            {
                if (_posicion == null)
                {
                    _posicion = new Posicion();
                }
                return _posicion;
            }
        }

    }

    public class DatosUrl
    {
        // Datos base de la URL para generar el QR
        public string UrlPruebasBase { get; set; } = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/";
        public string UrlProduccionBase { get; set; } = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/";
        public string UrlEnvio { get; set; } // URL completa con parámetros

        public bool EntornoProduccion { get; set; } = true; // Defecto entorno producción
        public bool VeriFactu { get; set; } = false; // Defecto sistema no VeriFactu

        public Enums.IdiomasQR IdiomaQR { get; set; } = Enums.IdiomasQR.es;
    }

    public class Posicion
    {
        // Posición, tamaño y color del QR
        public double PosX { get; set; } = 10; // Posicion desde la izquierda
        public double PosY { get; set; } = 10; // Posición desde la parte superior
        public double Ancho { get; set; } = 30; // Ancho en mm
        public double Alto { get; set; } = 30; // Alto en mm
        public string ColorQR { get; set; } = "#000000"; // Color del QR en formato hexadecimal (defeto negro)
    }

    public class DatosFactura
    {
        // Datos de la factura que se insertarán en el QR
        public string NifEmisor { get; set; } // Nif del emisor de la factura
        public string NumeroFactura { get; set; } // Numero de la factura
        public DateTime FechaFactura { get; set; } // Fecha de la factura
        public decimal TotalFactura { get; set; } // Importe total de la factura

    }

    public class DatosAdicionales
    {
        // Texto adiconal a insertar en el QR
        public string TextoAbajo { get; set; } = "";

        public string TextoArriba { get; set; } = "QR Tributario";
    }
}
