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
            this.colum_length = colum_length;
            this.row_length = row_length;
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

        public static ImageMatrix operator +(ImageMatrix a, ImageMatrix b)
        {
            if (a.row_length != b.row_length || a.colum_length != b.colum_length)
                return a;

            ImageMatrix result = new ImageMatrix(a.colum_length, a.row_length);
            for (int y = 0; y < a.row_length; y++)
            {
                for (int x = 0; x < a.colum_length; x++)
                {
                    result.SetElement(x, y, a.GetElement(x, y) + b.GetElement(x, y));
                }
            }
            return result;
        }

        public static ImageMatrix operator -(ImageMatrix a, ImageMatrix b)
        {
            if (a.row_length != b.row_length || a.colum_length != b.colum_length)
                return a;

            ImageMatrix result = new ImageMatrix(a.colum_length, a.row_length);
            for (int y = 0; y < a.row_length; y++)
            {
                for (int x = 0; x < a.colum_length; x++)
                {
                    result.SetElement(x, y, a.GetElement(x, y) - b.GetElement(x, y));
                }
            }
            return result;
        }

        public static ImageMatrix operator *(ImageMatrix a, ImageMatrix b)
        {
            if (a.colum_length != b.row_length)
                return a;

            ImageMatrix result = new ImageMatrix(b.colum_length, a.row_length);
            for (int y = 0; y < b.colum_length; y++)
            {
                for (int x = 0; x < a.row_length; x++)
                {
                    double temp=0;
                    for (int i = 0; i < a.colum_length; i++)
                    {
                        temp+=a.GetElement(i,y)*b.GetElement(x,i);
                    }
                    result.SetElement(x, y, temp);
                }
            }
            return result;
        }
    }
}
