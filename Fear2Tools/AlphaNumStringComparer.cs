using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fear2Tools
{
    public class InsensitiveAlphaNumStringComparer : IComparer<string>
    {
        private static readonly InsensitiveAlphaNumStringComparer _default = new InsensitiveAlphaNumStringComparer();
        public static InsensitiveAlphaNumStringComparer Default => _default;

        public int Compare(string value1, string value2)
        {
            string v1 = value1;
            string v2 = value2;

            int min = v1.Length > v2.Length ? v2.Length : v1.Length;
            for (int i = 0; i < min; i++)
            {
                char c1 = char.ToLower(v1[i]);
                char c2 = char.ToLower(v2[i]);

                if (c1 < c2)
                    return -1;
                else if (c1 > c2)
                    return 1;
            }
            if (v1.Length < v2.Length)
                return -1;
            else if (v1.Length > v2.Length)
                return 1;

            return 0;
        }
    }
}