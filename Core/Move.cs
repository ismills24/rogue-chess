using UnityEngine;

namespace ChessRogue.Core
{
    public struct Move
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
            string capture = IsCapture ? "x" : "-";
            return $"{Piece.Name} {From} {capture} {To}";
        }
    }
}
