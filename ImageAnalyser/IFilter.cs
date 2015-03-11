using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnalyser {
	internal interface IFilter<T> {
		T[,] Filter(T[,] inData,params double[] parameter);
	}
}