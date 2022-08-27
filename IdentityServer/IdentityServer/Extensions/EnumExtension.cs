using System.ComponentModel;

namespace PrefeituraBrasil.Shared.Extensions
{
    public class EnumExtension
    {
        public static string StaticGetDescriptionEnum(Enum value)
        {
            if (value is null)
            {
                return "";
            }

            return GetDescription(value);
        }

        public string GetDescriptionEnum(Enum value)
        {
            if (value is null)
            {
                return "";
            }

            return GetDescription(value);
        }

        public static string GetDescription(Enum input)
        {
            Type type = input.GetType();
            System.Reflection.MemberInfo[] memInfo = type.GetMember(input.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = (object[])memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return input.ToString();
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
    }
}
