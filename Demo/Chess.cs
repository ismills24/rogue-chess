using System;
using UnityEngine;
using ChessRogue.Core;
using ChessRogue.Core.Runner;
using ChessRogue.Core.Rules;

class Program
{
    static void Main()
    {
        var board = SetupStandardBoard();

        var state = new GameState(board, PlayerColor.White);

        var runner = new GameRunner(
            state,
            new HumanController(),   // White
            new HumanController(),   // Black
            new CheckmateCondition()
        );

        // Run game loop
        while (true)
        {
            PrintBoard(state.Board);
            runner.RunTurn();
        }
    }

    static Board SetupStandardBoard()
    {
        var board = new Board(8, 8);

        // Pawns
        for (int x = 0; x < 8; x++)
        {
            board.PlacePiece(new Pawn(PlayerColor.White, new Vector2Int(x,1)), new Vector2Int(x,1));
            board.PlacePiece(new Pawn(PlayerColor.Black, new Vector2Int(x,6)), new Vector2Int(x,6));
        }

        // Rooks
        board.PlacePiece(new Rook(PlayerColor.White, new Vector2Int(0,0)), new Vector2Int(0,0));
        board.PlacePiece(new Rook(PlayerColor.White, new Vector2Int(7,0)), new Vector2Int(7,0));
        board.PlacePiece(new Rook(PlayerColor.Black, new Vector2Int(0,7)), new Vector2Int(0,7));
        board.PlacePiece(new Rook(PlayerColor.Black, new Vector2Int(7,7)), new Vector2Int(7,7));

        // Knights
        board.PlacePiece(new Knight(PlayerColor.White, new Vector2Int(1,0)), new Vector2Int(1,0));
        board.PlacePiece(new Knight(PlayerColor.White, new Vector2Int(6,0)), new Vector2Int(6,0));
        board.PlacePiece(new Knight(PlayerColor.Black, new Vector2Int(1,7)), new Vector2Int(1,7));
        board.PlacePiece(new Knight(PlayerColor.Black, new Vector2Int(6,7)), new Vector2Int(6,7));

        // Bishops
        board.PlacePiece(new Bishop(PlayerColor.White, new Vector2Int(2,0)), new Vector2Int(2,0));
        board.PlacePiece(new Bishop(PlayerColor.White, new Vector2Int(5,0)), new Vector2Int(5,0));
        board.PlacePiece(new Bishop(PlayerColor.Black, new Vector2Int(2,7)), new Vector2Int(2,7));
        board.PlacePiece(new Bishop(PlayerColor.Black, new Vector2Int(5,7)), new Vector2Int(5,7));

        // Queens
        board.PlacePiece(new Queen(PlayerColor.White, new Vector2Int(3,0)), new Vector2Int(3,0));
        board.PlacePiece(new Queen(PlayerColor.Black, new Vector2Int(3,7)), new Vector2Int(3,7));

        // Kings
        board.PlacePiece(new King(PlayerColor.White, new Vector2Int(4,0)), new Vector2Int(4,0));
        board.PlacePiece(new King(PlayerColor.Black, new Vector2Int(4,7)), new Vector2Int(4,7));

        return board;
    }

    static void PrintBoard(Board board)
    {
        for (int y = board.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var piece = board.GetPieceAt(new Vector2Int(x,y));
                if (piece == null) Console.Write(".");
                else
                {
                    char c = piece.Name[0];
                    if (piece.Owner == PlayerColor.Black)
                        c = char.ToLower(c);
                    Console.Write(c);
                }
                Console.Write(" ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}
