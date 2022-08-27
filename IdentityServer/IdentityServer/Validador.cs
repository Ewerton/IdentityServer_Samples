using System.Text.RegularExpressions;

namespace PrefeituraBrasil.IdentityServer
{
    public static class Validador
    {
        public static class CPF
        {
            public static bool Valido(string cpf)
            {
                if (string.IsNullOrWhiteSpace(cpf))
                    return false;

                int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
                string tempCpf;
                string digito;
                int soma;
                int resto;
                cpf = cpf.Trim();
                cpf = cpf.Replace(".", "").Replace("-", "");

                if (cpf.Equals("00000000000") ||
                  cpf.Equals("11111111111") ||
                  cpf.Equals("22222222222") ||
                  cpf.Equals("33333333333") ||
                  cpf.Equals("44444444444") ||
                  cpf.Equals("55555555555") ||
                  cpf.Equals("66666666666") ||
                  cpf.Equals("77777777777") ||
                  cpf.Equals("88888888888") ||
                  cpf.Equals("99999999999"))
                {
                    return false;
                }


                if (cpf.Length != 11)
                    return false;
                tempCpf = cpf.Substring(0, 9);
                soma = 0;

                for (int i = 0; i < 9; i++)
                    soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
                resto = soma % 11;
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = resto.ToString();
                tempCpf = tempCpf + digito;
                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
                resto = soma % 11;
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;
                digito = digito + resto.ToString();
                return cpf.EndsWith(digito);
            }
        }

        public static class CNPJ
        {
            public static bool Valido(string cnpj)
            {
                if (String.IsNullOrWhiteSpace(cnpj))
                    return false;

                int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
                int soma;
                int resto;
                string digito;
                string tempCnpj;

                cnpj = cnpj.Trim();
                cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");

                if (cnpj.Length != 14)
                    return false;

                tempCnpj = cnpj.Substring(0, 12);

                soma = 0;
                for (int i = 0; i < 12; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

                resto = (soma % 11);
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;

                digito = resto.ToString();

                tempCnpj = tempCnpj + digito;
                soma = 0;
                for (int i = 0; i < 13; i++)
                    soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

                resto = (soma % 11);
                if (resto < 2)
                    resto = 0;
                else
                    resto = 11 - resto;

                digito = digito + resto.ToString();

                return cnpj.EndsWith(digito);
            }
        }

        public static class Email
        {
            public static bool Valido(string email)
            {
                if (String.IsNullOrWhiteSpace(email))
                    return false;

                return Regex.IsMatch(email, @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$");
            }
        }

        public static class CEP
        {
            public static bool Valido(string cep)
            {
                if (String.IsNullOrWhiteSpace(cep))
                    return false;
                return Regex.IsMatch(cep, ("[0-9]{5}-[0-9]{3}"));
            }
        }

    }
}
