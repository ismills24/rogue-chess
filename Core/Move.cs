namespace ChessRogue.Core
{
    public class Move
    {
        public Vector2Int From { get; }
        public Vector2Int To { get; }
        public IPiece Piece { get; }
        public bool IsCapture { get; }

        public Move(Vector2Int from, Vector2Int to, IPiece piece, bool isCapture = false)
        {
            From = from;
            To = to;
            Piece = piece;
            IsCapture = isCapture;
        }

        public override string ToString()
        {
            string fromFile = ((char)('a' + From.x)).ToString();
            string fromRank = (From.y + 1).ToString();
            string toFile = ((char)('a' + To.x)).ToString();
            string toRank = (To.y + 1).ToString();

            string capture = IsCapture ? "x" : "-";
            return $"{NameSymbol(Piece)} {fromFile}{fromRank}{capture}{toFile}{toRank}";
        }

        private string NameSymbol(IPiece piece)
        {
            return piece.Name switch
            {
                "Pawn" => "",
                "Knight" => "N",
                "King" => "K",
                "Queen" => "Q",
                "Rook" => "R",
                "Bishop" => "B",
                _ => piece.Name.Substring(0, 1),
            };
        }
    }
}
