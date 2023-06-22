using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.CodeDom;

namespace ChessClient
{
    public class cell : RadioButton //класс клетки
    {
        Pos pos; //позиция клетки
        int[] size = new int[2]; //размер клетки
        Piece piece = null; //фигура в клетке
        bool isChecked = false; //выделена ли клетка

        public cell(int x, int y)
        {
            pos = new Pos(x, y);
            this.Style = FindResource(typeof(ToggleButton)) as Style;
        }
        public override string ToString()
        {
            string c;
            if (piece.GetType() == typeof(Pawn))
                c = "";
            else if (piece.GetType() == typeof(King))
                c = "K";
            else if (piece.GetType() == typeof(Queen))
                c = "Q";
            else if (piece.GetType() == typeof(Rook))
                c = "R";
            else if (piece.GetType() == typeof(Knight))
                c = "N";
            else
                c = "B";
            switch (pos.x)
            {
                case 0:
                    return c + "a" + (8 - pos.y).ToString();
                case 1:
                    return c + "b" + (8 - pos.y).ToString();
                case 2:
                    return c + "c" + (8 - pos.y).ToString();
                case 3:
                    return c + "d" + (8 - pos.y).ToString();
                case 4:
                    return c + "e" + (8 - pos.y).ToString();
                case 5:
                    return c + "f" + (8 - pos.y).ToString();
                case 6:
                    return c + "g" + (8 - pos.y).ToString();
            }
            return c + "h" + (8 - pos.y).ToString();
        }
        public void SetCheckState(bool state) //поменять состояние выделения
        {
            isChecked = state;
        }
        public bool GetCheckState()
        {
            return isChecked;
        }
        public void SetSize(int x_, int y_) //поменять размер клетки
        {
            size[0] = x_;
            size[1] = y_;
        }
        public void SetPiece(Piece piece_) //поставить фигуру на клетку
        {
            piece = piece_;
            if (piece_ == null)
                ((Grid)Content).Children.Clear();
            else
            {
                System.Windows.Controls.Image dotImage = new System.Windows.Controls.Image();
                dotImage.Source = piece_.Image().Source;
                ((Grid)Content).Children.Add(dotImage);
            }
        }
        public Piece GetPiece() //получить фигуру
        {
            return piece;
        }
        public Pos GetPos() //узнать позицию
        {
            return pos;
        }
    }
    public class Board //класс доски
    {
        private PieceColor UserColor;
        private ServiceChess.ServiceChessClient Client; //храним клиента для отправки запросов на сервер
        private int ID;
        cell[][] board = new cell[8][];
        MainWindow Window;
        BitmapImage bmi = new BitmapImage(new Uri("pack://application:,,,/resources/dot.png")); //точка для возможного хода
        bool moved = false; //был ли совершен ход после последнего выделения клетки
        public cell[] prev = new cell[2]; //хранение предыдущего хода для взятия на проходе
        System.Windows.Controls.Image[] promote_white = new System.Windows.Controls.Image[4] { //для показа окошка для выбора фигуры заместо пешки
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/whitequeen.png"))),
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/whiterook.png"))),
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/whitebishop.png"))),
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/whiteknight.png")))
        };
        System.Windows.Controls.Image[] promote_black = new System.Windows.Controls.Image[4] {
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/blackqueen.png"))),
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/blackrook.png"))),
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/blackbishop.png"))),
            ToImage(new BitmapImage(new Uri("pack://application:,,,/resources/blackknight.png")))
        };
        PieceColor turn = PieceColor.White; //первый ход у белых
        static private System.Windows.Controls.Image ToImage(BitmapImage img) //преобразование из одного класса изображения в другой
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = img;
            return image;
        }
        public enum CheckStaleMate //перечисление состояний короля (ничего, мат, пат)
        {
            None,
            Checkmate,
            Stalemate
        }
        private void Cell_Click(object sender, RoutedEventArgs e) //если клетку выделили второй раз, то убрать выделение. Если сделан ход, то убрать выделение
        {
            bool state = ((cell)sender).GetCheckState();
            if (state || moved)
            {
                ((cell)sender).IsChecked = false;
                return;
            }
            if (((cell)sender).GetPiece() != null && GetTurn() == UserColor)
                foreach (Pos pos in ((cell)sender).GetPiece().PossibleMoves())
                {
                    System.Windows.Controls.Image dotImage = new System.Windows.Controls.Image();
                    dotImage.Source = bmi;
                    ((Grid)board[pos.x][pos.y].Content).Children.Add(dotImage);
                }
            ((cell)sender).SetCheckState(true);
        }
        private void Cell_Uncheck(object sender, RoutedEventArgs e) //убрать возможные ходы после отмены выделения клетки
        {
            ((cell)sender).SetCheckState(false);
            var new_cell = Window.gridBoard.Children.OfType<cell>().FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            if (((cell)sender).GetPiece() != null)
            {
                foreach (Pos pos in ((cell)sender).GetPiece().PossibleMoves())
                {
                    for (int i = ((Grid)board[pos.x][pos.y].Content).Children.Count - 1; i >= 0; i--)
                        if (((Grid)board[pos.x][pos.y].Content).Children[i].GetValue(System.Windows.Controls.Image.SourceProperty) == bmi)
                            ((Grid)board[pos.x][pos.y].Content).Children.RemoveAt(i);
                }
                moved = Move((cell)sender, new_cell);
            }
        }
        public void SetEnabled(bool val) //включить/выключить клетку
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                    board[i][j].IsEnabled = val;
            }
        }
        public bool Move(cell start, cell end, bool thisplayer = true, int prom = -1) //подвинуть фигуру и отправить в список ходов
        {
            bool ret = false;
            if (thisplayer && start.GetPiece().GetPieceColor() != UserColor)
                return ret;
            if (end != null && start.GetPiece() != null)
            {
                if (start.GetPiece().PossibleMoves().Contains(end.GetPos()))
                {
                    Client.CancelDraw(ID); //отменить предложения о ничьей
                    string move = "";
                    if (start.GetPiece().GetType() == typeof(King) && Math.Abs(end.GetPos().x - start.GetPos().x) == 2) //рокировка
                    {
                        if (end.GetPos().x - start.GetPos().x == 2)
                            move = "0-0";
                        if (end.GetPos().x - start.GetPos().x == -2)
                            move = "0-0-0";
                    }
                    else
                    {
                        move = start.ToString() + " - " + end.GetPos().ToString(); //остальные ходы записываются как начальная и конечная позиции
                    }
                    if (start.GetPiece().GetPieceColor() == PieceColor.White)
                    {
                        Window.lbMoves.Items.Add((Window.lbMoves.Items.Count + 1).ToString() + ". " + move); //номер хода
                    }
                    else
                    {
                        Window.lbMoves.Items[Window.lbMoves.Items.Count - 1] = Window.lbMoves.Items[Window.lbMoves.Items.Count - 1].ToString() + " | " + move; //отобразить ход черных
                    }
                    Capture captured = start.GetPiece().MovePiece(board, end.GetPos());
                    if (captured.piece != null)
                    {
                        string str = (string)Window.lbMoves.Items[Window.lbMoves.Items.Count - 1];
                        int i = str.LastIndexOf("-");
                        if (i != -1)
                            Window.lbMoves.Items[Window.lbMoves.Items.Count - 1] = str.Remove(i, 1).Insert(i, ":"); //взятие фигуры обозначается двоеточием вместо тире
                    }
                    ret = true;
                    if (end.GetPiece().GetType() == typeof(Pawn) && end.GetPos().y == (end.GetPiece().GetPieceColor() == PieceColor.White ? 0 : 7)) //повышение пешки до другой фигуры
                    {
                        Piece[] pieces = new Piece[4];
                        pieces[0] = new Queen(this, end.GetPos().x, end.GetPos().y, end.GetPiece().GetPieceColor());
                        pieces[1] = new Rook(this, end.GetPos().x, end.GetPos().y, end.GetPiece().GetPieceColor());
                        pieces[2] = new Bishop(this, end.GetPos().x, end.GetPos().y, end.GetPiece().GetPieceColor());
                        pieces[3] = new Knight(this, end.GetPos().x, end.GetPos().y, end.GetPiece().GetPieceColor());
                        if (prom == -1)
                        {
                            Rect rect = new Rect(end.PointToScreen(new System.Windows.Point(0, 0)), end.PointToScreen(new System.Windows.Point(end.ActualWidth, end.ActualHeight)));
                            Promote promote = new Promote(rect, end.GetPiece().GetPieceColor() == PieceColor.White ? promote_white : promote_black);
                            promote.ShowDialog();
                            end.SetPiece(null);
                            end.SetPiece(pieces[promote.id]);
                            prom = promote.id;
                        }
                        else
                        {
                            end.SetPiece(null);
                            end.SetPiece(pieces[prom]);
                        }
                        Window.lbMoves.Items[Window.lbMoves.Items.Count - 1] = Window.lbMoves.Items[Window.lbMoves.Items.Count - 1].ToString() + end.ToString()[0]; //добавить в конце хода имя фигуры на которую поменяли пешку
                    }
                    PieceColor opposite = (end.GetPiece().GetPieceColor() == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (IsCheck(end.GetPiece().GetPieceColor())) //если шах, то поменять фон клетки короля
                    {
                        FindKing(opposite).Background = System.Windows.Media.Brushes.Red;
                        Window.lbMoves.Items[Window.lbMoves.Items.Count - 1] = Window.lbMoves.Items[Window.lbMoves.Items.Count - 1].ToString() + "+"; //шах обозначается плюсом в конце
                    }
                    cell king = FindKing(end.GetPiece().GetPieceColor());
                    if (king.Background == System.Windows.Media.Brushes.Red) //поменять фон короля, который больше не под шахом
                        if (!IsCheck(opposite))
                            king.Background = (king.GetPos().x + king.GetPos().y) % 2 == 1 ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
                    if (king == end && (start).Background == System.Windows.Media.Brushes.Red)
                        if (!IsCheck(opposite))
                            (start).Background = ((start).GetPos().x + (start).GetPos().y) % 2 == 1 ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
                    Client.Move(ID, (start).GetPos().x, (start).GetPos().y, end.GetPos().x, end.GetPos().y, prom);
                    SwapTurn();
                    Window.Turn.Source = GetTurn() == PieceColor.White ? new BitmapImage(new Uri("pack://application:,,,/resources/whitequeen.png")) : //поменять изображение текущего хода
                        new BitmapImage(new Uri("pack://application:,,,/resources/blackqueen.png"));
                    if (GetTurn() != end.GetPiece().GetPieceColor())
                    {
                        CheckStaleMate checkStaleMate = IsMate(end.GetPiece().GetPieceColor());
                        if (checkStaleMate == CheckStaleMate.Checkmate) //мат
                        {
                            string str = (string)Window.lbMoves.Items[Window.lbMoves.Items.Count - 1];
                            int i = str.LastIndexOf("+");
                            if (i != -1)
                                Window.lbMoves.Items[Window.lbMoves.Items.Count - 1] = str.Remove(i, 1).Insert(i, "x"); //мат обозначается крестиком в конце
                            string text = end.GetPiece().GetPieceColor() == PieceColor.White ? "1 - 0" : "0 - 1";
                            Window.lbMoves.Items.Add(text);
                            SwapTurn();
                            MessageBox.Show((end.GetPiece().GetPieceColor() == PieceColor.White ? "Белые" : "Чёрные") + " поставили мат! Игра окончена!");
                            Window.Enable(false);
                        }
                        else if (checkStaleMate == CheckStaleMate.Stalemate) //пат
                        {
                            string text = "0.5 - 0.5";
                            Window.lbMoves.Items.Add(text);
                            SwapTurn();
                            MessageBox.Show("Пат! Игра окончена!");
                            Window.Enable(false);
                        }
                    }
                }
            }
            return ret;
        }
        public cell FindKing(PieceColor col) //находим короля
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (board[x][y].GetPiece() != null && board[x][y].GetPiece().GetPieceColor() == col && board[x][y].GetPiece().GetType() == typeof(King))
                        return board[x][y];
            return null;
        }
        public CheckStaleMate IsMate(PieceColor by) //проверка на мат
        {
            bool check = IsCheck(by);
            bool nomoves = true;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board[x][y].GetPiece() == null)
                        continue;
                    if (board[x][y].GetPiece().GetPieceColor() != by)
                    {
                        if (board[x][y].GetPiece().PossibleMoves().Count() > 0)
                            nomoves = false;
                    }
                    if (!nomoves)
                        break;
                }
                if (!nomoves)
                    break;
            }
            if (nomoves && check)
                return CheckStaleMate.Checkmate;
            else if (nomoves && !check)
                return CheckStaleMate.Stalemate;
            else
                return CheckStaleMate.None;
        }
        public bool IsCheck(PieceColor by) //проверка на шах
        {
            bool swapped = false;
            if (GetTurn() != by)
            {
                SwapTurn();
                swapped = true;
            }
            bool check = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board[x][y].GetPiece() == null)
                        continue;
                    if (board[x][y].GetPiece().GetPieceColor() == by)
                    {
                        PieceColor opposite = (by == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                        cell king = FindKing(opposite);
                        if (board[x][y].GetPiece().PossibleMoves(true).Contains(king.GetPos()))
                        {
                            check = true;
                        }
                    }
                }
            }
            if (swapped)
                SwapTurn();
            return check;
        }
        public cell[][] GetBoard() //получить доску
        {
            return board;
        }
        public PieceColor GetTurn() //получить текущий ход
        {
            return turn;
        }
        public PieceColor GetUser() //получить цвет игрока
        {
            return UserColor;
        }
        public void SetUser(PieceColor val) //присвоить цвет игрока
        {
            UserColor = val;
        }
        public void SwapTurn() //поменять ход
        {
            turn = (turn == PieceColor.White ? PieceColor.Black : PieceColor.White);
        }
        public void SetClient(ServiceChess.ServiceChessClient client, int id) //изменить клиента и айди
        {
            Client = client;
            ID = id;
        }
        public Board(PieceColor Color) //расставить фигуры на доске
        {
            UserColor = Color;
            for (int i = 0; i < 8; i++)
            {
                board[i] = new cell[8];
            }
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    board[x][y] = new cell(x, y);
                    board[x][y].IsEnabled = false;
                    board[x][y].Content = new Grid();
                    board[x][y].AddHandler(RadioButton.ClickEvent, new RoutedEventHandler(Cell_Click));
                    board[x][y].AddHandler(RadioButton.UncheckedEvent, new RoutedEventHandler(Cell_Uncheck));
                    board[x][y].Background = (x + y) % 2 == 1 ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
                    if (y == 1)
                    {
                        board[x][y].SetPiece(new Pawn(this, x, y, PieceColor.Black));
                    }
                    if (y == 6)
                    {
                        board[x][y].SetPiece(new Pawn(this, x, y, PieceColor.White));
                    }
                }
            }
            board[0][0].SetPiece(new Rook(this, 0, 0, PieceColor.Black));
            board[1][0].SetPiece(new Knight(this, 1, 0, PieceColor.Black));
            board[2][0].SetPiece(new Bishop(this, 2, 0, PieceColor.Black));
            board[3][0].SetPiece(new Queen(this, 3, 0, PieceColor.Black));
            board[4][0].SetPiece(new King(this, 4, 0, PieceColor.Black));
            board[5][0].SetPiece(new Bishop(this, 5, 0, PieceColor.Black));
            board[6][0].SetPiece(new Knight(this, 6, 0, PieceColor.Black));
            board[7][0].SetPiece(new Rook(this, 7, 0, PieceColor.Black));

            board[0][7].SetPiece(new Rook(this, 0, 7, PieceColor.White));
            board[1][7].SetPiece(new Knight(this, 1, 7, PieceColor.White));
            board[2][7].SetPiece(new Bishop(this, 2, 7, PieceColor.White));
            board[3][7].SetPiece(new Queen(this, 3, 7, PieceColor.White));
            board[4][7].SetPiece(new King(this, 4, 7, PieceColor.White));
            board[5][7].SetPiece(new Bishop(this, 5, 7, PieceColor.White));
            board[6][7].SetPiece(new Knight(this, 6, 7, PieceColor.White));
            board[7][7].SetPiece(new Rook(this, 7, 7, PieceColor.White));
        }
        public void Draw(MainWindow window) //отрисовка доски
        {
            Window = window;
            Window.lbMoves.Items.Clear();
            double w = Window.gridBoard.Width, h = Window.gridBoard.Height;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (UserColor == PieceColor.White)
                    {
                        board[i][j].SetSize(System.Convert.ToInt32(Math.Round(w / 8)), System.Convert.ToInt32(Math.Round(h / 8)));
                        Grid.SetRow(board[i][j], j);
                        Grid.SetColumn(board[i][j], i);
                        Window.gridBoard.Children.Add(board[i][j]);
                    }
                    else
                    {
                        board[i][j].SetSize(System.Convert.ToInt32(Math.Round(w / 8)), System.Convert.ToInt32(Math.Round(h / 8)));
                        Grid.SetRow(board[i][j], 7 - j);
                        Grid.SetColumn(board[i][j], 7 - i);
                        Window.gridBoard.Children.Add(board[i][j]);
                    }
                }
            }
        }
    }
}
