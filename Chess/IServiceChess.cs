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
        int Connect(string name); //подключение к матчу
        [OperationContract]
        void Disconnect(int id); //отключение
        [OperationContract(IsOneWay = true)]
        void SendMsg(string msg, int id); //отправить сообщение
        [OperationContract(IsOneWay = true)] //не нужна обработка от сервера
        void Move(int x, char y);
    }
    public interface IServerChessCallback //интерфейс действий сервера
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(string msg); //рассылка сообщений пользователям
    }
}
