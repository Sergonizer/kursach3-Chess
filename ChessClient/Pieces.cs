using ChessClient.Properties;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace ChessClient
{
    struct Pos //стурктура позиции по координатам
    {
        public int x;
        public int y;
        public Pos(int x_, int y_)
        {
            x = x_;
            y = y_;
        }
    }
    struct Search //структура поиска
    {
        public bool add;
        public bool stop;
    }
    struct Capture //структура захвата
    {
        public Pos pos;
        public Piece piece;

        public Capture(Pos pos_, Piece piece_) //конструктор
        {
            pos = pos_;
            piece = piece_;
        }
    }
    enum PieceColor //перечисление цветов фигур
    {
        White,
        Black
    }
    abstract class Piece //класс типа фигуры
    {
        protected Board Board;
        protected PieceColor PieceColor;
        protected System.Windows.Controls.Image image = new System.Windows.Controls.Image();
        protected Pos Pos;
        protected bool moved = false;
        public abstract List<Pos> PossibleMoves(bool test = false); //возможные шаги
        public bool WasMoved() //узнаём двигалась ли фигура ранее
        {
            return moved;
        }
        public void UnMove() //изменяем это состояние 
        {
            moved = false;
        }
        public Capture MovePiece(cell[][] board, Pos pos, bool test = false) //двигаем фигуру
        {
            if (pos.x < 0 || pos.y < 0 || pos.x > 7 || pos.y > 7)
                return new Capture(pos, null);
            cell[] tmp = new cell[2];
            tmp[0] = Board.prev[0];
            tmp[1] = Board.prev[1];
            Board.prev[0] = board[Pos.x][Pos.y];
            Board.prev[1] = board[pos.x][pos.y];
            if (GetType() == typeof(King))
            {
                if (Math.Abs(pos.x - Pos.x) > 1)
                {
                    for (int x = pos.x; x >= 0 && x < 8; x += (pos.x - Pos.x) / 2)
                    {
                        if (board[x][Pos.y].GetPiece() != null && board[x][Pos.y].GetPiece().GetType() != typeof(Rook))
                            break;
                        if (board[x][Pos.y].GetPiece() != null && board[x][Pos.y].GetPiece().GetType() == typeof(Rook) && !board[x][Pos.y].GetPiece().WasMoved())
                        {
                            Pos finpos;
                            finpos.x = Pos.x + (pos.x - Pos.x) / 2;
                            finpos.y = Pos.y;
                            board[x][Pos.y].GetPiece().MovePiece(board, finpos);
                        }
                    }
                }
            }

            board[Pos.x][Pos.y].SetPiece(null);

            if (GetType() == typeof(Pawn))
            {
                if (PieceColor == PieceColor.White)
                {
                    if (pos.x != Pos.x && board[pos.x][pos.y].GetPiece() == null && board[pos.x][pos.y + 1].GetPiece() != null && board[pos.x][pos.y + 1].GetPiece().GetType() == typeof(Pawn))
                    {
                        Capture enpassanted = new Capture(new Pos(pos.x, pos.y + 1), board[pos.x][pos.y + 1].GetPiece());

                        board[pos.x][pos.y + 1].SetPiece(null);
                        board[pos.x][pos.y].SetPiece(this);
                        Pos.x = pos.x;
                        Pos.y = pos.y;
                        if (!test)
                            moved = true;
                        else
                        {
                            Board.prev[0] = tmp[0];
                            Board.prev[1] = tmp[1];
                        }
                        return enpassanted;
                    }
                }
                if (PieceColor == PieceColor.Black)
                {
                    if (pos.x != Pos.x && board[pos.x][pos.y].GetPiece() == null && board[pos.x][pos.y - 1].GetPiece() != null && board[pos.x][pos.y - 1].GetPiece().GetType() == typeof(Pawn))
                    {
                        Capture enpassanted = new Capture(new Pos(pos.x, pos.y + 1), board[pos.x][pos.y - 1].GetPiece());

                        board[pos.x][pos.y - 1].SetPiece(null);
                        board[pos.x][pos.y].SetPiece(this);
                        Pos.x = pos.x;
                        Pos.y = pos.y;
                        if (!test)
                            moved = true;
                        else
                        {
                            Board.prev[0] = tmp[0];
                            Board.prev[1] = tmp[1];
                        }
                        return enpassanted;
                    }
                }
            }
            Capture captured = new Capture(pos, board[pos.x][pos.y].GetPiece());
            board[pos.x][pos.y].SetPiece(null);
            board[pos.x][pos.y].SetPiece(this);
            Pos.x = pos.x;
            Pos.y = pos.y;
            if (!test)
                moved = true;
            else
            {
                Board.prev[0] = tmp[0];
                Board.prev[1] = tmp[1];
            }
            return captured;
        }
        public PieceColor GetPieceColor() //узнаём цвет фигуры
        {
            return PieceColor;
        }
        public System.Windows.Controls.Image Image() //узнаём картинку фигуры
        {
            return image;
        }
        protected Search ToAddPos(Pos Pos)
        {
            Search ret;
            if (Pos.x > 7 || Pos.x < 0 || Pos.y > 7 || Pos.y < 0)
            {
                ret.add = false;
                ret.stop = true;
            }
            else
            {
                var piece = Board.GetBoard()[Pos.x][Pos.y].GetPiece();
                if (piece == null)
                {
                    ret.add = true;
                    ret.stop = false;
                }
                else
                {
                    if (piece.GetPieceColor() != PieceColor)
                    {
                        ret.add = true;
                        ret.stop = true;
                    }
                    else
                    {
                        ret.add = false;
                        ret.stop = true;
                    }
                }
            }
            return ret;
        }
    }
    class Pawn : Piece //пешка производный класс от класса фигур
    {
        public Pawn(Board board, int x, int y, PieceColor pieceColor)
        {
            Pos.x = x; Pos.y = y;
            Board = board;
            PieceColor = pieceColor;
            BitmapImage bmi = new BitmapImage(new Uri(PieceColor == PieceColor.White ? "pack://application:,,,/resources/whitepawn.png" : "pack://application:,,,/resources/blackpawn.png"));
            image.Source = bmi;
        }
        protected Search PawnMove(Pos Pos)
        {
            Search ret;
            if (Pos.x > 7 || Pos.x < 0 || Pos.y > 7 || Pos.y < 0)
            {
                ret.add = false;
                ret.stop = true;
            }
            else
            {
                var piece = Board.GetBoard()[Pos.x][Pos.y].GetPiece();
                if (piece == null)
                {
                    ret.add = true;
                    ret.stop = false;
                }
                else
                {
                    ret.add = false;
                    ret.stop = true;
                }
            }
            return ret;
        }
        private bool PawnCapture(Pos Pos)
        {
            bool ret;
            if (Pos.x > 7 || Pos.x < 0 || Pos.y > 7 || Pos.y < 0)
            {
                ret = false;
            }
            else
            {
                var piece = Board.GetBoard()[Pos.x][Pos.y].GetPiece();
                if (piece == null)
                {
                    ret = false;
                }
                else
                {
                    if (piece.GetPieceColor() != PieceColor)
                    {
                        ret = true;
                    }
                    else
                    {
                        ret = false;
                    }
                }
            }
            return ret;
        }
        public override List<Pos> PossibleMoves(bool test = false)
        {
            if (PieceColor != Board.GetTurn())
                return new List<Pos>();

            List<Pos> pos = new List<Pos>();
            if (PieceColor == PieceColor.White)
            {
                Pos currpos;
                Search search;
                for (int i = 1; i < (moved ? 2 : 3); i++)
                {
                    currpos = new Pos(Pos.x, Pos.y - i);
                    search = PawnMove(currpos);
                    if (search.add)
                        pos.Add(currpos);
                    if (search.stop)
                        break;
                }
                if (Pos.y == 3 && Board.prev[0] != null && Board.prev[1] != null && Board.prev[1].GetPiece().GetType() == typeof(Pawn) && Board.prev[1].GetPiece().GetPieceColor() == PieceColor.Black && Board.prev[1].GetPos().y - Board.prev[0].GetPos().y == 2)
                {
                    if (Math.Abs(Board.prev[1].GetPos().x - Pos.x) == 1)
                        pos.Add(new Pos(Board.prev[1].GetPos().x, Pos.y - 1));
                }
                currpos = new Pos(Pos.x - 1, Pos.y - 1);
                if (PawnCapture(currpos))
                    pos.Add(currpos);
                currpos = new Pos(Pos.x + 1, Pos.y - 1);
                if (PawnCapture(currpos))
                    pos.Add(currpos);
            }
            if (PieceColor == PieceColor.Black)
            {
                Pos currpos;
                Search search;
                for (int i = 1; i < (moved ? 2 : 3); i++)
                {
                    currpos = new Pos(Pos.x, Pos.y + i);
                    search = PawnMove(currpos);
                    if (search.add)
                        pos.Add(currpos);
                    if (search.stop)
                        break;
                }
                if (Pos.y == 4 && Board.prev[0] != null && Board.prev[1] != null && Board.prev[1].GetPiece().GetType() == typeof(Pawn) && Board.prev[1].GetPiece().GetPieceColor() == PieceColor.White && Board.prev[1].GetPos().y - Board.prev[0].GetPos().y == -2)
                {
                    if (Math.Abs(Board.prev[1].GetPos().x - Pos.x) == 1)
                        pos.Add(new Pos(Board.prev[1].GetPos().x, Pos.y + 1));
                }
                currpos = new Pos(Pos.x - 1, Pos.y + 1);
                if (PawnCapture(currpos))
                    pos.Add(currpos);
                currpos = new Pos(Pos.x + 1, Pos.y + 1);
                if (PawnCapture(currpos))
                    pos.Add(currpos);
            }

            if (!test)
            {
                Pos startpos = Pos;
                foreach (Pos p in pos.ToList())
                {
                    Capture captured = MovePiece(Board.GetBoard(), p, true);
                    PieceColor opposite = (PieceColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (Board.IsCheck(opposite))
                        pos.Remove(p);
                    MovePiece(Board.GetBoard(), startpos, true);
                    Board.GetBoard()[captured.pos.x][captured.pos.y].SetPiece(captured.piece);
                }
            }
            return pos;
        }
    }
    class Rook : Piece //ладья производный класс от класса фигур
    {
        public Rook(Board board, int x, int y, PieceColor pieceColor)
        {
            Pos.x = x; Pos.y = y;
            Board = board;
            PieceColor = pieceColor;
            BitmapImage bmi = new BitmapImage(new Uri(PieceColor == PieceColor.White ? "pack://application:,,,/resources/whiterook.png" : "pack://application:,,,/resources/blackrook.png"));
            image.Source = bmi;
        }
        private bool RookMoves(List<Pos> pos, int n, bool dir)
        {
            Pos currpos = new Pos(Pos.x + (dir ? n : 0), Pos.y + (!dir ? n : 0));
            Search search = ToAddPos(currpos);
            if (search.add)
                pos.Add(currpos);
            if (search.stop)
                return true;
            else
                return false;
        }
        public override List<Pos> PossibleMoves(bool test = false)
        {
            if (PieceColor != Board.GetTurn())
                return new List<Pos>();

            List<Pos> pos = new List<Pos>();
            for (int i = 1; i < 8; i++)
                if (RookMoves(pos, i, true))
                    break;
            for (int i = -1; i > -8; i--)
                if (RookMoves(pos, i, true))
                    break;
            for (int i = 1; i < 8; i++)
                if (RookMoves(pos, i, false))
                    break;
            for (int i = -1; i > -8; i--)
                if (RookMoves(pos, i, false))
                    break;

            if (!test)
            {
                Pos startpos = Pos;
                foreach (Pos p in pos.ToList())
                {
                    Capture captured = MovePiece(Board.GetBoard(), p, true);
                    PieceColor opposite = (PieceColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (Board.IsCheck(opposite))
                        pos.Remove(p);
                    MovePiece(Board.GetBoard(), startpos, true);
                    Board.GetBoard()[captured.pos.x][captured.pos.y].SetPiece(captured.piece);
                }
            }
            return pos;
        }
    }
    class Knight : Piece //конь производный класс от класса фигур
    {
        public Knight(Board board, int x, int y, PieceColor pieceColor)
        {
            Pos.x = x; Pos.y = y;
            Board = board;
            PieceColor = pieceColor;
            BitmapImage bmi = new BitmapImage(new Uri(PieceColor == PieceColor.White ? "pack://application:,,,/resources/whiteknight.png" : "pack://application:,,,/resources/blackknight.png"));
            image.Source = bmi;
        }

        public override List<Pos> PossibleMoves(bool test = false)
        {
            if (PieceColor != Board.GetTurn())
                return new List<Pos>();

            List<Pos> pos = new List<Pos>();
            for (int x = -2; x < 3; x++)
            {
                if (x != 0)
                {
                    Pos currpos;
                    currpos.x = Pos.x - x;
                    currpos.y = Pos.y - (2 / Math.Abs(x));
                    if (ToAddPos(currpos).add)
                        pos.Add(currpos);
                    currpos.y += 4 / Math.Abs(x);
                    if (ToAddPos(currpos).add)
                        pos.Add(currpos);
                }
            }

            if (!test)
            {
                Pos startpos = Pos;
                foreach (Pos p in pos.ToList())
                {
                    Capture captured = MovePiece(Board.GetBoard(), p, true);
                    PieceColor opposite = (PieceColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (Board.IsCheck(opposite))
                        pos.Remove(p);
                    MovePiece(Board.GetBoard(), startpos, true);
                    Board.GetBoard()[captured.pos.x][captured.pos.y].SetPiece(captured.piece);
                }
            }
            return pos;
        }
    }
    class Bishop : Piece //слон производный класс от класса фигур
    {
        public Bishop(Board board, int x, int y, PieceColor pieceColor)
        {
            Pos.x = x; Pos.y = y;
            Board = board;
            PieceColor = pieceColor;
            BitmapImage bmi = new BitmapImage(new Uri(PieceColor == PieceColor.White ? "pack://application:,,,/resources/whitebishop.png" : "pack://application:,,,/resources/blackbishop.png"));
            image.Source = bmi;
        }
        private bool BishopMoves(List<Pos> pos, int n, bool dir)
        {
            Pos currpos = new Pos(Pos.x + n, dir ? Pos.y + n : Pos.y - n);
            Search search = ToAddPos(currpos);
            if (search.add)
                pos.Add(currpos);
            if (search.stop)
                return true;
            else
                return false;
        }
        public override List<Pos> PossibleMoves(bool test = false)
        {
            if (PieceColor != Board.GetTurn())
                return new List<Pos>();

            List<Pos> pos = new List<Pos>();
            for (int i = 1; i < 8; i++)
                if (BishopMoves(pos, i, true))
                    break;
            for (int i = -1; i > -8; i--)
                if (BishopMoves(pos, i, true))
                    break;
            for (int i = 1; i < 8; i++)
                if (BishopMoves(pos, i, false))
                    break;
            for (int i = -1; i > -8; i--)
                if (BishopMoves(pos, i, false))
                    break;

            if (!test)
            {
                Pos startpos = Pos;
                foreach (Pos p in pos.ToList())
                {
                    Capture captured = MovePiece(Board.GetBoard(), p, true);
                    PieceColor opposite = (PieceColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (Board.IsCheck(opposite))
                        pos.Remove(p);
                    MovePiece(Board.GetBoard(), startpos, true);
                    Board.GetBoard()[captured.pos.x][captured.pos.y].SetPiece(captured.piece);
                }
            }
            return pos;
        }
    }
    class Queen : Piece //королева производный класс от класса фигур
    {
        public Queen(Board board, int x, int y, PieceColor pieceColor)
        {
            Pos.x = x; Pos.y = y;
            Board = board;
            PieceColor = pieceColor;
            BitmapImage bmi = new BitmapImage(new Uri(PieceColor == PieceColor.White ? "pack://application:,,,/resources/whitequeen.png" : "pack://application:,,,/resources/blackqueen.png"));
            image.Source = bmi;
        }
        private bool QueenDMoves(List<Pos> pos, int n, bool dir)
        {
            Pos currpos = new Pos(Pos.x + n, dir ? Pos.y + n : Pos.y - n);
            Search search = ToAddPos(currpos);
            if (search.add)
                pos.Add(currpos);
            if (search.stop)
                return true;
            else
                return false;
        }
        private bool QueenAMoves(List<Pos> pos, int n, bool dir)
        {
            Pos currpos = new Pos(Pos.x + (dir ? n : 0), Pos.y + (!dir ? n : 0));
            Search search = ToAddPos(currpos);
            if (search.add)
                pos.Add(currpos);
            if (search.stop)
                return true;
            else
                return false;
        }
        public override List<Pos> PossibleMoves(bool test = false)
        {
            if (PieceColor != Board.GetTurn())
                return new List<Pos>();

            List<Pos> pos = new List<Pos>();
            for (int i = 1; i < 8; i++)
                if (QueenAMoves(pos, i, true))
                    break;
            for (int i = -1; i > -8; i--)
                if (QueenAMoves(pos, i, true))
                    break;
            for (int i = 1; i < 8; i++)
                if (QueenAMoves(pos, i, false))
                    break;
            for (int i = -1; i > -8; i--)
                if (QueenAMoves(pos, i, false))
                    break;
            for (int i = 1; i < 8; i++)
                if (QueenDMoves(pos, i, true))
                    break;
            for (int i = -1; i > -8; i--)
                if (QueenDMoves(pos, i, true))
                    break;
            for (int i = 1; i < 8; i++)
                if (QueenDMoves(pos, i, false))
                    break;
            for (int i = -1; i > -8; i--)
                if (QueenDMoves(pos, i, false))
                    break;

            if (!test)
            {
                Pos startpos = Pos;
                foreach (Pos p in pos.ToList())
                {
                    Capture captured = MovePiece(Board.GetBoard(), p, true);
                    PieceColor opposite = (PieceColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (Board.IsCheck(opposite))
                        pos.Remove(p);
                    MovePiece(Board.GetBoard(), startpos, true);
                    Board.GetBoard()[captured.pos.x][captured.pos.y].SetPiece(captured.piece);
                }
            }
            return pos;
        }
    }
    class King : Piece //король производный класс от класса фигур
    {
        public King(Board board, int x, int y, PieceColor pieceColor)
        {
            Pos.x = x; Pos.y = y;
            Board = board;
            PieceColor = pieceColor;
            BitmapImage bmi = new BitmapImage(new Uri(PieceColor == PieceColor.White ? "pack://application:,,,/resources/whiteking.png" : "pack://application:,,,/resources/blackking.png"));
            image.Source = bmi;
        }
        private List<Pos> Castling()
        {
            if (moved)
                return null;
            List<Pos> list = new List<Pos>();
            bool add = true;
            Pos currpos = Pos, addpos;

            if (Board.GetBoard()[0][Pos.y].GetPiece() == null ||
                Board.GetBoard()[0][Pos.y].GetPiece() != null && Board.GetBoard()[0][Pos.y].GetPiece().GetType() != typeof(Rook) ||
                Board.GetBoard()[0][Pos.y].GetPiece() != null && Board.GetBoard()[0][Pos.y].GetPiece().GetType() == typeof(Rook) && Board.GetBoard()[0][Pos.y].GetPiece().WasMoved())
                add = false;
            for (int x = currpos.x - 1; x >= 0; x--)
            {
                if (Board.GetBoard()[x][Pos.y].GetPiece() != null && Board.GetBoard()[x][Pos.y].GetPiece().GetType() != typeof(Rook) ||
                    Board.GetBoard()[x][Pos.y].GetPiece() != null && Board.GetBoard()[x][Pos.y].GetPiece().GetType() == typeof(Rook) && Board.GetBoard()[x][Pos.y].GetPiece().WasMoved())
                {
                    add = false;
                    break;
                }
            }
            if (add)
            {
                Pos inpos = Pos;
                for (int i = 1; i < 3; i++)
                {
                    currpos.x = inpos.x - i;
                    MovePiece(Board.GetBoard(), currpos, true);
                    if (Board.IsCheck(PieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White))
                    {
                        add = false;
                        break;
                    }
                }
                MovePiece(Board.GetBoard(), inpos, true);
            }
            addpos.x = Pos.x - 2;
            addpos.y = Pos.y;
            if (add)
                list.Add(addpos);

            add = true;

            if (Board.GetBoard()[7][Pos.y].GetPiece() == null ||
                Board.GetBoard()[7][Pos.y].GetPiece() != null && Board.GetBoard()[7][Pos.y].GetPiece().GetType() != typeof(Rook) ||
                Board.GetBoard()[7][Pos.y].GetPiece() != null && Board.GetBoard()[7][Pos.y].GetPiece().GetType() == typeof(Rook) && Board.GetBoard()[7][Pos.y].GetPiece().WasMoved())
                add = false;
            for (int x = currpos.x + 1; x < 8; x++)
            {
                if (Board.GetBoard()[x][Pos.y].GetPiece() != null && Board.GetBoard()[x][Pos.y].GetPiece().GetType() != typeof(Rook) ||
                    Board.GetBoard()[x][Pos.y].GetPiece() != null && Board.GetBoard()[x][Pos.y].GetPiece().GetType() == typeof(Rook) && Board.GetBoard()[x][Pos.y].GetPiece().WasMoved())
                {
                    add = false;
                    break;
                }
            }
            if (add)
            {
                Pos inpos = Pos;
                for (int i = 1; i < 3; i++)
                {
                    currpos.x = inpos.x + i;
                    MovePiece(Board.GetBoard(), currpos, true);
                    if (Board.IsCheck(PieceColor == PieceColor.White ? PieceColor.Black : PieceColor.White))
                    {
                        add = false;
                        break;
                    }
                }
                MovePiece(Board.GetBoard(), inpos, true);
            }
            addpos.x = Pos.x + 2;
            addpos.y = Pos.y;
            if (add)
                list.Add(addpos);
            return list;
        }
        public override List<Pos> PossibleMoves(bool test = false)
        {
            if (PieceColor != Board.GetTurn())
                return new List<Pos>();

            List<Pos> pos = new List<Pos>();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i != 0 || j != 0)
                    {
                        Pos currpos = new Pos(Pos.x + i, Pos.y + j);
                        if (ToAddPos(currpos).add)
                            pos.Add(currpos);
                    }
                }
            }

            List<Pos> castling = Castling();
            if (castling != null)
                pos.AddRange(castling);

            if (!test)
            {
                Pos startpos = Pos;
                foreach (Pos p in pos.ToList())
                {
                    Capture captured = MovePiece(Board.GetBoard(), p, true);
                    PieceColor opposite = (PieceColor == PieceColor.White) ? PieceColor.Black : PieceColor.White;
                    if (Board.IsCheck(opposite))
                        pos.Remove(p);
                    MovePiece(Board.GetBoard(), startpos, true);
                    Board.GetBoard()[captured.pos.x][captured.pos.y].SetPiece(captured.piece);
                }
            }
            return pos;
        }
    }
}
