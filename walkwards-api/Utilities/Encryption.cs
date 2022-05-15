using System;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using walkwards_api.UserManager;

namespace walkwards_api.Utilities
{
    public static class Encryption
    {
        private static readonly string Defaultkey = "gabrysia";

        public static string EncryptedData(string data, string key = "")
        {
            if (key == "") key = Defaultkey;
            List<char> newData = new List<char>();
            char[] keyCharArray = key.Replace(" ", string.Empty).ToCharArray();

            char[] dataChar = data.ToCharArray();
            
            foreach (var item in dataChar)
            {
                bool added = false;
                for (int i = 0; i < keyCharArray.Length; i++)
                {
                    if(item == keyCharArray[i])
                    {
                        if ((i % 2 == 0 || i == 0) && i != keyCharArray.Length - 1)
                        {
                            newData.Add(keyCharArray[i + 1]);
                            added = true;
                        }
                        else
                        {
                            newData.Add(keyCharArray[i - 1]);
                            added = true;
                        }
                        break;
                    }
                }

                if (!added) newData.Add(item);
            }
            
            string result = "";
            foreach (var item in newData)
            {
                result += item.ToString();
            }

            return result;
        }
    }
}