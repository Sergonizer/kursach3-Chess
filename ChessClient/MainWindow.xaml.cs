using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ChessClient.Board;

namespace ChessClient
{
    public struct Get { public int id; public PieceColor color; };
    public partial class MainWindow : Window, ServiceChess.IServiceChessCallback
    {
        bool isConnected = false;
        ServiceChess.ServiceChessClient client;
        int ID;
        internal Board board;
        public MainWindow()
        {
            InitializeComponent();
        }
        public void Enable(bool val) //включить доску и кнопки сдачи и ничьей
        {
            board.SetEnabled(val);
            btnSurr.IsEnabled = val;
            btnDraw.IsEnabled = val;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) //при загрузке окна надо создать доску и клиента
        {
            ChangeColor(PieceColor.White);
            board.Draw(this);
            Enable(false);
            client = new ServiceChess.ServiceChessClient(new System.ServiceModel.InstanceContext(this));
        }
        void ConnectUser() //подключиться и получить данные о пользователе с сервера
        {
            if (!isConnected)
            {
                ServiceChess.Get g = client.Connect(tbUserName.Text);
                ID = g.id;
                ChangeColor(g.color);
                board.Draw(this);
                tbUserName.IsEnabled = false;
                btnCon.Content = "Disconnect";
                isConnected = true;
                client.Ready(ID);
            }
        }
        void DisconnectUser() //отключиться и заблокировать доску
        {
            if (isConnected)
            {
                client.Disconnect(ID);
                tbUserName.IsEnabled = true;
                btnCon.Content = "Connect";
                isConnected = false;
                Enable(false);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e) //подключение и отключение по нажатию на кнопку
        {
            if (isConnected)
            {
                DisconnectUser();
            }
            else
            {
                ConnectUser();
            }
        }
        public void MsgCallback(string msg) //принять сообщение
        {
            lbChat.Items.Add(msg);
            lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count - 1]);
        }
        public void ChangeColor(PieceColor color) //сменить цвет (и пересоздать доску)
        {
            board = new Board(color);
            board.SetClient(client, ID);
            Turn.Source = new BitmapImage(new Uri("pack://application:,,,/resources/whitequeen.png"));
        }
        public void ChangeColor(ServiceChess.PieceColor color)
        {
            ChangeColor((PieceColor)color);
            client.UpdateColor(ID, color);
        }
        void ServiceChess.IServiceChessCallback.MoveUser(int x1, int y1, int x2, int y2, int promote) //принять ход с сервера
        {
            board.Move(board.GetBoard()[x1][y1], board.GetBoard()[x2][y2], false, promote);
        }

        void ServiceChess.IServiceChessCallback.SurrenderUser(int val) //принять сдачу от другого игрока
        {
            if (btnSurr.IsEnabled == true)
            {
                string text = board.GetUser() == PieceColor.White ? "1 - 0" : "0 - 1";
                lbMoves.Items.Add(text);
                if (val == 1)
                    MessageBox.Show("Соперник сдался! Игра окончена!");
                else if (val == 2)
                    MessageBox.Show("Соперник вышел! Игра окончена!");
            }
            Enable(false);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //отключиться при закрытии окна
        {
            DisconnectUser();
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e) //отправить сообщение в чат
        {
            if (e.Key == Key.Enter && client != null)
            {
                client.SendMsg(tbMessage.Text, ID, 0);
                tbMessage.Text = string.Empty;
            }
        }

        void ServiceChess.IServiceChessCallback.Start() //начать игру
        {
            ChangeColor(board.GetUser());
            board.Draw(this);
            Enable(true);
        }

        private void Surrender(object sender, RoutedEventArgs e) //сдаться
        {
            Enable(false);
            client.Surrender(ID, 1);
        }

        private void Draw(object sender, RoutedEventArgs e) //отправить предложение о ничьей
        {
            client.Draw(ID);
        }

        void ServiceChess.IServiceChessCallback.DrawOffer()
        {
            MessageBox.Show("Соперник предложил ничью!");
        }

        void ServiceChess.IServiceChessCallback.DrawUser()
        {
            string text = "0.5 - 0.5 | 0.5 - 0.5";
            lbMoves.Items.Add(text);
            MessageBox.Show("Ничья! Игра окончена!");
            Enable(false);
        }
    }
}
