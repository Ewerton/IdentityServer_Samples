/// TODO: Avaliar se Perfil deveria ficar aqui ou na Global.
/// Se iremos utilizar um mecanismo de Policies / PolicyGroups / Roles para gerenciar permissões talvez seja uma boa ideia centralizar esta gestão no IdentityServer
/// Vou deixar temporariamente aqui mas teremos que discutir isso em algum momento.

namespace IdentityServer.Models
{
    public enum Perfil
    {
        Servidor = 1,
        Estagiario = 2,
        Advogado = 3,
        Cedido = 4,
        AdministradorEntidade = 5,
        AdministradorMunicipal = 6,
        AdministradorSiapro = 7,
        Suporte = 8,
    }
}
