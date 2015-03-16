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
		private double principalCurvatureThreshold = 10;
		private double contrastThreshold = 0.03;
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
			LocalizeKeyPoints();
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
								if ((keypoint.X >> o) == x && (keypoint.Y >> o) == y) {
									bufBool = true;
								}
							}
							if (bufBool) {
								continue;
							}
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
				Console.WriteLine();
			}
			Console.WriteLine("    Succsess searching KeyPoints from DoG\n" +
			                  "    -----------------------------------------------");
			Console.WriteLine("Step 1 Complete!!\n"
			                  + "now keypoints is " + keypoints.Count() + " point");
		}

		private void LocalizeKeyPoints() {
			Console.WriteLine("Step 2 : keypoints localization");
			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start localize with principal curvature");

			Console.WriteLine("    Succsess localize with principal curvature\n" +
			                  "    -----------------------------------------------");

			Console.WriteLine("    -----------------------------------------------\n" +
			                  "    Start localize with contrast");
			double[,] mD = new double[3, 3]; //導関数行列
			double[,] iD = new double[3, 3]; //の逆行列用
			double[] xD = new double[3]; //キーポイント位置
			double[] X = new double[3]; //サブピクセル位置
			List<KeyPoint> remobeKeys=new List<KeyPoint>();
			foreach (var key in keypoints) {
				int o = key.Octave;
				int s = key.Scale;
				int x = key.X;
				int y = key.Y;

				int sm1 = (s - 1 < 0) ? 0 : s - 1;
				int sm2 = (s - 2 < 0) ? 0 : s - 2;
				int sp1 = (s + 1 >= scale + 2) ? scale + 1 : s + 1;
				int sp2 = (s + 2 >= scale + 2) ? scale + 1 : s + 2;

				if (x < 2 || y < 2 || x >= DoG[o][s].GetLength(0) - 2 || y >= DoG[o][s].GetLength(1) - 2) {
					continue;
				}

				//サブピクセル推定
				double Dx = (DoG[o][s][x - 1, y] - DoG[o][s][x + 1, y]);
				double Dy = (DoG[o][s][x, y - 1] - DoG[o][s][x, y + 1]);
				double Dxx = (DoG[o][s][x - 2, y] + DoG[o][s][x + 2, y] - 2 * DoG[o][s][x, y]);
				double Dyy = (DoG[o][s][x, y - 2] + DoG[o][s][x, y + 2] - 2 * DoG[o][s][x, y]);
				double Dxy = (DoG[o][s][x - 1, y - 1] - DoG[o][s][x - 1, y + 1]
				              - DoG[o][s][x + 1, y - 1] + DoG[o][s][x + 1, y + 1]);
				double Ds = (DoG[o][sm1][x, y] - DoG[o][sp1][x, y]);

				double Dss = (DoG[o][sm2][x, y] - DoG[o][sp2][x, y] + 2 * DoG[o][s][x, y]);
				double Dxs = (DoG[o][sm1][x - 1, y] - DoG[o][sm1][x + 1, y]
				              + DoG[o][sp1][x - 1, y] - DoG[o][sp1][x + 1, y]);
				double Dys = (DoG[o][sm1][x, y - 1] - DoG[o][sm1][x, y + 1]
				              + DoG[o][sp1][x, y - 1] - DoG[o][sp1][x, y + 1]);

				mD[0, 0] = Dxx;
				mD[0, 1] = Dxy;
				mD[0, 2] = Dxs;
				mD[1, 0] = Dxy;
				mD[1, 1] = Dyy;
				mD[1, 2] = Dys;
				mD[2, 0] = Dxs;
				mD[2, 1] = Dys;
				mD[2, 2] = Dss;

				xD[0] = -Dx;
				xD[1] = -Dy;
				xD[2] = -Ds;

				//逆行列計算(mDの逆行列をiDに)
				iD = calcMatrixInverth(mD);

				//サブピクセル位置(行列の積)
				X[0] = iD[0, 0] * xD[0] + iD[0, 1] * xD[1] + iD[0, 2] * xD[2];
				X[1] = iD[1, 0] * xD[0] + iD[1, 1] * xD[1] + iD[1, 2] * xD[2];
				X[2] = iD[2, 0] * xD[0] + iD[2, 1] * xD[1] + iD[2, 2] * xD[2];

				//サブピクセル位置での出力(式21)
				double Dpow = Math.Abs(DoG[o][s][x, y] + (xD[0] * X[0] + xD[1] * X[1] + xD[2] * X[2]) / 2);

				//閾値処理
				if (Dpow < contrastThreshold + 127) {
					remobeKeys.Add(key);
				}
			}

			foreach (var remobeKey in remobeKeys) {
				keypoints.Remove(remobeKey);
			}
			Console.WriteLine("    Succsess localize with contrast\n" +
			                  "    -----------------------------------------------");
			Console.WriteLine("now keypoints is " + keypoints.Count() + " point");
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

		private double[,] calcMatrixInverth(double[,] mat) {
			int i, j, k;
			double buf;
			double[,] inv=new double[3,3];

			//初期化
			for (i = 0; i < 3; i++) {
				for (j = 0; j < 3; j++) {
					inv[i, j] = 0;
				}
				inv[i, i] = 1;
			}

			//掃き出し法
			for (i = 0; i < 3; i++) {
				buf = 1 / mat[i, i];
				for (j = 0; j < 3; j++) {
					mat[i, j] *= buf;
					inv[i, j] *= buf;
				}

				for (j = 0; j < 3; j++) {
					if (i != j) {
						buf = mat[j, i];
						for (k = 0; k < 3; k++) {
							mat[j, k] -= mat[i, k] * buf;
							inv[j, k] -= inv[i, k] * buf;
						}
					}
				}
			}
			return inv;
		}
	}
}