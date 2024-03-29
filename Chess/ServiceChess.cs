﻿using ChessClient;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace Chess
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] //все пользователи подключаются к одной сессии
    public class ServiceChess : IServiceChess
    {
        List<List<ServerUser>> users = new List<List<ServerUser>>(); //создаём список пользователей
        List<ServerUser> curr = new List<ServerUser>(); //очередь
        public int nextId = 1; //переменная для создания id пользователей

        public Get Connect(string name)
        {
            int c = 0;
            if (curr.Count == 0)
            {
                Random r = new Random();
                c = r.Next(2);
            }
            else
            {
                if (curr[0].Color == PieceColor.White)
                    c = 1;
                else
                    c = 0;
            }
            ServerUser user = new ServerUser() //создаём нового пользователя и задаём его данные
            {
                ID = nextId,
                Name = name.Length == 0 ? "Player " + nextId.ToString() : name,
                Color = c == 0 ? PieceColor.White : PieceColor.Black,
                OperationContext = OperationContext.Current
            };
            nextId++;
            curr.Add(user); //добавляем пользователя в сессию
            if (curr.Count == 2)
            {
                users.Add(new List<ServerUser>(curr));
                for (int i = 0; i < 2; i++)
                {
                    if (curr[i] != user)
                        SendMsg(user.Name + " зашёл в игру", 0, curr[i].ID); //отправляем сообщение
                }
                curr.Clear();
            }
            Get get;
            get.color = user.Color;
            get.id = user.ID;
            return get;
        }

        public void Disconnect(int id)
        {
            GetUser(id).Ready = false;
            foreach (var user in curr)
            {
                if (user.ID == id)
                {
                    curr.Remove(user);
                    return;
                }
            }
            foreach (var sess in users.ToList())
            {
                if (sess.Count == 0)
                {
                    users.Remove(sess);
                    continue;
                }
                for (int i = 0; i < 2; i++)
                {
                    var user = sess[i];
                    if (user.ID == id)
                    {
                        if (user != null)
                        {
                            sess[1 - i].OperationContext.GetCallbackChannel<IServerChessCallback>().Surrender(2); //игрок сдался путем выхода из игры
                            SendMsg(user.Name + " вышел из игры", 0, sess[1 - i].ID); //отправляем сообщение о выходе игрока из игры
                            sess[1 - i].Ready = false;
                            int c = 0;
                            if (curr.Count == 0)
                            {
                                Random r = new Random();
                                c = r.Next(2);
                            }
                            else
                            {
                                if (curr[0].Color == PieceColor.White)
                                    c = 1;
                                else
                                    c = 0;
                            }
                            sess[1 - i].Color = c == 0 ? PieceColor.White : PieceColor.Black;
                            sess[1 - i].OperationContext.GetCallbackChannel<IServerChessCallback>().ChangeColor(sess[1 - i].Color);
                            sess[1 - i].Ready = true;
                            curr.Add(sess[1 - i]); //перекинуть оставшегося игрока в очередь
                            users.Remove(sess);
                            sess.Clear();
                            if (curr.Count == 2) //начать игру если в очереди уже кто-то был
                            {
                                SendMsg(curr[1].Name + " зашёл в игру", 0, curr[0].ID); //отправляем сообщение о входе
                                curr[1].Color = c == 0 ? PieceColor.White : PieceColor.Black;
                                curr[1].OperationContext.GetCallbackChannel<IServerChessCallback>().ChangeColor(curr[1].Color);
                                var opp = curr[0];
                                if (opp != null && opp.Ready)
                                {
                                    curr[0].OperationContext.GetCallbackChannel<IServerChessCallback>().Start();
                                    curr[1].OperationContext.GetCallbackChannel<IServerChessCallback>().Start();
                                }
                                users.Add(new List<ServerUser>(curr));
                                curr.Clear();
                            }
                        }
                    }
                    if (sess.Count == 0)
                        return;
                }
            }
        }
        ServerUser GetUser(int id)
        {
            if (curr.Count == 1) //если в очереди есть человек, то это может оказаться нужный
            {
                var user = curr[0];
                if (user != null)
                {
                    if (user.ID == id)
                    {
                        return user;
                    }
                }
            }
            if (curr.Count == 2) //если в очереди два человека но она еще не добавлена в список [не должно вызываться вообще]
            {
                for (int i = 0; i < 2; i++)
                {
                    var user = curr[i];
                    if (user != null)
                    {
                        if (user.ID == id)
                        {
                            return user;
                        }
                    }
                }
            }
            foreach (var sess in users) //проходим по всем сессиям
            {
                for (int i = 0; i < 2; i++) //находим игрока
                {
                    var user = sess[i];
                    if (user != null)
                    {
                        if (user.ID == id)
                        {
                            return user;
                        }
                    }
                }
            }
            return null;
        }
        ServerUser GetOpponent(int id)
        {
            if (curr.Count == 2) //если в очереди два человека но она еще не добавлена в список [не должно вызываться вообще]
            {
                for (int i = 0; i < 2; i++)
                {
                    var user = curr[i];
                    if (user != null)
                    {
                        if (user.ID == id)
                        {
                            return curr[1-i];
                        }
                    }
                }
            }
            foreach (var sess in users) //проходим по всем сессиям
            {
                for (int i = 0; i < 2; i++) //находим игрока
                {
                    var user = sess[i];
                    if (user != null)
                    {
                        if (user.ID == id)
                        {
                            return sess[1 - i];
                        }
                    }
                }
            }
            return null;
        }
        public void SendMsg(string msg, int id, int to) //отправление сообщения
        {
            var user = GetUser(id);
            string answer = DateTime.Now.ToShortTimeString() + ": ";
            if (to != 0)
            {
                var receiver = GetUser(to);
                answer += msg;

                receiver?.OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(answer); //отправляем сообщение определенному игроку
                return;
            }
            var opp = GetOpponent(id);
            if (user != null)
                answer += "<" + user.Name + ">: ";
            answer += msg;

            user?.OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(answer); //отправляем обоим
            opp?.OperationContext.GetCallbackChannel<IServerChessCallback>().MsgCallback(answer);
        }
        public void Surrender(int id, int val)
        {
            GetOpponent(id).OperationContext.GetCallbackChannel<IServerChessCallback>().Surrender(val); //отправляем сообщение о сдаче
        }

        void IServiceChess.Ready(int id)
        {
            var user = GetUser(id);
            user.Ready = true;
            var opp = GetOpponent(id);
            if (opp != null && opp.Ready)
            {
                user.OperationContext.GetCallbackChannel<IServerChessCallback>().Start();
                opp.OperationContext.GetCallbackChannel<IServerChessCallback>().Start();
            }
        }

        void IServiceChess.UpdateColor(int id, PieceColor color)
        {
            var user = GetUser(id);
            if (user != null)
                user.Color = color;
        }

        void IServiceChess.Draw(int id)
        {
            var user = GetUser(id);
            user.Draw = true;
            var opp = GetOpponent(id);
            if (opp != null && !opp.Draw)
                opp.OperationContext.GetCallbackChannel<IServerChessCallback>().DrawOffer();
            else if (opp != null && opp.Draw)
            {
                opp.OperationContext.GetCallbackChannel<IServerChessCallback>().Draw();
                user.OperationContext.GetCallbackChannel<IServerChessCallback>().Draw();
            }
        }

        void IServiceChess.CancelDraw(int id)
        {
            var user = GetUser(id);
            user.Draw = false;
        }

        void IServiceChess.Move(int id, int x1, int y1, int x2, int y2, int promote)
        {
            GetOpponent(id).OperationContext.GetCallbackChannel<IServerChessCallback>().Move(x1, y1, x2, y2, promote); //отправляем ход
        }
    }
}
