namespace piskworks.GameSrc
{
    /// <summary>
    /// Symbols on the board (or free field)
    /// </summary>
    public enum SymbolKind
    {
        Free,
        Nought,
        Cross,
        Invalid
    }
    
    /// <summary>
    /// 3D board for noughts and crosses.
    /// </summary>
    public class GameBoard
    {
        private SymbolKind[,,] _board;
        private int _symbolCount;
        
        public int N;
        public Field[] WinningFields;

        /// <summary>
        /// True if all fields of the board have symbols placed
        /// </summary>
        public bool BoardIsFull => _symbolCount == N * N * N;

        public GameBoard(int dimension)
        {
            N = dimension;
            _board = new SymbolKind[N, N, N];
            _symbolCount = 0;
        }

        /// <summary>
        /// Place a symbol on the coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="symbol">Type of symbol - nought or cross</param>
        /// <returns>If it is possible to place symbol (field is free)</returns>
        public bool PlaceSybol(int x, int y, int z, SymbolKind symbol)
        {
            if (_board[x,y,z] != SymbolKind.Free) {
                return false;
            }
            _board[x, y, z] = symbol;
            _symbolCount++;
            return true;
        }

        /// <summary>
        /// Do move on the game board.
        /// </summary>
        /// <param name="move">wanted move <see cref="GameMove"/></param>
        /// <returns>If move was sucessful</returns>
        public bool DoMove(GameMove move)
        {
            return PlaceSybol(move.Field.X, move.Field.Y, move.Field.Z, move.Symbol);
        }

        /// <summary>
        /// Get the symbol on specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>symbol from the board</returns>
        public SymbolKind GetSymbol(int x, int y, int z)
        {
            return _board[x, y, z];
        }

        /// <summary>
        /// Fill the board with some symbols for testing purposes.
        /// </summary>
        public void FillForTesting()
        {
            PlaceSybol(0, 0, 0, SymbolKind.Cross);
            PlaceSybol(0, 1, 0, SymbolKind.Cross);
            PlaceSybol(1, 1, 0, SymbolKind.Nought);
            PlaceSybol(2,2,2, SymbolKind.Cross);
            
            PlaceSybol(1, 2, 2, SymbolKind.Nought);
            //PlaceSybol(1, 3, 3, SymbolKind.Nought);
        }

        /// <summary>
        /// Checks if there is a win on the board - N symbols in arow in any direction
        /// Only checks for one symbol
        /// </summary>
        /// <param name="symbol">Symbol to check for a win - nought or a cross</param>
        /// <returns></returns>
        public bool CheckForWin(SymbolKind symbol)
        {
            WinningFields = new Field[N];
            bool result = symbol == SymbolKind.Cross ? checkCrossesForWin() : checkNoughtsForWin();
            if (result == false) {
                WinningFields = null; // winning fields has values only when there is a win on the board
            }
            return result;
        }

        private bool checkNoughtsForWin()
        {
            var nought = SymbolKind.Nought;
            return checkHorizontalX(nought) || checkHorizontalY(nought) || checkVertical(nought) 
                   || checkInclined(nought) || checkDiagonal(nought);
        }

        private bool checkCrossesForWin()
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
                        WinningFields[y] = new Field(x, y, z);
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
                        WinningFields[x] = new Field(x, y, z);
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
                        WinningFields[z] = new Field(x, y, z);
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
                    WinningFields[yz] = new Field(x, yz, yz);
                    hasWin &= checkOne(checkSymbol, x, yz, yz);
                }
                if (hasWin) {
                    return true;
                }
                hasWin = true;
                for (int y = 0; y < N; y++) {
                    WinningFields[y] = new Field(x, y, N - y - 1);
                    hasWin &= checkOne(checkSymbol, x, y, N - y - 1);
                }
                if (hasWin) {
                    return true;
                }
            }

            for (int y = 0; y < N; y++) {
                bool hasWin = true;
                for (int xz = 0; xz < N; xz++) {
                    WinningFields[xz] = new Field(xz, y, xz);
                    hasWin &= checkOne(checkSymbol, xz, y, xz);
                }
                if (hasWin) {
                    return true;
                }
                hasWin = true;
                for (int x = 0; x < N; x++) {
                    WinningFields[x] = new Field(x, y, N - x - 1);
                    hasWin &= checkOne(checkSymbol, x, y, N - x - 1);
                }
                if (hasWin) {
                    return true;
                }
            }

            for (int z = 0; z < N; z++) {
                bool hasWin = true;
                for (int xy = 0; xy < N; xy++) {
                    WinningFields[xy] = new Field(xy, xy, z);
                    hasWin &= checkOne(checkSymbol, xy, xy, z);
                }
                if (hasWin) {
                    return true;
                }
                hasWin = true;
                for (int x = 0; x < N; x++) {
                    WinningFields[x] = new Field(x, N - x - 1, z);
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
                WinningFields[xyz] = new Field(xyz, xyz, xyz);
                hasWin &= checkOne(checkSymbol, xyz, xyz, xyz);
            }
            if (hasWin) {
                return true;
            }
            hasWin = true;
            for (int xy = 0; xy < N; xy++) {
                WinningFields[xy] = new Field(xy, xy, N - xy - 1);
                hasWin &= checkOne(checkSymbol, xy, xy, N - xy - 1);
            }
            if (hasWin) {
                return true;
            }
            hasWin = true;
            for (int xz = 0; xz < N; xz++) {
                WinningFields[xz] = new Field(xz, N - xz - 1, xz);
                hasWin &= checkOne(checkSymbol, xz, N - xz - 1, xz);
            }
            if (hasWin) {
                return true;
            }
            hasWin = true;
            for (int x = 0; x < N; x++) {
                WinningFields[x] = new Field(x, N - x - 1, N - x - 1);
                hasWin &= checkOne(checkSymbol, x, N - x - 1, N - x - 1);
            }
            if (hasWin) {
                return true;
            }

            return false;
        }
        

        private bool checkOne(SymbolKind checkSymbol, int x, int y, int z)
        {
            if (_board[x, y, z] == checkSymbol) {
                return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// Move on the <see cref="GameBoard"/> - field to place the symbol and the kind of symbol
    /// </summary>
    public struct GameMove
    {
        /// <summary>
        /// Field in the <see cref="GameBoard"/> to place the symbol
        /// </summary>
        public Field Field { get; set; }
        /// <summary>
        /// Nought or cross
        /// </summary>
        public SymbolKind Symbol { get; set; }

        public GameMove(int x, int y, int z, SymbolKind symbol)
        {
            Field = new Field(x, y, z);
            Symbol = symbol;
        }

        public override string ToString()
        {
            return "Move " + Symbol + $" X: {Field.X} Y: {Field.Y} Z: {Field.Z}";
        }
    }

    /// <summary>
    /// Struct holding coordinates of a field in the <see cref="GameBoard"/>
    /// </summary>
    public struct Field
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public Field(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
    }
}