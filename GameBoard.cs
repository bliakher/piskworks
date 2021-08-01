namespace piskworks
{
    public enum SymbolKind
    {
        Free,
        Nought,
        Cross,
        Invalid
    }
    
    public class GameBoard
    {
        private SymbolKind[,,] board;
        public int N;

        public GameBoard(int dimension)
        {
            N = dimension;
            board = new SymbolKind[N, N, N];
        }

        public bool PlaceSybol(int x, int y, int z, SymbolKind symbol)
        {
            if (board[x,y,z] != SymbolKind.Free) {
                return false;
            }
            board[x, y, z] = symbol;
            return true;
        }

        public void DoMove(GameMove move)
        {
            PlaceSybol(move.X, move.Y, move.Z, move.Symbol);
        }

        public SymbolKind GetSymbol(int x, int y, int z)
        {
            return board[x, y, z];
        }

        public bool CheckForWin()
        {
            // ToDo: add inclined and diagonals
            return checkHorizontalX() || checkHorizontalY() || checkVertical();
        }

        private bool checkHorizontalY()
        {
            for (int z = 0; z < N; z++) {
                for (int x = 0; x < N; x++) {
                    SymbolKind cur = SymbolKind.Invalid;
                    for (int y = 0; y < N; y++) {
                        if (cur == SymbolKind.Invalid) {
                            if (board[x,y,z] == SymbolKind.Free) {
                                break;
                            }
                            cur = board[x, y, z];
                        }
                        else {
                            if (cur != board[x,y,z]) {
                                break;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool checkHorizontalX()
        {
            for (int z = 0; z < N; z++) {
                for (int y = 0; y < N; y++) {
                    SymbolKind cur = SymbolKind.Invalid;
                    for (int x = 0; x < N; x++) {
                        if (cur == SymbolKind.Invalid) {
                            if (board[x,y,z] == SymbolKind.Free) {
                                break;
                            }
                            cur = board[x, y, z];
                        }
                        else {
                            if (cur != board[x,y,z]) {
                                break;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool checkVertical()
        {
            for (int x = 0; x < N; x++) {
                for (int y = 0; y < N; y++) {
                    SymbolKind cur = SymbolKind.Invalid;
                    for (int z = 0; z < N; z++) {
                        if (cur == SymbolKind.Invalid) {
                            if (board[x,y,z] == SymbolKind.Free) {
                                break;
                            }
                            cur = board[x, y, z];
                        }
                        else {
                            if (cur != board[x,y,z]) {
                                break;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public struct GameMove
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public SymbolKind Symbol { get; }

        public GameMove(int x, int y, int z, SymbolKind symbol)
        {
            X = x;
            Y = y;
            Z = z;
            Symbol = symbol;
        }
    }
}