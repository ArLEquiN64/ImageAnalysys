﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageAnalyser {
	public class ImageController {
		private readonly string _filePath;
		private readonly string _filename;
		private readonly string _outputPath;
		private const string DefaultPath = @"D:\Data\Projects\ImageAnalysys\color\Lenna.bmp";
		private readonly Bitmap _originalImg;
		private readonly Color[,] _originalPixelData;
		private double[,] _grayScaleData;
		public double[,] GrayScaleData {
			get { return _grayScaleData; }
		}
		private readonly int _width;

		public int width {
			get { return _width; }
		}
		private readonly int _height;
		public int height {
			get { return _height; }
		}


		public ImageController(string filePath = DefaultPath) {
			Console.Write("opening file...");
			_filePath = filePath.Substring(0, filePath.LastIndexOf('\\') + 1);
			_filename = filePath.Substring(filePath.LastIndexOf('\\') + 1);
			_outputPath = _filePath + DateTime.Now.Ticks + @"\";
			Directory.CreateDirectory(_outputPath);
			_originalImg = new Bitmap(_filePath + _filename);
			_width = _originalImg.Width;
			_height = _originalImg.Height;
			_originalPixelData = new Color[width, height];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					_originalPixelData[x, y] = _originalImg.GetPixel(x, y);
				}
			}
			Console.WriteLine("succsess : " + _filename);
			MakeGrayScale();
		}

		public void SaveToFile(string fileName, Color[,] newPixelData) {
			Console.Write("saveing...");
			var outputPath = _outputPath + _filename.Insert(_filename.LastIndexOf('.'), "_" + fileName);
			if (File.Exists(outputPath)) {
				/*
				Console.WriteLine("\nError : " + fileName + "is already exists.\n");
				return
				 */
				int i = 0;
				while (File.Exists(outputPath.Insert(outputPath.LastIndexOf('.'), outputPath+" - " + i))) {
					i++;
				}
				outputPath = outputPath.Insert(outputPath.LastIndexOf('.'), outputPath + " - " + i);
			}
			int w = newPixelData.GetLength(0);
			int h = newPixelData.GetLength(1);
			using (var saveImg = new Bitmap(w, h)) {
				for (int y = 0; y < h; y++) {
					for (int x = 0; x < w; x++) {
						saveImg.SetPixel(x, y, newPixelData[x, y]);
					}
				}
				saveImg.Save(outputPath, ImageFormat.Bmp);
				Console.WriteLine("succsess : " + outputPath.Substring(outputPath.LastIndexOf('\\') + 1));
			}
		}

		public void SaveToFile(string fileName, double[,] newPixelData) {
			Console.Write("exchange to Color...");
			int w = newPixelData.GetLength(0);
			int h = newPixelData.GetLength(1);
			var saveImg = new Color[w, h];
			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					saveImg[x, y] = Color.FromArgb(255, (int) newPixelData[x, y], (int) newPixelData[x, y], (int) newPixelData[x, y]);
				}
			}
			SaveToFile(fileName, saveImg);
		}

		private void MakeGrayScale() {
			Console.Write("Makeing Gray Scale...");
			int width = _originalPixelData.GetLength(0);
			int height = _originalPixelData.GetLength(1);
			_grayScaleData = new double[width, height];
			for (var y = 0; y < height; y++) {
				for (var x = 0; x < width; x++) {
					_grayScaleData[x, y] = (int) (_originalPixelData[x, y].GetBrightness() * 255);
				}
			}
			Console.WriteLine("succsess");
			SaveToFile("GrayScale", _grayScaleData);
		}
	}
}