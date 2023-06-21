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
            using(var host = new ServiceHost(typeof(Chess.ServiceChess))) //запускаем хост
            {
                host.Open(); //открываем
                Console.WriteLine("Хост активен!");
                Console.ReadLine(); //чтобы закрыть хост достаточно нажать Enter
            }
        }
    }
}
