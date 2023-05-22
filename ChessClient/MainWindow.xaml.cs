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

namespace ChessClient
{
    public partial class MainWindow : Window, ServiceChess.IServiceChessCallback
    {
        bool isConnected = false;
        ServiceChess.ServiceChessClient client;
        int ID;
        internal Board board = new Board();
        public MainWindow()
        {
            InitializeComponent();
            board.Draw(gridBoard);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new ServiceChess.ServiceChessClient(new System.ServiceModel.InstanceContext(this));
        }
        void ConnectUser()
        {
            if (!isConnected)
            {
                ID = client.Connect(tbUserName.Text);
                tbUserName.IsEnabled = false;
                btnCon.Content = "Disconnect";
                isConnected = true;
            }
        }
        void DisconnectUser()
        {
            if (isConnected)
            {
                client.Disconnect(ID);
                tbUserName.IsEnabled = true;
                btnCon.Content = "Connect";
                isConnected = false;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
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
        public void MsgCallback(string msg)
        {
            lbChat.Items.Add(msg);
            lbChat.ScrollIntoView(lbChat.Items[lbChat.Items.Count-1]);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisconnectUser();
        }

        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && client != null) 
            {
                client.SendMsg(tbMessage.Text, ID);
                tbMessage.Text = string.Empty;
            }
        }
    }
}
