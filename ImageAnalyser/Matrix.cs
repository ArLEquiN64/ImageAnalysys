using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalyser
{
    class ImageMatrix
    {
        double[,] matrix;

        public int colum_length { get; set; }//列、横の長さ
        public int row_length{ get; set; }//行、縦の長さ

        public ImageMatrix(int colum_length,int row_length)
        {
            matrix = new double[colum_length, row_length];
        }

        public ImageMatrix(double[,] Image)
        {
            this.matrix = Image;
            int colum_length = Image.GetLength(0);
            int row_length = Image.GetLength(1);
        }

        public double[,] GetMatrix()
        {
            return matrix;
        }

        public double GetElement(int x,int y)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x > row_length)
                x = row_length;
            if (y > colum_length)
                y = colum_length;

            return matrix[x, y];
        }

        public void SetElement(int x, int y,double element)
        {
            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;
            if (x > row_length)
                x = row_length;
            if (y > colum_length)
                y = colum_length;

            matrix[x, y] = element;
        }
    }
}
