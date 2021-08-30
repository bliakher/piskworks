namespace piskworks
{
    public enum MessageKind
    {
        Move,
        GameOver,
        Dimension
    }
    public class MessageObject
    {
        public MessageKind Kind { get; set; }
        
        public int Dimension { get; set; }
        
        public GameMove Move { get; set; }
        public bool IsWinning { get; set; }
        
        public static MessageObject CreateMoveMsg(GameMove move, bool isWinning = false)
        {
            return new MessageObject() {Kind = MessageKind.Move, Dimension = -1, Move = move, IsWinning = isWinning};
        }

        public static MessageObject CreateDimensionMsg(int dimension)
        {
            return new MessageObject()
                {Kind = MessageKind.Dimension, Dimension = dimension, 
                    Move = new GameMove(-1, -1, -1, SymbolKind.Invalid), IsWinning = false};
        }

        public static MessageObject CreateGameOverMsg()
        {
            return new MessageObject()
            {Kind = MessageKind.GameOver, Dimension = -1, 
                Move = new GameMove(-1, -1, -1, SymbolKind.Invalid), IsWinning = false};
        }
    }
    

}