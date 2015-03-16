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
		private readonly int scale = 3;
		private readonly int minWidth = 10;
		private int octave;
		private int width;
		private int height;
		private double[][][,] L;
		private double[][][,] DoG;
		private List<KeyPoint> keypoints = new List<KeyPoint>();

		private struct KeyPoint {
			public KeyPoint(int x, int y, int octave, int scale) : this() {
				X = x;
				Y = y;
				Octave = octave;
				Scale = scale;
			}

			public int X { get; private set; }
			public int Y { get; private set; }
			public int Octave { get; private set; }
			public int Scale { get; private set; }
		}

		public SIFT(ImageController ic) {
			this.ic = ic;
			width = ic.GrayScaleData.GetLength(0);
			height = ic.GrayScaleData.GetLength(1);

			int W = (width < height) ? width : height;
			for (octave = 0; W > minWidth; octave++, W /= 2) {
			}
			L = new double[octave][][,];
			DoG = new double[octave][][,];
			for (int i = 0; i < octave; i++) {
				L[i] = new double[scale + 3][,];
				DoG[i] = new double[scale + 2][,];
			}
		}

		public void DoSIFT() {
			Console.WriteLine("~~~~~  SIFT START  ~~~~~");
			GetKeyPoints();
		}

		private void GetKeyPoints() {
			Console.WriteLine("Step 1 : Keypoint detection");
			double k = Math.Pow(2, 1 / (double) scale);

			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start makeing Level Images");
			double[,] imgBuf = ic.GrayScaleData;
			for (int i = 0; i < octave; i++) {
				Console.WriteLine("    octave " + i + " --------------------");
				double sigBuf = sigma;
				for (int j = 0; j < scale + 3; j++) {
					Console.Write("    --  " + j + " : ");
					L[i][j] = new double[width, height];
					L[i][j] = new GaussianFilter().Filter(imgBuf, sigBuf);
					ic.SaveToFile("L_" + i + j, L[i][j]);
					sigBuf *= k;
				}
				imgBuf = new DownSampling().Filter(imgBuf);
				Console.WriteLine();
			}
			Console.WriteLine("    Succsess makeing Level Images\n" +
			                  "    -----------------------------------------------");

			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start makeing Difference of Gaussian Images (DoG)");
			for (int i = 0; i < octave; i++) {
				Console.WriteLine("    octave " + i + " --------------------");
				for (int j = 0; j < scale + 2; j++) {
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

			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start searching KeyPoints from DoG");
			for (int o = 0; o < octave; o++) {
				Console.Write("  octave : " + o);
				int w = DoG[o][0].GetLength(0);
				int h = DoG[o][0].GetLength(1);

				for (int s = 1; s < scale + 1; s++) {
					Console.Write("  scale : " + s);
					for (int x = 0; x < w; x++) {
						for (int y = 0; y < h; y++) {
							bool bufBool = false;
							foreach (var keypoint in keypoints) {
								if (keypoint.X == x && keypoint.Y == y) {
									bufBool = true;
								}
							}
							if (bufBool) {
								continue;
							}
							else {
								bool isMax = true;
								bool isMin = true;
								for (int ds = s - 1; ds < s + 1; ds++) {
									for (int dx = x - 1; dx < x + 1; dx++) {
										for (int dy = y - 1; dy < y + 1; dy++) {
											if (dx == -1 || dy == -1 || dx >= w || dy >= h || (ds == s && dx == x && dy == y)) {
												continue;
											}

											if (isMax && DoG[o][s][x, y] <= DoG[o][ds][dx, dy]) {
												isMax = false;
											}
											if (isMin && DoG[o][s][x, y] >= DoG[o][ds][dx, dy]) {
												isMin = false;
											}
											if (!isMax && !isMin) {
												goto next;
											}
										}
									}
								}
								var point = new KeyPoint(x, y, o, s);
								keypoints.Add(point);
								next:
								;
							}
						}
					}
				}
				Console.WriteLine();
			}
			Console.WriteLine("    Succsess searching KeyPoints from DoG\n" +
			                  "    -----------------------------------------------");
			Console.WriteLine("Step 1 Complete!!\n"
			                  + "now keypoints is " + keypoints.Count() + " point");
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

        public void LocalizePrincipalCurvatures(ImageMatrix inData, double gamma)
        {
            ImageMatrix TrH =
                ImageController.DifferentiationOfColum(ImageController.DifferentiationOfColum(inData)) +
                ImageController.DifferentiationOfRow(ImageController.DifferentiationOfRow(inData));
            ImageMatrix DetH =
                (ImageController.DifferentiationOfColum(ImageController.DifferentiationOfColum(inData)) *
                ImageController.DifferentiationOfRow(ImageController.DifferentiationOfRow(inData))) -
                (ImageController.DifferentiationOfColum(ImageController.DifferentiationOfRow(inData)) *
                ImageController.DifferentiationOfColum(ImageController.DifferentiationOfRow(inData)));

            for (int y = 0; y < TrH.row_length; y++)
            {
                for (int x = 0; x < TrH.colum_length; x++)
                {
                    //要改善
                    if (Math.Pow(TrH.GetElement(x, y), 2) / DetH.GetElement(x, y) < Math.Pow((gamma + 1), 2) / gamma)
                        keypoints.Remove(new KeyPoint(x,y,octave,scale));
                }
            }
        }
	}
}
