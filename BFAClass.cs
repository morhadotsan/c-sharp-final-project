using System.Collections.Generic;
using System.IO;

namespace WpfApp1
{
    internal class BFAClass
    {
        private static readonly object lockObject = new object(); // Lock for thread safety

        // Function to generate combinations of characters of given length
        public static void GenerateCombinations(string prefix, int length, string charset, List<string> combinations)
        {
            if (length == 0)
            {
                combinations.Add(prefix);
                return;
            }

            foreach (char c in charset)
            {
                GenerateCombinations(prefix + c, length - 1, charset, combinations);
            }
        }

        // Function to search for a given string in the generated combinations
        public static bool SearchInCombinations(string target, int length, string pword, string psalt)
        {
            string charset = VarClass.ProgCharset();
            List<string> combinations = new List<string>();
            GenerateCombinations("", length, charset, combinations);

            foreach (string combination in combinations)
            {
                string loopComb = combination.PadRight(8, '0');
                string decryptedText = AesClass.Decrypt(target, pword, psalt);

                if (decryptedText != "decryption_failed")
                {
                    lock (lockObject)
                    {
                        File.WriteAllText("decrypted_text.txt", decryptedText);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}