using ChessClient;
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
        List<ServerUser[]> users = new List<ServerUser[]>(); //создаём список пользователей
        ServerUser[] curr = new ServerUser[2];
        public int nextId = 1; //переменная для создания id пользователей

        public Get Connect(string name)
        {
            int c = 0;
            int sessid = nextId;
            if (sessid % 2 == 1)
            {
                Random r = new Random();
                c = r.Next(2);
            }
            else
                c = 1 - c;
            ServerUser user = new ServerUser() //создаём нового пользователя и задаём его данные
            {
                ID = nextId,
                Name = name.Length == 0 ? "Player " + nextId.ToString() : name,
                Color = c == 0 ? PieceColor.White : PieceColor.Black,
                OperationContext = OperationContext.Current
            };
            nextId++;
            if (curr[(sessid - 1) % 2] != null)
                SendMsg(": <" + user.Name + "> зашёл в игру", 0, curr[(sessid - 1) % 2].ID); //отправляем сообщение
            curr[(sessid) % 2] = user; //добавляем пользователя в список
            if (curr[0] != null && curr[1] != null)
            {
                users.Add((ServerUser[])curr.Clone());
                curr[0] = null;
                curr[1] = null;
            }
            Get get;
            get.color = user.Color;
            get.id = user.ID;
            return get;
        }

        public void Disconnect(int id)
        {
            foreach (var sess in users)
            {
                for (int i = 0; i < 2; i++)
                {
                    var user = sess[i];
                    if (user.ID == id)
                    {
                        if (user != null)
                        {
                            SendMsg(": <" + user.Name + "> вышел из игры", 0, sess[1-i].ID); //отправляем сообщение об этом
                            sess[i] = null;
                        }
                    }
                }
                if (sess[0] == null && sess[1] == null)
                    users.Remove(sess);
            }
        }

        public void SendMsg(string msg, int id, int to = 0) //отправление сообщения
        {
            if (to != 0)
            {
                foreach (var sess in users) //проходим по всем пользователям
                {
                    if (sess[0].ID == to)
                    {
                        sess[0].OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(msg); //отправляем само сообщение
                    }
                    if (sess[1].ID == to)
                    {
                        sess[1].OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(msg); //отправляем само сообщение
                    }
                }
                return;
            }
            //foreach(var item in users) //проходим по всем пользователям
            //{
            //    string answer = DateTime.Now.ToShortTimeString();

            //    var user = users.FirstOrDefault(i=> i.ID == id); //находим нужного пользователя
            //    if (user != null)
            //    {
            //        answer += ": <" + user.Name + ">: ";
            //    }
            //    answer += msg;

            //    item.OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(answer); //отправляем само сообщение
            //}
        }

        public void Move(int x, char y) //функция хода
        {
            throw new NotImplementedException();
        }
    }
}
