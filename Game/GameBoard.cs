using System;

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
        
        public Field[] WinningFields;

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

        public bool DoMove(GameMove move)
        {
            return PlaceSybol(move.Field.X, move.Field.Y, move.Field.Z, move.Symbol);
        }

        public SymbolKind GetSymbol(int x, int y, int z)
        {
            return board[x, y, z];
        }

        public void FillForTesting()
        {
            PlaceSybol(0, 0, 0, SymbolKind.Cross);
            PlaceSybol(0, 1, 0, SymbolKind.Cross);
            PlaceSybol(1, 1, 0, SymbolKind.Nought);
            PlaceSybol(3, 3, 3, SymbolKind.Cross);
            
            PlaceSybol(1, 2, 2, SymbolKind.Nought);
            PlaceSybol(1, 3, 3, SymbolKind.Nought);
        }

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
            if (board[x, y, z] == checkSymbol) {
                return true;
            }
            return false;
        }
    }
    

    public struct GameMove
    {
        public Field Field { get; set; }
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