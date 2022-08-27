namespace IdentityServer.Models
{
    public class UsuarioConfiguracao
    {
        public Guid MunicipioId { get; set; }
        public Guid EntidadeId { get; set; }
        public Perfil Perfil { get; set; }
        public int ServidorId { get; set; }
    }
}
