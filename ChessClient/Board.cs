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

namespace ChessClient
{
    class cell : RadioButton
    {
        Pos pos;
        int[] size = new int[2];
        Piece piece = null;
        bool isChecked = false;

        public cell(int x, int y)
        {
            pos = new Pos(x, y);
            this.Style = FindResource(typeof(ToggleButton)) as Style;
        }
        public bool ChangeCheckState()
        {
            isChecked = !isChecked;
            return isChecked;
        }
        public void ChangeCheckState(bool state)
        {
            isChecked = state;
        }
        public void SetSize(int x_, int y_)
        {
            size[0] = x_;
            size[1] = y_;
        }
        public void SetPiece(Piece piece_)
        {
            piece = piece_;
            if (piece_ == null)
                ((Grid)Content).Children.Clear();
            else
            {
                if (piece_.GetType() == typeof(Pawn))
                    ;
                System.Windows.Controls.Image dotImage = new System.Windows.Controls.Image();
                dotImage.Source = piece_.Image().Source;
                ((Grid)Content).Children.Add(dotImage);
            }
        }
        public Piece GetPiece()
        {
            return piece;
        }
        public Pos GetPos()
        {
            return pos;
        }
    }
    internal class Board
    {
        Grid GridBoard;
        BitmapImage bmi = new BitmapImage(new Uri("pack://application:,,,/resources/dot.png"));
        cell[][] board = new cell[8][];
        public cell[] prev = new cell[2];
        System.Windows.Controls.Image[] promote_white = new System.Windows.Controls.Image[4] {
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
        PieceColor turn = PieceColor.White;
        static private System.Windows.Controls.Image ToImage(BitmapImage img)
        {
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = img;
            return image;
        }
        public enum CheckStaleMate
        {
            None,
            Checkmate,
            Stalemate
        }
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            bool state = ((cell)sender).ChangeCheckState();
            if (!state)
                Cell_Deselect(sender, e);
            if (state && ((cell)sender).GetPiece() != null)
                foreach (Pos pos in ((cell)sender).GetPiece().PossibleMoves())
                {
                    System.Windows.Controls.Image dotImage = new System.Windows.Controls.Image();
                    dotImage.Source = bmi;
                    ((Grid)board[pos.x][pos.y].Content).Children.Add(dotImage);
                }
        }
        private void Cell_Deselect(object sender, RoutedEventArgs e)
        {
            ((cell)sender).ChangeCheckState(false);
            if (((cell)sender).GetPiece() != null)
                foreach (Pos pos in ((cell)sender).GetPiece().PossibleMoves())
                {
                    if (((Grid)board[pos.x][pos.y].Content).Children.Count > 0)
                        ((Grid)board[pos.x][pos.y].Content).Children.RemoveAt(((Grid)board[pos.x][pos.y].Content).Children.Count - 1);
                }
            var new_cell = GridBoard.Children.OfType<cell>().FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            if (new_cell != null && ((cell)sender).GetPiece() != null)
            {
                if (((cell)sender).GetPiece().PossibleMoves().Contains(new_cell.GetPos()))
                {
                    Capture captured = ((cell)sender).GetPiece().MovePiece(board, new_cell.GetPos());
                    if (new_cell.GetPiece().GetType() == typeof(Pawn) && new_cell.GetPos().y == (new_cell.GetPiece().GetPieceColor() == PieceColor.White ? 0 : 7))
                    {
                        Piece[] pieces = new Piece[4];
                        pieces[0] = new Queen(this, new_cell.GetPos().x, new_cell.GetPos().y, new_cell.GetPiece().GetPieceColor());
                        pieces[1] = new Rook(this, new_cell.GetPos().x, new_cell.GetPos().y, new_cell.GetPiece().GetPieceColor());
                        pieces[2] = new Bishop(this, new_cell.GetPos().x, new_cell.GetPos().y, new_cell.GetPiece().GetPieceColor());
                        pieces[3] = new Knight(this, new_cell.GetPos().x, new_cell.GetPos().y, new_cell.GetPiece().GetPieceColor());
                        Rect rect = new Rect(new_cell.PointToScreen(new System.Windows.Point(0, 0)), new_cell.PointToScreen(new System.Windows.Point(new_cell.ActualWidth, new_cell.ActualHeight)));
                        Promote promote = new Promote(rect, new_cell.GetPiece().GetPieceColor() == PieceColor.White ? promote_white : promote_black);
                        promote.ShowDialog();

                        new_cell.SetPiece(null);
                        new_cell.SetPiece(pieces[promote.id]);
                    }
                    PieceColor opposite = (new_cell.GetPiece().GetPieceColor() == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (IsCheck(new_cell.GetPiece().GetPieceColor()))
                        FindKing(opposite).Background = System.Windows.Media.Brushes.Red;
                    cell king = FindKing(new_cell.GetPiece().GetPieceColor());
                    if (king.Background == System.Windows.Media.Brushes.Red)
                        if (!IsCheck(opposite))
                            king.Background = (king.GetPos().x + king.GetPos().y) % 2 == 1 ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
                    if (king == new_cell && ((cell)sender).Background == System.Windows.Media.Brushes.Red)
                        if (!IsCheck(opposite))
                            ((cell)sender).Background = (((cell)sender).GetPos().x + ((cell)sender).GetPos().y) % 2 == 1 ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
                    CheckStaleMate checkStaleMate = IsMate(new_cell.GetPiece().GetPieceColor());
                    if (checkStaleMate == CheckStaleMate.Checkmate)
                        MessageBox.Show((new_cell.GetPiece().GetPieceColor() == PieceColor.White ? "Белые" : "Чёрные") + " поставили мат! Игра окончена!");
                    else if (checkStaleMate == CheckStaleMate.Stalemate)
                        MessageBox.Show("Пат! Игра окончена!");
                    SwapTurn();
                }
            }
        }
        public cell FindKing(PieceColor col)
        {
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (board[x][y].GetPiece() != null && board[x][y].GetPiece().GetPieceColor() == col && board[x][y].GetPiece().GetType() == typeof(King))
                        return board[x][y];
            return null;
        }
        public CheckStaleMate IsMate(PieceColor by)
        {
            bool check = IsCheck(by);
            bool nomoves = true;
            SwapTurn();
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
            SwapTurn();
            if (nomoves && check)
                return CheckStaleMate.Checkmate;
            else if (nomoves && !check)
                return CheckStaleMate.Stalemate;
            else
                return CheckStaleMate.None;
        }
        public bool IsCheck(PieceColor by)
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
        public cell[][] GetBoard()
        {
            return board;
        }
        public PieceColor GetTurn()
        {
            return turn;
        }
        public void SwapTurn()
        {
            turn = (turn == PieceColor.White ? PieceColor.Black : PieceColor.White);
        }
        public Board()
        {
            for (int i = 0; i < 8; i++)
            {
                board[i] = new cell[8];
            }
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    board[x][y] = new cell(x, y);
                    board[x][y].Content = new Grid();
                    board[x][y].AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(Cell_Click));
                    board[x][y].AddHandler(RadioButton.UncheckedEvent, new RoutedEventHandler(Cell_Deselect));
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
        public void Draw(Grid gridBoard)
        {
            GridBoard = gridBoard;
            double w = gridBoard.Width, h = gridBoard.Height;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    board[i][j].SetSize(System.Convert.ToInt32(Math.Round(w / 8)), System.Convert.ToInt32(Math.Round(h / 8)));
                    Grid.SetRow(board[i][j], j);
                    Grid.SetColumn(board[i][j], i);
                    gridBoard.Children.Add(board[i][j]);
                }
            }
        }
    }
}
