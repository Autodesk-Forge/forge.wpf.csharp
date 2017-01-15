//
// Copyright (c) 2017 Autodesk, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// by Cyrille Fauvel
// Autodesk Forge Partner Development
//
using System;
using System.Linq;
using System.IO;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO.Compression;

namespace Autodesk.Forge.WpfCsharp {

	public class ImageSourceConverter : IValueConverter {

		public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
			if (   value == null || !(value is string)
				|| (value as string).Substring (0, 7) == @"Images\"
				|| File.Exists (value as string)
				|| (value as string).Substring (0, 4).ToLower () == "http"
				|| (value as string).Substring (0, 3).ToLower () == "ftp"
			)
				return (value) ;

			string [] sts =(value as string).Split (':') ;
			if ( sts.Length == 3 ) {
				sts [1] =sts [0] + ":" + sts [1] ;
				sts =sts.Where (w => w != sts [0]).ToArray () ;
			}
			BitmapImage bitmap =null ;
			if ( File.Exists (sts [0]) ) {
				FileStream zipStream =File.OpenRead (sts [0]) ;
				using ( ZipArchive zip =new ZipArchive (zipStream) ) {
					ZipArchiveEntry icon =zip.GetEntry (sts [1]) ;
					Stream imgStream =icon.Open () ;
					Byte [] buffer =new Byte [icon.Length] ;
					imgStream.Read (buffer, 0, buffer.Length) ;
					var byteStream =new System.IO.MemoryStream (buffer) ;
					bitmap =new BitmapImage () ;
					bitmap.BeginInit () ;
					bitmap.CacheOption =BitmapCacheOption.OnLoad ;
					bitmap.StreamSource =byteStream ;
					bitmap.EndInit () ;
				}
			}
			BitmapSource source =bitmap ;
			if (   bitmap.Format == PixelFormats.Bgra32
				|| bitmap.Format == PixelFormats.Prgba64
				|| bitmap.Format == PixelFormats.Rgba128Float
				|| bitmap.Format == PixelFormats.Rgba64
			)
				source =AutoCropBitmap (bitmap) as BitmapSource ;

			TransformedBitmap scaledBitmap =new TransformedBitmap () ;
			scaledBitmap.BeginInit () ;
			scaledBitmap.Source =source ;
			double scale =100 / source.Width ;
			scaledBitmap.Transform =new ScaleTransform (scale, scale, source.Width / 2, source.Height / 2) ;
			scaledBitmap.EndInit () ;

			return (scaledBitmap) ;
		}

		public object ConvertBack (object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture) {
			return (value) ;
		}

		// http://wpftutorial.net/Images.html
		public static ImageSource AutoCropBitmap (BitmapSource source) {
			if ( source == null )
				throw new ArgumentException ("source") ;

			if ( source.Format != PixelFormats.Bgra32 )
				source =new FormatConvertedBitmap (source, PixelFormats.Bgra32, null, 0) ;

			int width =source.PixelWidth ;
			int height =source.PixelHeight ;
			int bytesPerPixel =source.Format.BitsPerPixel / 8 ;
			int stride =width * bytesPerPixel ;

			var pixelBuffer =new byte [height * stride] ;
			source.CopyPixels (pixelBuffer, stride, 0) ;

			int cropTop =height, cropBottom =0, cropLeft =width, cropRight =0 ;
			for ( int y =0 ; y < height ; y++ ) {
				for ( int x =0 ; x < width ; x++ ) {
					int offset =(y * stride + x * bytesPerPixel) ;
					byte blue =pixelBuffer [offset] ;
					byte green =pixelBuffer [offset + 1] ;
					byte red =pixelBuffer [offset + 2] ;
					byte alpha =pixelBuffer [offset + 3] ;

					// Define a threshold when a pixel has a content
					bool hasContent =alpha > 10 ;
					if ( hasContent ) {
						cropLeft =Math.Min (x, cropLeft) ;
						cropRight =Math.Max (x, cropRight) ;
						cropTop =Math.Min (y, cropTop) ;
						cropBottom =Math.Max (y, cropBottom) ;
					}
				}
			}
			return (new CroppedBitmap (source, new Int32Rect (cropLeft, cropTop, cropRight - cropLeft, cropBottom - cropTop))) ;
		}

	}

}
