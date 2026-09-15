namespace FRFront.Models
{
    public class ConfiguracionTiendaDto
    {
        public string NombreTienda { get; set; } = "F & R";
        public string LogoUrl { get; set; } = "/images/logo-fr.png";
        public string ColorFondo { get; set; } = "#FFFFFF";
        
        // Contactos
        public string TelefonoContactanos { get; set; } = "3564-451239";
        public string EmailContacto { get; set; } = "f&r_tienda_online@gmail.com";
        public string Ubicación { get; set; } = "Sunchales, Santa Fe";
        public string Instagram { get; set; } = "@F&R.OFICIAL";

        // Medios de pago
        public string MediosPago { get; set; } = "EFECTIVO, TARJETA DE DEBITO, TARJETA DE CREDITO, TRANSFERENCIA";

        // Tipografía
        public string TipografiaSeleccionada { get; set; } = "Roboto";

        // Categorías y Menú
        public string CategoriasMenu { get; set; } = "BUZOS, CAMISAS, SWEATER, REMERAS, PANTALONES";
        public string MenuEncabezado { get; set; } = "MIS COMPRAS, LANZAMIENTOS, HOMBRE, MUJER, OFERTAS, PRODUCTOS";

        // Banner Promocional
        public string Linea1 { get; set; } = "F&R F&R F&R F&R F&R F&R F&R F&R F&R";
        public string Linea2 { get; set; } = "F&R F&R F&R F&R F&R F&R F&R F&R F&R";
        public string Linea3 { get; set; } = "ENVÍOS A TODO EL PAIS - OFERTAS!";
        public string Linea4 { get; set; } = "¡¡ SALE !!";
    }
}