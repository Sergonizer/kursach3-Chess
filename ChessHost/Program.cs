using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace ChessHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var host = new ServiceHost(typeof(Chess.ServiceChess))) // включаем хост
            {
                host.Open(); //открываем хост
                Console.WriteLine("Хостим");
                Console.ReadLine(); //чтобы хост не закрылся
            }
        }
    }
}
