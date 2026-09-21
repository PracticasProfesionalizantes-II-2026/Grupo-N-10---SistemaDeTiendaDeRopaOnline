namespace FRFront.Models
{
    public class UsuarioSimpleDto
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;

        public int ResolveId()
        {
            return Id > 0 ? Id : IdUsuario;
        }
    }
}
