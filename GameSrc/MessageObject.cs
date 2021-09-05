namespace piskworks.GameSrc
{
    /// <summary>
    /// Kinds of messages
    /// </summary>
    public enum MessageKind
    {
        Move, // msg about a move of a player on the game board
        GameOver, // msg about the game ending with who won
        Dimension // msg with chosen dimension of the board
    }
    
    /// <summary>
    /// Object that represents message.
    ///
    /// Message objects are serialized and sent through the network between players.
    /// Different fields of the MessageObject are used depending on the kind of message
    /// </summary>
    public class MessageObject
    {
        public MessageKind Kind { get; set; }
        
        public int Dimension { get; set; }
        
        public GameMove Move { get; set; }
        public bool IsWinning { get; set; }
        
        public bool IsDraw { get; set; }
        
        /// <summary>
        /// Creates message with MessageKind.Move
        /// </summary>
        /// <param name="move">Move on the <see cref="GameBoard"/></param>
        /// <param name="isWinning">If this move wins the game</param>
        public static MessageObject CreateMoveMsg(GameMove move, bool isWinning = false)
        {
            return new MessageObject() {Kind = MessageKind.Move, Dimension = -1, Move = move, IsWinning = isWinning, IsDraw = false};
        }

        /// <summary>
        /// Creates message with MessageKind.Dimension
        /// </summary>
        /// <param name="dimension">Dimension of the board</param>
        public static MessageObject CreateDimensionMsg(int dimension)
        {
            return new MessageObject()
                {Kind = MessageKind.Dimension, Dimension = dimension, 
                    Move = new GameMove(-1, -1, -1, SymbolKind.Invalid), IsWinning = false, IsDraw = false};
        }

        /// <summary>
        /// Creates message with MessageKind.GameOver
        /// </summary>
        /// <param name="iswinning">True if game ended because of the victory of one player,
        /// false if ended because one player quit the game</param>
        /// <returns></returns>
        public static MessageObject CreateGameOverMsg(bool iswinning, bool isDraw = false)
        {
            return new MessageObject()
            {Kind = MessageKind.GameOver, Dimension = -1, 
                Move = new GameMove(-1, -1, -1, SymbolKind.Invalid), IsWinning = iswinning, IsDraw = isDraw};
        }
    }
    

}