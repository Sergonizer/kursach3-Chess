using ChessClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Chess
{
    [ServiceContract(CallbackContract = typeof(IServerChessCallback))] //даём понять что у нас есть интерфейс действий сервера
    public interface IServiceChess //интерфейс действий пользователя
    {
        [OperationContract]
        Get Connect(string name); //подключение к партии
        [OperationContract]
        void Disconnect(int id); //отключение
        [OperationContract(IsOneWay = true)] //не нужна обработка от сервера
        void SendMsg(string msg, int id, int to); //отправить сообщение
        [OperationContract(IsOneWay = true)]
        void Move(int id, int x1, int y1, int x2, int y2, int promote); //ход (от первого игрока)
        [OperationContract(IsOneWay = true)]
        void Surrender(int id, int val); //сдача (от первого игрока)
        [OperationContract(IsOneWay = true)]
        void Ready(int id); //готовность
        [OperationContract(IsOneWay = true)]
        void Draw(int id); //готовность к ничьей
        [OperationContract(IsOneWay = true)]
        void CancelDraw(int id); //убрать состояние ничьей
        [OperationContract(IsOneWay = true)]
        void UpdateColor(int id, PieceColor color); //у игрока поменялся цвет
    }
    public interface IServerChessCallback //интерфейс действий сервера
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(string msg); //рассылка сообщений пользователям
        [OperationContract(IsOneWay = true)]
        void ChangeColor(PieceColor color); //смена цвета при переподключении к лобби
        [OperationContract(IsOneWay = true)]
        void Start(); //начало
        [OperationContract(IsOneWay = true)]
        void DrawOffer(); //предложить ничью
        [OperationContract(Name = "DrawUser", IsOneWay = true)]
        void Draw(); //ничья
        [OperationContract(Name = "MoveUser", IsOneWay = true)]
        void Move(int x1, int y1, int x2, int y2, int promote); //ход (второму игроку)
        [OperationContract(Name = "SurrenderUser", IsOneWay = true)]
        void Surrender(int val); //сдача (второму игроку)
    }
}
