namespace PrefeituraBrasil.Shared.Extensions
{
    public static class decimalExtension : System.Object
    {
        public static decimal AndarCasas(this decimal numero, int casas)
        {
            for (var i = 0; i < casas; i++)
            {
                numero /= 10;
            }
            return numero;
        }
    }
}
