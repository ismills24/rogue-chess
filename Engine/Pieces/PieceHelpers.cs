using RogueChess.Engine.Interfaces;
using RogueChess.Engine.Pieces.Decorators;

namespace RogueChess.Engine.Pieces
{
    public static class PieceValueCalculator
    {
        public static int GetTotalValue(IPiece piece)
        {
            int value = 0;

            // Walk decorators until we hit the core piece
            var current = piece;
            while (true)
            {
                value += current.GetValue();

                if (current is StatusEffectDecorator statusDecorator)
                {
                    // Add modifiers from attached status effects
                    foreach (var status in statusDecorator.GetStatuses())
                    {
                        value += status.ValueModifier();
                    }

                    current = statusDecorator.Inner;
                }
                else if (current is PieceDecoratorBase decorator)
                {
                    current = decorator.Inner;
                }
                else
                {
                    break;
                }
            }

            return value;
        }
    }
}
