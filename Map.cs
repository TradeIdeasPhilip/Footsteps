using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Footsteps
{
    enum CellType { Free, Blocked, Goal, Death };

    /// <summary>
    /// This describes the world before you make any moves.  This includes the player's starting
    /// position, the size of the world, and the positions of any fixed objects, like the goal.
    /// </summary>
    class Map
    {
        private readonly List<List<CellType>> _rows = new List<List<CellType>>();
        static readonly char[] SPLIT = new char[] { '\n', '|' };

        /// <summary>
        /// The player's initial position.
        /// </summary>
        public int InitialX { get; private set; }

        /// <summary>
        /// The player's initial position.
        /// </summary>
        public int InitialY { get; private set; }

        /// <summary>
        /// How tall is the world?
        /// </summary>
        public int RowCount { get { return _rows.Count; } }

        /// <summary>
        /// How wide is the world?
        /// </summary>
        public int ColumnCount { get; private set; }

        /// <summary>
        /// Find the content of the cell.  This only finds fixed items, like monsters.
        /// The player can move, so WoldView needs to keep track of the player's position. 
        /// 
        /// It is legal to ask for the content of a cell that's off the edge of the map.
        /// That always shows up as blocked.  That makes it easier for other components.
        /// </summary>
        /// <param name="x">0 is the far left.  Positive numbers go right.</param>
        /// <param name="y">0 is the top.  Positive numbers go down.</param>
        /// <returns>The content of the requested cell.</returns>
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

        /// <summary>
        /// Create a map from a single string, using a | or a new line to seperate each row from the next.
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns></returns>
        public static Map FromOneString(string encoded)
        {
            return new Map(encoded.Split(SPLIT));
        }

        /// <summary>
        /// Create a map from an array of strings.  That might be easier to write in some cases.
        /// </summary>
        /// <param name="encodedRows"></param>
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
