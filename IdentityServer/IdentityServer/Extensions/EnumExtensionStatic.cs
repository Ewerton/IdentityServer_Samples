using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PrefeituraBrasil.Shared.Extensions
{
    public static class EnumExtensionStatic
    {
        public static string GetDescription(this Enum e)
        {
            try
            {
                if (e == null)
                    return string.Empty;

                string strRetorno = string.Empty;

                if (Enum.IsDefined(e.GetType(), e))
                {
                    var attribute = e.GetType()
                                       .GetTypeInfo()
                                       .GetMember(e.ToString())
                                       .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                                       ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                       .SingleOrDefault()
                                       as DescriptionAttribute;

                    strRetorno = attribute?.Description ?? e.ToString();
                }

                return strRetorno;
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        public static string GetDisplayName(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.Name;
        }
    }
}
