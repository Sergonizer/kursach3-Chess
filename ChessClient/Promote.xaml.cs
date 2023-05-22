using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessClient
{
    public partial class Promote : Window
    {
        public int id = 0;
        private Image[] prom = new Image[4];
        void Cell_Click(object sender, RoutedEventArgs e)
        {
            id = ((cell)sender).GetPos().x;
            DialogResult = true;
        }
        void Cell_Hover(object sender, RoutedEventArgs e)
        {
            prom[((cell)sender).GetPos().x].Opacity = 1;
        }
        void Cell_Unhover(object sender, RoutedEventArgs e)
        {
            prom[((cell)sender).GetPos().x].Opacity = 0.5;
        }
        public Promote(Rect rect, Image[] prom_)
        {
            Left = rect.X - rect.Width * 1.5;
            Top = rect.Y - rect.Height * 0.5;
            InitializeComponent();
            for (int i = 0; i < 4; i++)
            {
                prom[i] = new Image();
                prom[i].Source = prom_[i].Source;
                prom[i].Opacity = 0.5;
            }
            cell[] curr = new cell[4];
            for (int i = 0; i < 4; i++)
            {
                curr[i] = new cell(i, 0);
                curr[i].SetSize(System.Convert.ToInt32(Math.Round(gridBoard.Width / 4)), System.Convert.ToInt32(Math.Round(gridBoard.Height)));
                curr[i].AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Cell_Click));
                curr[i].AddHandler(ButtonBase.MouseEnterEvent, new RoutedEventHandler(Cell_Hover));
                curr[i].AddHandler(ButtonBase.MouseLeaveEvent, new RoutedEventHandler(Cell_Unhover));
                Grid.SetColumn(curr[i], i);
                gridBoard.Children.Add(curr[i]);
                curr[i].Content = new Grid();
                ((Grid)curr[i].Content).Children.Add(prom[i]);
            }
        }
    }
}
