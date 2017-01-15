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
using System.Windows;

namespace Autodesk.Forge.WpfCsharp {

	public partial class DownloadProgress : Window {
		protected string _key ;
		private IProgress<ProgressInfo> _progressIndicator ;

		protected DownloadProgress () {
			InitializeComponent () ;
		}

		public DownloadProgress (string key) {
			_key =key ;
			InitializeComponent () ;
		}

		#region Job Progress tasks
		public void ReportProgress (ProgressInfo value) {
			progressBar.Value =value.pct ;
			progressMsg.Content =value.msg ;
			progressBar.IsIndeterminate =(value.pct != 0 && value.pct != 100) ;
		}

		#endregion

		#region Window events
		private void Window_Loaded (object sender, RoutedEventArgs e) {
			label.Content =_key ;
			ReportProgress (new ProgressInfo (1, "Downloading file from the server...")) ;
			_progressIndicator =new Progress<ProgressInfo> (ReportProgress) ;
		}

		private void Button_Click (object sender, RoutedEventArgs e) {
			Close () ;
		}

		#endregion

	}

}
