using System;
using System.Text;

namespace regiondeployer
{
    public static class NameGenerator
    {
        public static string CreateRandomName(string prependAge, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int totalLength = prependAge.Length + length;
            char[] selected = new char[totalLength];
            Random random = new Random();

            for (int index = 0; index < length; index++)
            {
                int randomNum = random.Next(chars.Length);
                char nextChar = chars[randomNum];
                selected[index] = nextChar;
            }

            return new string(selected);
        }
    }
}