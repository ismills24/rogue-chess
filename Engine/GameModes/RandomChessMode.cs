using RogueChess.Engine.Board;
using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces;
using RogueChess.Engine.Pieces.Decorators;
using RogueChess.Engine.Primitives;
using RogueChess.Engine.RuleSets;
using RogueChess.Engine.Tiles;

namespace RogueChess.Engine.GameModes
{
    /// <summary>
    /// Random chess game mode with random board size, pieces, decorators, and special tiles
    /// </summary>
    public class RandomChessMode : IGameMode
    {
        private readonly Random _random = new Random();
        private int _boardWidth;
        private int _boardHeight;

        public string Name => "Random Chess";
        public string Description =>
            "Chaotic chess with random board size, pieces, decorators, and special tiles";

        public IBoard SetupBoard()
        {
            // Random board size between 4x4 and 8x8
            _boardWidth = _random.Next(4, 9); // 4 to 8
            _boardHeight = _boardWidth;

            var board = new RogueChess.Engine.Board.Board(_boardWidth, _boardHeight);

            // Set up random special tiles
            SetupRandomTiles(board);

            // Set up random pieces with decorators
            SetupRandomPieces(board);

            return board;
        }

        private void SetupRandomTiles(IBoard board)
        {
            var tileTypes = new Type[]
            {
                typeof(ScorchedTile),
                typeof(SlipperyTile),
                typeof(GuardianTile),
            };

            // Place random special tiles (about 20% of the board)
            for (int x = 0; x < _boardWidth; x++)
            {
                for (int y = 0; y < _boardHeight; y++)
                {
                    if (_random.NextDouble() < 0.2) // 20% chance
                    {
                        var tileType = tileTypes[_random.Next(tileTypes.Length)];
                        var tile = (ITile)Activator.CreateInstance(tileType)!;
                        board.SetTile(new Vector2Int(x, y), tile);
                    }
                }
            }
        }

        private void SetupRandomPieces(IBoard board)
        {
            var pieceTypes = new Type[]
            {
                typeof(Pawn),
                typeof(Rook),
                typeof(Knight),
                typeof(Bishop),
                typeof(Queen),
            };
            var decoratorTypes = new Type[] { typeof(ExplodingDecorator), typeof(MartyrDecorator) };

            // Calculate how many pieces to place (max 50% of board, but at least 2 per side)
            var totalSquares = _boardWidth * _boardHeight;
            var maxPiecesPerSide = Math.Min(totalSquares / 4, _boardWidth * 2); // Max 50% of board, but reasonable limit
            var piecesPerSide = Math.Max(2, _random.Next(2, maxPiecesPerSide + 1));

            // Place White pieces
            PlaceRandomPiecesForColor(
                board,
                PlayerColor.White,
                0,
                piecesPerSide,
                pieceTypes,
                decoratorTypes
            );

            // Place Black pieces
            PlaceRandomPiecesForColor(
                board,
                PlayerColor.Black,
                _boardHeight - 1,
                piecesPerSide,
                pieceTypes,
                decoratorTypes
            );
        }

        private void PlaceRandomPiecesForColor(
            IBoard board,
            PlayerColor color,
            int startRow,
            int pieceCount,
            Type[] pieceTypes,
            Type[] decoratorTypes
        )
        {
            var placedPieces = 0;
            var attempts = 0;
            var maxAttempts = _boardWidth * _boardHeight * 2; // Prevent infinite loops

            while (placedPieces < pieceCount && attempts < maxAttempts)
            {
                var x = _random.Next(_boardWidth);
                var y = _random.Next(_boardHeight);
                var pos = new Vector2Int(x, y);

                // Skip if position is already occupied
                if (board.GetPieceAt(pos) != null)
                {
                    attempts++;
                    continue;
                }

                // For the first piece of each color, always place a King
                IPiece piece;
                if (placedPieces == 0)
                {
                    piece = new King(color, pos);
                }
                else
                {
                    // Random piece type for other pieces
                    var pieceType = pieceTypes[_random.Next(pieceTypes.Length)];
                    piece = (IPiece)Activator.CreateInstance(pieceType, color, pos)!;
                }

                // Add random decorators
                piece = AddRandomDecorators(piece, decoratorTypes);

                board.PlacePiece(piece, pos);
                placedPieces++;
                attempts++;
            }
        }

        private IPiece AddRandomDecorators(IPiece piece, Type[] decoratorTypes)
        {
            // Randomly add a status effect (30% chance)
            if (_random.NextDouble() < 0.3)
            {
                var effects = new RogueChess.Engine.StatusEffects.IStatusEffect[]
                {
                    new RogueChess.Engine.StatusEffects.BurningStatus(),
                    // add more when you implement them, e.g. new PoisonedStatus()
                };
                var effect = effects[_random.Next(effects.Length)];

                var statusDecorator = new StatusEffectDecorator(piece);
                statusDecorator.AddStatus(effect);
                piece = statusDecorator;
            }

            // Randomly add Exploding (30%)
            if (_random.NextDouble() < 0.3)
            {
                piece = new ExplodingDecorator(piece);
            }

            // Randomly add Martyr (30%)
            if (_random.NextDouble() < 0.3)
            {
                piece = new MartyrDecorator(piece);
            }

            return piece;
        }

        public IRuleSet GetRuleSet()
        {
            return new LastPieceStandingRuleSet(); // Use last piece standing for random games
        }
    }
}
