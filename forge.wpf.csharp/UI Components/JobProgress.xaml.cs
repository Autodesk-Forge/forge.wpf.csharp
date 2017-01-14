using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

using Autodesk.Forge;
using Autodesk.Forge.Client;
using System.Text.RegularExpressions;

namespace Autodesk.Forge.WpfCsharp {

	public delegate void JobCompletedDelegate (ForgeObjectInfo item) ;

	/// <summary>
	/// Interaction logic for JobProgress.xaml
	/// </summary>
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
						item.TranslationRequested =false ;
						if ( _callback != null )
							this.Dispatcher.Invoke (_callback, new Object [] { item }) ;
						break ;
					}
				} catch ( Exception /*ex*/ ) {
				}
			}
		}

		protected async Task<ProgressInfo> JobProgressRequest (ForgeObjectInfo item) {
			DerivativesApi md =new DerivativesApi () ;
			md.Configuration.AccessToken =_accessToken ;
			string urn =MainWindow.URN (item.Properties.bucketKey, item) ;
			ApiResponse<dynamic> response =await md.GetManifestAsyncWithHttpInfo (urn) ;
			if ( MainWindow.httpErrorHandler (response, "Failed to get manifest") )
				return (new ProgressInfo (0, "Initializing...")) ;
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
		}

		#endregion

		#region Window events
		private async void Window_Loaded (object sender, RoutedEventArgs e) {
			sceneid.Content =_item.Name ;
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
