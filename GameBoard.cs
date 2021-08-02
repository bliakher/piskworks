namespace piskworks
{
    public enum SymbolKind
    {
        Free,
        Nought,
        Cross
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

        public void FillForTesting()
        {
            PlaceSybol(0, 0, 0, SymbolKind.Cross);
            PlaceSybol(0, 1, 0, SymbolKind.Cross);
            PlaceSybol(3, 3, 3, SymbolKind.Cross);
            
            PlaceSybol(1, 2, 2, SymbolKind.Nought);
            PlaceSybol(1, 3, 3, SymbolKind.Nought);
        }

        public bool CheckForWin()
        {
            return CheckNoughtsForWin() || CheckCrossesForWin();
        }

        public bool CheckNoughtsForWin()
        {
            var nought = SymbolKind.Nought;
            return checkHorizontalX(nought) || checkHorizontalY(nought) || checkVertical(nought) 
                   || checkInclined(nought) || checkDiagonal(nought);
        }

        public bool CheckCrossesForWin()
        {
            var cross = SymbolKind.Cross;
            return checkHorizontalX(cross) || checkHorizontalY(cross) || checkVertical(cross) 
                   || checkInclined(cross) || checkDiagonal(cross);
        }

        private bool checkHorizontalY(SymbolKind checkSymbol)
        {
            for (int z = 0; z < N; z++) {
                for (int x = 0; x < N; x++) {
                    bool hasWin = true;
                    for (int y = 0; y < N; y++) {
                        hasWin &= checkOne(checkSymbol, x, y, z);
                    }
                    if (hasWin) {
                        return true;    
                    }
                }
            }
            return false;
        }

        private bool checkHorizontalX(SymbolKind checkSymbol)
        {
            for (int z = 0; z < N; z++) {
                for (int y = 0; y < N; y++) {
                    bool hasWin = true;
                    for (int x = 0; x < N; x++) {
                        hasWin &= checkOne(checkSymbol, x, y, z);
                    }
                    if (hasWin) {
                        return true;   
                    }
                }
            }
            return false;
        }

        private bool checkVertical(SymbolKind checkSymbol)
        {
            for (int x = 0; x < N; x++) {
                for (int y = 0; y < N; y++) {
                    bool hasWin = true;
                    for (int z = 0; z < N; z++) {
                        hasWin &= checkOne(checkSymbol, x, y, z);
                    }
                    if (hasWin) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool checkInclined(SymbolKind checkSymbol)
        {
            for (int x = 0; x < N; x++) {
                bool hasWin = true;
                for (int yz = 0; yz < N; yz++) {
                    hasWin &= checkOne(checkSymbol, x, yz, yz);
                }
                if (hasWin) {
                    return true;
                }
                hasWin = true;
                for (int y = 0; y < N; y++) {
                    hasWin &= checkOne(checkSymbol, x, y, N - y - 1);
                }
                if (hasWin) {
                    return true;
                }
            }

            for (int y = 0; y < N; y++) {
                bool hasWin = true;
                for (int xz = 0; xz < N; xz++) {
                    hasWin &= checkOne(checkSymbol, xz, y, xz);
                }
                if (hasWin) {
                    return true;
                }
                hasWin = true;
                for (int x = 0; x < N; x++) {
                    hasWin &= checkOne(checkSymbol, x, y, N - x - 1);
                }
                if (hasWin) {
                    return true;
                }
            }

            for (int z = 0; z < N; z++) {
                bool hasWin = true;
                for (int xy = 0; xy < N; xy++) {
                    hasWin &= checkOne(checkSymbol, xy, xy, z);
                }
                if (hasWin) {
                    return true;
                }
                hasWin = true;
                for (int x = 0; x < N; x++) {
                    hasWin &= checkOne(checkSymbol, x, N - x - 1, z);
                }
                if (hasWin) {
                    return true;
                }
            }
            return false;
        }

        private bool checkDiagonal(SymbolKind checkSymbol)
        {
            bool hasWin = true;
            for (int xyz = 0; xyz < N; xyz++) {
                hasWin &= checkOne(checkSymbol, xyz, xyz, xyz);
            }
            if (hasWin) {
                return true;
            }
            hasWin = true;
            for (int xy = 0; xy < N; xy++) {
                hasWin &= checkOne(checkSymbol, xy, xy, N - xy - 1);
            }
            if (hasWin) {
                return true;
            }
            hasWin = true;
            for (int yz = 0; yz < N; yz++) {
                hasWin &= checkOne(checkSymbol, N - yz - 1, yz, yz);
            }
            if (hasWin) {
                return true;
            }
            hasWin = true;
            for (int x = 0; x < N; x++) {
                hasWin &= checkOne(checkSymbol, x, N - x - 1, N - x - 1);
            }
            if (hasWin) {
                return true;
            }

            return false;
        }
        

        private bool checkOne(SymbolKind checkSymbol, int x, int y, int z)
        {
            if (board[x, y, z] == checkSymbol) {
                return true;
            }
            return false;
        }
    }
    

    public readonly struct GameMove
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