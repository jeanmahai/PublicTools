using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignSimulate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("please input a sign datetime");
            var input = Console.ReadLine();
            DateTime signDate;
            if(DateTime.TryParse(input ,out signDate))
            {
                SignHelper.Sign(signDate);
            }
            Console.ReadKey();
        }
    }
}
