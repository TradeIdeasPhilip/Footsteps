using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footsteps
{
    enum CellType { Free, Blocked, Goal, Death };

    class Map
    {
        private readonly List<List<CellType>> _rows = new List<List<CellType>>();
        static readonly char[] SPLIT = new char[] { '\n', '|' };
        public int InitialX { get; private set; }
        public int InitialY { get; private set; }
        public int RowCount { get { return _rows.Count; } }
        public int ColumnCount { get; private set; }
        public CellType GetCellType(int x, int y)
        {
            if ((x < 0) || (y < 0))
                return CellType.Blocked;
            if (y >= RowCount)
                return CellType.Blocked;
            List<CellType> row = _rows[y];
            if (x >= row.Count)
                return CellType.Blocked;
            return row[x];
        }
        public static Map FromOneString(string encoded)
        {
            return new Map(encoded.Split(SPLIT));
        }
        public Map(params string[] encodedRows)
        {
            foreach (string encodedRow in encodedRows)
            {
                List<CellType> row = new List<CellType>();
                foreach (char ch in encodedRow)
                {
                    switch (ch)
                    {
                        case ' ':
                        case '_':
                            row.Add(CellType.Free);
                            break;
                        case '.':
                            row.Add(CellType.Blocked);
                            break;
                        case 'X':
                        case 'x':
                            row.Add(CellType.Death);
                            break;
                        case 'G':
                        case 'g':
                            row.Add(CellType.Goal);
                            break;
                        case 'S':
                        case 's':
                            // S is for Start.
                            InitialX = row.Count;
                            InitialY = _rows.Count;
                            row.Add(CellType.Free);
                            break;
                    }
                    if (row.Count > ColumnCount)
                        ColumnCount = row.Count;
                }
                _rows.Add(row);
            }
        }
    }
}
