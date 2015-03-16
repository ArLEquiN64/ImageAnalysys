using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalyser {
	internal class SIFT {
		private ImageController ic;
		private readonly double sigma = 1.6;
		private readonly int s = 3;
		private readonly int minW = 10;
		private int oct;
		private int width;
		private int height;
		private double[][][,] L;
		private double[][][,] DoG;

		struct KeyPoint {
			private int x;
			private int y;
			private int octave;
			private int scale;
		}

		public SIFT(ImageController ic) {
			this.ic = ic;
			width = ic.GrayScaleData.GetLength(0);
			height = ic.GrayScaleData.GetLength(1);

			int W = (width < height) ? width : height;
			for (oct = 0; W > minW; oct++, W /= 2) {
			}
			L = new double[oct][][,];
			DoG = new double[oct][][,];
			for (int i = 0; i < oct; i++) {
				L[i] = new double[s + 3][,];
				DoG[i] = new double[s + 2][,];
			}
		}

		public void DoSIFT() {
			Console.WriteLine("~~~~~  SIFT START  ~~~~~");
			GetKeyPoints();
		}

		private void GetKeyPoints() {
			Console.WriteLine("Step 1 : Keypoint detection");
			double k = Math.Pow(2, 1 / (double) s);

			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start makeing Level Images");
			double sigBuf = sigma;
			double[,] imgBuf = ic.GrayScaleData;
			for (int i = 0; i < oct; i++) {
				Console.WriteLine("    octave " + i + " --------------------");
				for (int j = 0; j < s + 3; j++) {
					Console.Write("    --  " + j + " : ");
					L[i][j] = new double[width, height];
					L[i][j] = new GaussianFilter().Filter(imgBuf, sigBuf);
					sigBuf*= k;
				}
				imgBuf = new DownSampling().Filter(imgBuf);
				Console.WriteLine();
			}
			Console.WriteLine("    Succsess makeing Level Images\n" +
			                  "    -----------------------------------------------");

			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start makeing Difference of Gaussian Images");
			for (int i = 0; i < oct; i++) {
				Console.WriteLine("    octave " + i + " --------------------");
				for (int j = 0; j < s + 2; j++) {
					Console.Write("    --  " + j + " : ");
					int w = (L[i][j + 1].GetLength(0) < L[i][j].GetLength(0)) ? L[i][j + 1].GetLength(0) : L[i][j].GetLength(0);
					int h = (L[i][j + 1].GetLength(1) < L[i][j].GetLength(1)) ? L[i][j + 1].GetLength(1) : L[i][j].GetLength(1);
					DoG[i][j] = new double[w, h];
					DoG[i][j] = DifferenceImg(L[i][j + 1], L[i][j], w, h);
					ic.SaveToFile("DoG_" + i + j, DoG[i][j]);
				}
				Console.WriteLine();
			}
			Console.WriteLine("    Succsess makeing Difference of Gaussian Images (DoG)\n" +
							  "    -----------------------------------------------");
		}

		private double[,] DifferenceImg(double[,] u, double[,] v, int w, int h) {
			double[,] outData = new double[w, h];
			for (int i = 0; i < w; i++) {
				for (int j = 0; j < h; j++) {
					outData[i, j] = u[i, j] - v[i, j] + 127;
				}
			}
			return outData;
		}
	}
}