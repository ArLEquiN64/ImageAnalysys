using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageAnalyser {
	public class ImageController {
		private static ImageController _instance;
		private readonly string _filePath;
		private readonly string _filename;
		private readonly string _outputPath;
		private const string DefaultPath = @"D:\Data\Projects\ImageAnalysys\color\Lenna.bmp";
		private readonly Bitmap _originalImg;
		private readonly Color[,] _originalPixelData;
		private double[,] _grayScaleData;

		private ImageController(string filePath = DefaultPath) {
			Console.Write("opening file...");
			_filePath = filePath.Substring(0, filePath.LastIndexOf('\\') + 1);
			_filename = filePath.Substring(filePath.LastIndexOf('\\') + 1);
			_outputPath = _filePath + DateTime.Now.Ticks + @"\";
			Directory.CreateDirectory(_outputPath);
			_originalImg = new Bitmap(_filePath + _filename);
			int w = _originalImg.Width;
			int h = _originalImg.Height;
			_originalPixelData = new Color[w, h];
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					_originalPixelData[x, y] = _originalImg.GetPixel(x, y);
				}
			}
			Console.WriteLine("succsess : " + _filename);
			MakeGrayScale();
		}

		public static ImageController Instance {
			get { return _instance ?? (_instance = new ImageController()); }
		}

		public void SaveToFile(string fileName, Color[,] newPixelData) {
			Console.Write("saveing...");
			int width = newPixelData.GetLength(0);
			int height = newPixelData.GetLength(1);
			using (var saveImg = new Bitmap(width, height)) {
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						saveImg.SetPixel(x, y, newPixelData[x, y]);
					}
				}

				var outputPath = _outputPath + DateTime.Now.Ticks + "_" + _filename.Insert(_filename.LastIndexOf('.'), "_" + fileName);
				saveImg.Save(outputPath, ImageFormat.Bmp);
				Console.WriteLine("succsess : " + _filename.Insert(_filename.LastIndexOf('.'), "_" + fileName));
			}
		}

		public void SaveToFile(string fileName, double[,] newPixelData) {
			Console.Write("saveing...");
			int width = newPixelData.GetLength(0);
			int height = newPixelData.GetLength(1);
			using (var saveImg = new Bitmap(width, height)) {
				for (int y = 0; y < height; y++) {
					for (int x = 0; x < width; x++) {
						saveImg.SetPixel(x, y,
							Color.FromArgb(255, (int)newPixelData[x, y], (int)newPixelData[x, y], (int)newPixelData[x, y]));
					}
				}

				var outputPath = _outputPath + DateTime.Now.Ticks + "_" + _filename.Insert(_filename.LastIndexOf('.'), "_" + fileName);
				saveImg.Save(outputPath, ImageFormat.Bmp);
				Console.WriteLine("succsess : " + _filename.Insert(_filename.LastIndexOf('.'), "_" + fileName));
			}
		}

		public void MakeGrayScale() {
			Console.Write("Makeing Gray Scale...");
			int width = _originalPixelData.GetLength(0);
			int height = _originalPixelData.GetLength(1);
			var _grayScaleData = new double[width, height];
			for (var y = 0; y < height; y++) {
				for (var x = 0; x < width; x++) {
					_grayScaleData[x, y] = (int)(_originalPixelData[x, y].GetBrightness() * 255);
				}
			}
			Console.WriteLine("succsess");
			SaveToFile("GrayScale", _grayScaleData);
		}
	}
}