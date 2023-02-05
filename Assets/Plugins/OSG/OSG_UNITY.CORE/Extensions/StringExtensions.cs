using System.Text;
using UnityEngine;

namespace OSG
{
    public static class StringExtensions
    {
        public static string InColor(this string text, Color color)
        {
            StringBuilder sb = new StringBuilder(text.Length + 25);
            sb.Append("<color=#");
            sb.Append(ColorUtility.ToHtmlStringRGBA(color));
            sb.Append('>');
            sb.Append(text);
            sb.Append("</color>");

            return sb.ToString();
        }

        public static string InColor(this string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }

        public static string InMagenta(this string text)
        {
            return text.InColor(Color.magenta);
        }

        public static string InRed(this string text)
        {
            return text.InColor(Color.red);
        }

        public static string InGreen(this string text)
        {
            return text.InColor(Color.green);
        }

        public static string InCyan(this string text)
        {
            return text.InColor(Color.cyan);
        }

        public static string InOrange(this string text)
        {
            return text.InColor(new Color(1, 0.5f, 0));
        }

        public static string InYellow(this string text)
        {
            return text.InColor(Color.yellow);
        }

        public static string InBlue(this string text)
        {
            return text.InColor(Color.blue);
        }


        public static string InSize(this string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }

        public static string InSizeRatio(this string text, float ratio)
        {
            return $"<size={(int) (ratio * 100)}%>{text}</size>";
        }

        public static string InBold(this string text)
        {
            return $"<b>{text}</b>";
        }

        /// <summary>
        /// for textmeshpro (http://digitalnativestudios.com/textmeshpro/docs/rich-text/#pos))
        /// set caret position
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static string AtPos(this string text, float ratio)
        {
            return $"<pos={(int) (ratio * 100)}%>{text}";
        }

        public static string InItalic(this string text)
        {
            return $"<i>{text}</i>";
        }

        /// <summary>
        /// Puts the string into the Clipboard.
        /// </summary>
        /// <param name="str"></param>
        public static void CopyToClipboard(this string str)
        {
            var te = new TextEditor {text = str};
            te.SelectAll();
            te.Copy();
        }

        /// <summary>
        /// Adds spaces before groups of uppercase or numbers
        /// replaces _ with space
        /// "WHAT_EverItIs100" => "WHAT Ever It Is 100"
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static string BeautifyName(this string label)
        {
            int i = 0;
            while (label[i] == '_')
            {
                if (i == label.Length)
                    return "";
                i++;
            }

            char[] replace = new char [label.Length * 2];
            int newLength = 0;
            bool wasUpper = true;
            bool wasNumber = true;

            while(i < label.Length)
            {
                char c = label[i++];
                if (c == '_')
                {
                    wasUpper = false;
                    wasNumber = false;
                    replace[newLength++] = ' ';
                    continue;
                }

                if (c >= 'A' && c <= 'Z')
                {
                    if (!wasUpper)
                        replace[newLength++] = ' ';
                    wasUpper = true;
                    wasNumber = false;
                }
                else if (c >= '0' && c <= '9')
                {
                    if (!wasNumber)
                        replace[newLength++] = ' ';
                    wasNumber = true;
                    wasUpper = false;
                }
                else
                {
                    wasUpper = false;
                    wasNumber = false;
                }

                replace[newLength++] = c;
            }

            return new string(replace, 0, newLength);
        }

        /// <summary>
        /// Capitalizes the first character of the input string.
        /// </summary>
        public static string Capitalize(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            char c = text[0];

            if (char.IsUpper(c))
                return text;

            char[] a = text.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        /// <summary>
        /// Capitalizes the first character of the input string and inserts spaces in-between collapsed words.
        /// </summary>
        public static string NiceName(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Remove ll first non-letter characters
            while (!char.IsLetter(text[0]))
                text = text.Substring(1);

            // Replace all underscores by spaces
            text = text.Replace("_", " ");

            // Capitalize first letter
            text = text.Capitalize();

            int length = text.Length;
            for (int i = 0; i < length - 2; i++)
            {
                char nextChar = text[i + 1];

                // This character is a space and next one is lowercase
                if (text[i] == ' ' && char.IsLower(nextChar))
                {
                    string upperChar = $"{nextChar}".ToUpper();
                    Debug.Log(upperChar);
                    text = text.Remove(i + 1, 1);
                    text = text.Insert(i + 1, upperChar);
                }

                // "i + 2" character is lowercase and next one is uppercase
                if (char.IsLower(text[i + 2]) && char.IsUpper(nextChar))
                {
                    text = text.Insert(i + 1, " ");
                    i++;
                }
            }

            return text;
        }

        /// <summary>
        /// Replaces all invalid characters with underscores.
        /// </summary>
        public static string CleanName(this string text, params char[] keepCharacters)
        {
            int length = text.Length;
            for (int i = 0; i < length; i++)
            {
                char c = text[i];

                bool exception = false;
                for (int param = 0; param < keepCharacters.Length; param++)
                {
                    if (c == keepCharacters[param])
                    {
                        exception = true;
                        break;
                    }
                }

                if (exception)
                    continue;

                if (!char.IsLetterOrDigit(c))
                {
                    text = text.Remove(i, 1);
                    text = text.Insert(i, "_");
                }
            }

            return text;
        }
    }
}