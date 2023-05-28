using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Chess
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] //все пользователи подключаются к одной сессии
    public class ServiceChess : IServiceChess
    {
        List<ServerUser> users = new List<ServerUser>(); //создаём список пользователей
        int nextId = 1; //переменная для создания id пользователей
        public int Connect(string name)
        {
            ServerUser user = new ServerUser() //создаём нового польщователя и задаём его данные
            {
                ID = nextId,
                Name = name,
                OperationContext = OperationContext.Current
            };
            nextId++;

            SendMsg(": <" + user.Name + "> зашёл в игру", 0); //отправляем сообщение
            users.Add(user); //добавляем пользователя в список
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(i=>i.ID == id); //находим пользователя
            if (user != null)
            {
                users.Remove(user); //отключпем пользователя
                SendMsg(": <" + user.Name + "> вышел из игры", 0); //отправляем сообшение об этом
            }
        }

        public void SendMsg(string msg, int id) //отправление сообщение
        {
            foreach(var item in users) //проходим по всем пользователям
            {
                string answer = DateTime.Now.ToShortTimeString();

                var user = users.FirstOrDefault(i=> i.ID == id); //находим нужного пользователя
                if (user != null)
                {
                    answer += ": <" + user.Name + ">: ";
                }
                answer += msg;

                item.OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(answer); //отправляем само сообщение
            }
        }

        public void Move(int x, char y) //функия хода
        {
            throw new NotImplementedException();
        }
    }
}
