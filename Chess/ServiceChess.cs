using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Chess
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceChess : IServiceChess
    {
        List<ServerUser> users = new List<ServerUser>();
        int nextId = 1;
        public int Connect(string name)
        {
            ServerUser user = new ServerUser()
            {
                ID = nextId,
                Name = name,
                OperationContext = OperationContext.Current
            };
            nextId++;

            SendMsg(": <" + user.Name + "> зашёл в игру", 0);
            users.Add(user);
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = users.FirstOrDefault(i=>i.ID == id);
            if (user != null)
            {
                users.Remove(user);
                SendMsg(": <" + user.Name + "> вышел из игры", 0);
            }
        }

        public void SendMsg(string msg, int id)
        {
            foreach(var item in users) 
            {
                string answer = DateTime.Now.ToShortTimeString();

                var user = users.FirstOrDefault(i=> i.ID == id);
                if (user != null)
                {
                    answer += ": <" + user.Name + ">: ";
                }
                answer += msg;

                item.OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(answer);
            }
        }

        public void Move(int x, char y)
        {
            throw new NotImplementedException();
        }
    }
}
