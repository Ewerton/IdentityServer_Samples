using System.ComponentModel;

namespace PrefeituraBrasil.Shared.Extensions
{
    public static class ListEnumExtension
    {
        public static IList<SelectOption> GetOptions<T>()
        {
            var type = typeof(T);
            var result = Enum.GetNames(type).Select(
                item => new SelectOption
                {
                    Id = Enum.Parse(type, item),
                    Descricao = GetDescription<T>(item)
                }).ToList();

            return result;

        }

        public static string GetDescription<T>(string enumValue)
        {
            var descriptioAttribute = typeof(T)
                .GetField(enumValue)
                .GetCustomAttributes(
                    typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return descriptioAttribute != null ? descriptioAttribute.Description : enumValue;
        }
    }
}
