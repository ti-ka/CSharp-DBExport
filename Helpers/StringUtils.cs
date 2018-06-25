using System.Collections.Generic;


namespace Utils.DbExport
{
    public static class StringUtils
    {
        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                char c;

#if HAVE_CHAR_TO_STRING_WITH_CULTURE
                c = char.ToLower(chars[i], CultureInfo.InvariantCulture);
#else
                c = char.ToLowerInvariant(chars[i]);
#endif
                chars[i] = c;
            }

            return new string(chars);
        }

        public static string ToPlural(this string s)
        {
            if (s.EndsWith("es"))
            {
                return s;
            }

            if (s.EndsWith("s"))
            {
                return s + "es";
            }

            if (s.EndsWith("y"))
            {
                return s.Substring(0, s.Length - 1) + "ies";
            }

            return s + "s";
        }

        public static string ToSingular(this string s)
        {
            if (s.EndsWith("ies"))
            {
                return s.Substring(0, s.Length - 3) + "y";
            }

            if (s.EndsWith("es"))
            {
                return s.Substring(0, s.Length - 1);
            }

            if (s.EndsWith("s"))
            {
                return s.Substring(0, s.Length - 1);
            }


            return s;

        }




        public static string FormatWith(this string input, Dictionary<string, string> fields)
        {
            foreach (var f in fields)
            {
                input = input.Replace("{" + f.Key + "}", f.Value);
            }
            return input;
        }

    }
}