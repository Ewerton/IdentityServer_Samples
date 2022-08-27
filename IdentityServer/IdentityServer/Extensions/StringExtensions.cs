namespace PrefeituraBrasil.IdentityServer.Extensions
{
    public static class StringExtensions
    {
        public static string SoNumeros(this string str)
        {
            string soNumeros = new String(str.Where(Char.IsDigit).ToArray());
            return soNumeros;
        }
    }
}
