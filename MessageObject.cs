namespace piskworks
{
    public enum MessageKind
    {
        Initial,
        Move,
        GameEnd
    }
    public class MessageObject
    {
        public MessageKind Kind { get; set; }
    }

    public class MoveMsgObject : MessageObject
    {
        public GameMove Move { get; set; }
        public bool IsWinning { get; set; }

        public MoveMsgObject()
        {
        }

        public MoveMsgObject(GameMove move, bool isWinning = false)
        {
            Move = move;
            Kind = MessageKind.Move;
            IsWinning = isWinning;
        }
    }
}