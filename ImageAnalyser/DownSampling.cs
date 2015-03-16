using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalyser {
	class DownSampling:IFilter<double> {
		public double[,] Filter(double[,] inData, params double[] parameter) {
			int w = inData.GetLength(0) / 2;
			int h = inData.GetLength(1) / 2;
			double[,] outData = new double[w, h];
			for (int x = 0; x < w; x++) {
				for (int y = 0; y < h; y++) {
					double tmp = inData[x * 2, y * 2];
					if (x == w - 1 && y != h - 1) {
						tmp += inData[x * 2, y * 2 + 1];
					}
					else if (x != w - 1 && y == h - 1) {
						tmp += inData[x * 2 + 1, y * 2];
					}
					else if (x == w - 1 && y == h - 1) {

					}
					else {
						tmp += inData[x * 2, y * 2 + 1] + inData[x * 2 + 1, y * 2] + inData[x * 2 + 1, y * 2 + 1];
					}

					outData[x, y] = tmp;
				}
			}
			return outData;
		}
	}
}
