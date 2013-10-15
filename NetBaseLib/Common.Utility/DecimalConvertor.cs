using System;
using System.Collections.Generic;

namespace Common.Utility
{
    public class DecimalConvertor
    {
        private const int DecimalSystem36 = 36;
        private const int DecimalSystem10 = 10;

        private const int AAsciiCode = 65;
        private const int ZAAsciiCode = 90;

        private const int ZeroAsciiCode = 48;
        private const int NineAsciiCode = 57;

        public static string Decimal10To36(int input)
        {
            if (input == 0)
            {
                return "0";
            }

            IList<int> resultList = new List<int>();

            int tempResult = input;

            while (tempResult != 0)
            {
                resultList.Insert(0, tempResult % DecimalSystem36);
                tempResult = tempResult / DecimalSystem36;
            }

            string resultString = string.Empty;

            foreach (int i in resultList)
            {
                if (i < DecimalSystem10)
                {
                    resultString += i.ToString();
                }
                else
                {
                    resultString += (char)(AAsciiCode + i - DecimalSystem10);
                }
            }

            return resultString;
        }

        public static int Decimal36To10(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input invaild.");
            }

            char[] inputChars = input.ToCharArray();
            int inputLength = inputChars.Length;

            int result = 0;

            for (int i = 0; i < inputLength; i++)
            {
                int power = inputLength - i - 1;
                int tempResult = Decimal36To10(inputChars[i]);

                result += tempResult * (int)Math.Pow(DecimalSystem36, power);
            }

            return result;
        }

        private static int Decimal36To10(char input)
        {
            int inputInt = input;

            if (inputInt >= ZeroAsciiCode && inputInt <= NineAsciiCode)
            {
                return inputInt - ZeroAsciiCode;
            }
            if (inputInt >= AAsciiCode && inputInt <= ZAAsciiCode)
            {
                return inputInt - AAsciiCode + DecimalSystem10;
            }
            throw new ArgumentException("Input invaild.");
        }

        public static int Pow(string decimal36Pow, string decimal36Index)
        {
            int index = Decimal36To10(decimal36Index);
            return Pow(decimal36Pow, index);
        }

        public static int Pow(string decimal36Pow, int decimal10Index)
        {
            int pow = Decimal36To10(decimal36Pow);
            return (int)Math.Pow(pow, decimal10Index);
        }
    }
}
