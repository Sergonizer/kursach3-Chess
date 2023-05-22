using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Chess
{
    // ПРИМЕЧАНИЕ. Можно использовать команду "Переименовать" в меню "Рефакторинг", чтобы изменить имя интерфейса "IServiceChess" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IServerChessCallback))]
    public interface IServiceChess
    {
        [OperationContract]
        int Connect(string name);
        [OperationContract]
        void Disconnect(int id);
        [OperationContract(IsOneWay = true)]
        void SendMsg(string msg, int id);
        [OperationContract(IsOneWay = true)]
        void Move(int x, char y);
    }
    public interface IServerChessCallback
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(string msg);
    }
}
