using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gwen.Control;

namespace ProjectorGaletes
{
    public interface Widthable
    {
        // interface members
        int Width { get;}
        int Height { get; }
        void SetPosition(int X, int Y);
    }


    class GLGrid : Dictionary<int, Dictionary<int, Widthable>>
    {
        public int originX;
        public int originY;
        public Dictionary<int, int> colsWidth;
        public Dictionary<int, int> rowsHeight;

        public GLGrid(int originX=0, int originY=0) 
        {
            this.originX = originX;
            this.originY = originY;
        }

        public void AddChild(int row, int col, Widthable child){

            if (child == null) { return; }

            if (this.ContainsKey(row))
            {
                this[row].Add(col, child);
            }
            else
            {
                this.Add(row, new Dictionary<int, Widthable> { { col, child } });
            }
            
        }

        public void Redraw()
        {
            int maxCols = 0;
            int maxRows = 0;
            int auxRow;
            int auxCol;
            Dictionary<int, int> maxColsWidth = new Dictionary<int, int>();
            Dictionary<int, int> maxRowsHeight = new Dictionary<int, int>();


            maxRows = this.Keys.Max();

            foreach (KeyValuePair<int, Dictionary<int, Widthable>> rowCells in this)
            {
                foreach(KeyValuePair<int, Widthable> cell in rowCells.Value){
                    auxRow = rowCells.Key;
                    auxCol = cell.Key;

                    if (maxColsWidth.ContainsKey(auxCol))
                    {
                        maxColsWidth[auxCol] = Math.Max(maxColsWidth[auxCol], cell.Value.Width);
                    }
                    else 
                    {
                        maxColsWidth.Add(auxCol, cell.Value.Width);
                    };


                    if (maxRowsHeight.ContainsKey(auxRow))
                    {
                        maxRowsHeight[auxRow] = Math.Max(maxRowsHeight[auxRow], cell.Value.Height);
                    }
                    else
                    {
                        maxRowsHeight.Add(auxRow, cell.Value.Height);
                    };
                }
            }

            //Sets the columns Width to max width found on each one
            colsWidth = maxColsWidth;
            rowsHeight = maxRowsHeight;

            foreach (KeyValuePair<int, Dictionary<int, Widthable>> rowCells in this)
            {
                foreach (KeyValuePair<int, Widthable> cell in rowCells.Value)
                {
                    auxRow = rowCells.Key;
                    auxCol = cell.Key;
                    cell.Value.SetPosition(calcColOffset(auxCol), calcRowOffset(auxRow));
                }
            }

        }

        private int calcColOffset(int column)
        {
            int offsetVal=0;

            for (int i = 0; i < column; i++)
            {
                if(colsWidth.ContainsKey(i)){
                    offsetVal += colsWidth[i];
                }
            }
            return originX+offsetVal;
        }

        private int calcRowOffset(int row)
        {
            int offsetVal = 0;

            for (int i = 0; i < row; i++)
            {
                if (rowsHeight.ContainsKey(i))
                {
                    offsetVal += rowsHeight[i];
                }
            }
            return originY+offsetVal;
        }

    }
}
