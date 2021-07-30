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
        private SymbolKind[,,] field;
        private int N;

        public GameBoard(int dimension)
        {
            N = dimension;
            field = new SymbolKind[N, N, N];
        }

        public bool PlaceSybol(int x, int y, int z, SymbolKind symbol)
        {
            if (field[x,y,z] != SymbolKind.Free) {
                return false;
            }
            field[x, y, z] = symbol;
            return true;
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
                            if (field[x,y,z] == SymbolKind.Free) {
                                break;
                            }
                            cur = field[x, y, z];
                        }
                        else {
                            if (cur != field[x,y,z]) {
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
                            if (field[x,y,z] == SymbolKind.Free) {
                                break;
                            }
                            cur = field[x, y, z];
                        }
                        else {
                            if (cur != field[x,y,z]) {
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
                            if (field[x,y,z] == SymbolKind.Free) {
                                break;
                            }
                            cur = field[x, y, z];
                        }
                        else {
                            if (cur != field[x,y,z]) {
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
}