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
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Text.RegularExpressions;

using Autodesk.Forge;
using Autodesk.Forge.Client;

namespace Autodesk.Forge.WpfCsharp {

	public delegate void JobCompletedDelegate (ForgeObjectInfo item) ;

	public partial class JobProgress : Window {
		public JobCompletedDelegate _callback =null ;
		protected CancellationTokenSource _cts =null ;
		protected ForgeObjectInfo _item ;
		protected string _accessToken ;

		protected JobProgress () {
			InitializeComponent () ;
		}

		public JobProgress (ForgeObjectInfo item, string accessToken) {
			_item =item ;
			_accessToken =accessToken ;
			InitializeComponent () ;
		}

		#region Job Progress tasks
		private void ReportProgress (ProgressInfo value) {
			progressBar.Value =value.pct ;
			progressMsg.Content =value.msg ;
		}

		private async Task JobProgressTask (ForgeObjectInfo item, IProgress<ProgressInfo> progress, CancellationToken ct, TaskScheduler uiScheduler) {
			progress.Report (new ProgressInfo (0, "Initializing...")) ;
			while ( !ct.IsCancellationRequested ) {
				try {
					ProgressInfo info =await JobProgressRequest (item) ;
					if ( info == null ) {
						progress.Report (new ProgressInfo (0, "Error")) ;
						break ;
					}
					progress.Report (info) ;
					if ( info.pct >= 100 || info.msg == "success" ) {
						item.TranslationRequested =StateEnum.Busy ;
						if ( _callback != null )
							this.Dispatcher.Invoke (_callback, new Object [] { item }) ;
						break ;
					}
				} catch ( Exception /*ex*/ ) {
				}
			}
		}

		protected async Task<ProgressInfo> JobProgressRequest (ForgeObjectInfo item) {
			try {
				DerivativesApi md =new DerivativesApi () ;
				md.Configuration.AccessToken =_accessToken ;
				string urn =MainWindow.URN (item.Properties.bucketKey, item) ;
				ApiResponse<dynamic> response =await md.GetManifestAsyncWithHttpInfo (urn) ;
				MainWindow.httpErrorHandler (response, "Initializing...") ;
				item.Manifest =response.Data ;
				int pct =100 ;
				if ( response.Data.progress != "complete" ) {
					try {
						string st =response.Data.progress ;
						Regex rgx =new Regex ("[^0-9]*") ;
						st =rgx.Replace (st, "") ;
						pct =int.Parse (st) ;
					} catch ( Exception ) {
						pct =0 ;
					}
				}
				string msg =response.Data.status ;
				return (new ProgressInfo (pct, msg)) ;
			} catch ( Exception /*ex*/ ) {
				return (new ProgressInfo (0, "Initializing...")) ;
			}
		}

		#endregion

		#region Window events
		private async void Window_Loaded (object sender, RoutedEventArgs e) {
			label.Content =_item.Name ;
			TaskScheduler uiScheduler =TaskScheduler.FromCurrentSynchronizationContext () ;
			var progressIndicator =new Progress<ProgressInfo> (ReportProgress) ;
			_cts =new CancellationTokenSource () ;
			try {
				await JobProgressTask (_item, progressIndicator, _cts.Token, uiScheduler) ;
			} catch ( OperationCanceledException /*ex*/ ) {
				//Trace.WriteLine (ex.Message, "Exception") ;
			}
		}

		private void Button_Click (object sender, RoutedEventArgs e) {
			_cts.Cancel () ;
			Close () ;
		}

		#endregion

	}

}
