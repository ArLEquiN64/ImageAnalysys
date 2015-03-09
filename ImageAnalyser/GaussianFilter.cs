using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalyser {
	class GaussianFilter:IGrayscaleFilter {

		public double[,] Filter(double[,] inData, double sigma) {
			Console.Write("Filtering Gaussian...");
			int width = inData.GetLength(0);
			int height = inData.GetLength(1);
			int r = ((int)(Math.Ceiling(3.0 * sigma) * 2 + 1) - 1) / 2;

			var outData = new double[width, height];

			double[,] tmpX = new double[width, height];
			double[,] tmpY = new double[width, height];

			double coefficientG = 2 * Math.PI * Math.Pow(sigma, 2);

			double[] convolution = new double[2 * r + 1];

			int absRange = 2 * r + 1;

			for (int i = 0; i < absRange; i++) {
				int sub = i - r;

				double gTmp = -1 * ((Math.Pow((sub), 2) / (2 * Math.Pow(sigma, 2))));
				double gauss = (1 / coefficientG) * Math.Pow(Math.E, gTmp);

				convolution[i] = gauss * 2;
			}

			double sum = 0;

			for (int i = 0; i < absRange; i++) {
				sum = sum + convolution[i];
			}

			for (int i = 0; i < absRange; i++) {
				convolution[i] = convolution[i] / sum;
			}

			//各方向にガウスフィルタを加える

			#region

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					double buf = 0;

					for (int m = 0; m < absRange; m++) {
						int sub = m - r;
						int pX = x + sub;

						if (pX > 0 && pX < width) {
							double bufTmp = inData[pX, y] * convolution[m];
							buf = buf + bufTmp;
						}
					}
					tmpX[x, y] = buf;
				}
			}

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					double buf = 0;

					for (int m = 0; m < absRange; m++) {
						int sub = m - r;
						int pY = y + sub;

						if (pY > 0 && pY < height) {
							double bufTmp = tmpX[x, pY] * convolution[m];
							buf = buf + bufTmp;
						}
					}
					tmpY[x, y] = buf;
				}
			}

			#endregion

			//画像の正規化

			#region

			double max = double.MinValue;
			double min = double.MaxValue;

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (max < tmpY[x, y]) {
						max = tmpY[x, y];
					}

					if (min > tmpY[x, y]) {
						min = tmpY[x, y];
					}
				}
			}

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					outData[x, y] = 255.0 * (tmpY[x, y] - min) / ((max - min));
				}
			}

			#endregion

			Console.WriteLine("succsess");
			return outData;
		}

		public double[,] Filter(double[,] inData) {
			throw new NotImplementedException("missing value sigma.");
		}
	}
}
