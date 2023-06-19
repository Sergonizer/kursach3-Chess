using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using ChessClient;

namespace Chess
{
    public class ServerUser //класс пользователя
    {
        public int ID { get; set; } //айди
        public string Name { get; set; } // имя
        public bool Ready { get; set; } //готовность
        public bool Draw { get; set; } //готов к ничьей
        public PieceColor Color { get; set; } //цвет фигуры
        public OperationContext OperationContext { get; set; } //сведения о подключении пользователя
    }
}
