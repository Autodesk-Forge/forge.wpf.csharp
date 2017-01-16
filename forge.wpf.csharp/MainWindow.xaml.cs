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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;

// Extended WPF Toolkit™ Community Edition - http://wpftoolkit.codeplex.com/
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

// For Folder Browser
using Microsoft.WindowsAPICodePack.Dialogs;

using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;
using System.Runtime.CompilerServices;

namespace Autodesk.Forge.WpfCsharp {

	public enum StateEnum {
		Busy,
		Idle
	}

	public class ForgeObjectInfo : INotifyPropertyChanged {
		public string Name { get; set; }
		public string Size { get; set; }
		public string Image { get; set; }
		public dynamic _Properties =null ; // public dynamic Properties { get; set; } =null ;
		public dynamic Properties { get { return (_Properties) ; } set { SetField (ref _Properties, value) ; } } 
		//public StateEnum PropertiesRequested { get; set; } =StateEnum.Idle ;
		private StateEnum _PropertiesRequested =StateEnum.Idle ;
		public StateEnum PropertiesRequested { get { return (_PropertiesRequested) ; } set { SetField (ref _PropertiesRequested, value) ; } } 
		private dynamic _Manifest =null ; // public dynamic Manifest { get; set; } =null ;
		public dynamic Manifest { get { return (_Manifest) ; } set { SetField (ref _Manifest, value) ; } } 
		//public StateEnum ManifestRequested { get; set; } =StateEnum.Idle ;
		private StateEnum _ManifestRequested =StateEnum.Idle ;
		public StateEnum ManifestRequested { get { return (_ManifestRequested) ; } set { SetField (ref _ManifestRequested, value) ; } } 
		//public StateEnum TranslationRequested { get; set; } =StateEnum.Idle ;
		private StateEnum _TranslationRequested =StateEnum.Idle ;
		public StateEnum TranslationRequested { get { return (_TranslationRequested) ; } set { SetField (ref _TranslationRequested, value) ; } }

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged ;
		private void OnPropertyChanged ([CallerMemberName] string propertyName =null) {
			// C# 6 null-safe operator
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName)) ;
		}
		// C# 5 - CallMemberName means we don't need to pass the property's name
		protected void SetField<T> (ref T field, T value, [CallerMemberName] string propertyName =null) {
			if ( EqualityComparer<T>.Default.Equals (field, value) )
				return ;
			field =value ;
			OnPropertyChanged (propertyName) ;
		}
		
		#endregion

	}

	public partial class MainWindow : Window, INotifyPropertyChanged {
		private const string APP_NAME ="ForgeWpf" ;
		private const string DEFAULT_IMAGE =@"Images\ForgeImg.png" ;

		#region Forge Initialization
		public enum Region {
			US,
			EMEA
		} ;
		private string FORGE_CLIENT_ID ="" ; // 'your_client_id'
		private string FORGE_CLIENT_SECRET ="" ; // 'your_client_secret'
		private string PORT ="" ; // 3006
		private string FORGE_CALLBACK =null ; // 'http://localhost:' + PORT + '/oauth' ;

		private string _grantType ="client_credentials" ; // {String} Must be ``client_credentials``
		private string _scope ="data:read data:write data:create data:search bucket:create bucket:read bucket:update bucket:delete" ;
		// todo private string _scopeViewer ="data:read" ;
		private string _bucket ="" ;
		private const string BUCKET ="wpfcsharp" ;// This is a default basename, you can edit that name
		private const PostBucketsPayload.PolicyKeyEnum _bucketType =PostBucketsPayload.PolicyKeyEnum.Persistent ;

		private bool readKeys () {
			FORGE_CLIENT_ID =Environment.GetEnvironmentVariable ("FORGE_CLIENT_ID") ;
			if ( string.IsNullOrEmpty (FORGE_CLIENT_ID) )
				FORGE_CLIENT_ID =Properties.Settings.Default.FORGE_CLIENT_ID ;
			FORGE_CLIENT_SECRET =Environment.GetEnvironmentVariable ("FORGE_CLIENT_SECRET") ;
			if ( string.IsNullOrEmpty (FORGE_CLIENT_SECRET) )
				FORGE_CLIENT_SECRET =Properties.Settings.Default.FORGE_CLIENT_SECRET ;
			PORT =Environment.GetEnvironmentVariable ("PORT") ;
			if ( string.IsNullOrEmpty (PORT) )
				PORT =Properties.Settings.Default.PORT ;
			if ( string.IsNullOrEmpty (PORT) )
				PORT ="3006" ;
			if ( string.IsNullOrEmpty (FORGE_CALLBACK) ) {
				FORGE_CALLBACK =Environment.GetEnvironmentVariable ("FORGE_CALLBACK") ;
				if ( string.IsNullOrEmpty (FORGE_CALLBACK) )
					FORGE_CALLBACK =Properties.Settings.Default.FORGE_CALLBACK ;
				if ( string.IsNullOrEmpty (FORGE_CALLBACK) )
					FORGE_CALLBACK ="http://localhost:" + PORT + "/oauth" ;
			}
			_bucket =(BUCKET + FORGE_CLIENT_ID).ToLower () ;
			return (true) ;
		}

		private bool readKeys (string clientId, string clientSecret, string port, string callback, bool bSaveInSettings =false) {
			FORGE_CLIENT_ID =clientId ;
			if ( bSaveInSettings )
				Properties.Settings.Default.FORGE_CLIENT_ID =FORGE_CLIENT_ID ;
			FORGE_CLIENT_SECRET =clientSecret ;
			if ( bSaveInSettings )
				Properties.Settings.Default.FORGE_CLIENT_SECRET =FORGE_CLIENT_SECRET ;
			PORT =port ;
			if ( string.IsNullOrEmpty (PORT) )
				PORT ="3006" ;
			if ( bSaveInSettings )
				Properties.Settings.Default.PORT =PORT ;
			if ( string.IsNullOrEmpty (FORGE_CALLBACK) ) {
				FORGE_CALLBACK =callback ;
				if ( string.IsNullOrEmpty (FORGE_CALLBACK) )
					FORGE_CALLBACK ="http://localhost:" + PORT + "/oauth" ;
			}
			if ( bSaveInSettings )
				Properties.Settings.Default.FORGE_CALLBACK =FORGE_CALLBACK ;
			_bucket =(BUCKET + FORGE_CLIENT_ID).ToLower () ;
			return (true) ;
		}

		#endregion

		#region State Property
		private StateEnum _State =StateEnum.Idle ;
		public StateEnum State { get { return (_State) ; } set { SetField (ref _State, value) ; } }

		public event PropertyChangedEventHandler PropertyChanged ;
		private void OnPropertyChanged ([CallerMemberName] string propertyName =null) {
			// C# 6 null-safe operator
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName)) ;
		}
		// C# 5 - CallMemberName means we don't need to pass the property's name
		protected void SetField<T> (ref T field, T value, [CallerMemberName] string propertyName =null) {
			if ( EqualityComparer<T>.Default.Equals (field, value) )
				return ;
			field =value ;
			OnPropertyChanged (propertyName) ;
		}

		#endregion

		#region access_token
		private TwoLeggedApi _twoLeggedApi =new TwoLeggedApi () ;
		private dynamic _2legged_bearer =null ;
		protected string accessToken { get { return (_2legged_bearer != null ? _2legged_bearer.access_token : null) ; } }

		private async Task<ApiResponse<dynamic>> oauthExecAsync () {
			try {
				ApiResponse<dynamic> bearer =await _twoLeggedApi.AuthenticateAsyncWithHttpInfo (FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, _grantType, _scope) ;
				httpErrorHandler (bearer, "Failed to get your token") ;
				_2legged_bearer =bearer.Data ;
				return (bearer) ;
			} catch ( Exception ex ) {
				_2legged_bearer =null ;
				MessageBox.Show ("Exception when calling TwoLeggedApi.AuthenticateWithHttpInfo: " + ex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
				return (null) ;
			}
		}

		#endregion

		public MainWindow () {
			readKeys () ;

			InitializeComponent () ;
			ForgeObjects.View =ForgeObjects.FindResource ("tileView") as ViewBase ;
			DataContext =this ;

			var values =Enum.GetValues (typeof (Region)) ;
			foreach ( var value in values )
				ForgeRegion.Items.Add (value.ToString ()) ;
			ForgeRegion.SelectedItem =Region.US.ToString () ;
		}

		protected ObservableCollection<ForgeObjectInfo> ItemsSource {
			get {
				return (ForgeObjects.ItemsSource == null ?
						  new ObservableCollection<ForgeObjectInfo> ()
						: new ObservableCollection<ForgeObjectInfo> ((IEnumerable<ForgeObjectInfo>)ForgeObjects.ItemsSource)
				) ;
			}
			set {
				ForgeObjects.ItemsSource =value ;
			}
		}

		private void Window_Loaded (object sender, RoutedEventArgs e) {
			Handled (e) ;
			bool isNetworkAvailable =System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable () ;
			if ( !isNetworkAvailable ) {
				//LogError ("Network Error: Check your network connection and try again...") ;
				MessageBox.Show ("Network Error: Check your network connection and try again...", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
				return ;
			}

			LoginMenu_Click (null, null) ;
		}

		// http://stackoverflow.com/questions/9992119/wpf-app-doesnt-shut-down-when-closing-main-window
		// Required to close when using a WebBrowser control
		// See also the App.xaml - ShutdownMode="OnExplicitShutdown"
		private void Window_Closed (object sender, EventArgs e) {
			//Handled (e) ;
			Application.Current.Shutdown () ;
		}

		#region UI Commands
		private void Configure_Click (object sender, RoutedEventArgs e) {
			ConfigureKeys () ;
		}

		private bool ConfigureKeys () {
			Credentials wnd =new Credentials () ;
			wnd.Owner =this ;
			wnd.CLIENT_ID.Text =FORGE_CLIENT_ID ;
			wnd.CLIENT_SECRET.Password =FORGE_CLIENT_SECRET ;
			wnd.PORT.Text =PORT ;
			wnd.CALLBACK.Text =FORGE_CALLBACK ;
			Nullable<bool> dialogResult =wnd.ShowDialog () ;
			if ( dialogResult.Value == false )
				return (false) ;
			bool? nullableBool =wnd.SaveInSettings.IsChecked ;
			readKeys (
				wnd.CLIENT_ID.Text,
				wnd.CLIENT_SECRET.Password,
				wnd.PORT.Text,
				wnd.CALLBACK.Text,
				nullableBool == true
			) ;
			return (true) ;
		}

		private async void LoginMenu_Click (object sender, RoutedEventArgs e) {
			Handled (e) ;
			try {
				if ( string.IsNullOrEmpty (FORGE_CLIENT_ID) || string.IsNullOrEmpty (FORGE_CLIENT_SECRET) ) {
					if ( ConfigureKeys () )
						return ;
				}

				State =StateEnum.Busy ;
				ForgeMenu.IsEnabled =false ;
				ForgeRegion.IsEnabled =false ;
				
				if ( _2legged_bearer == null )
					await oauthExecAsync () ;
				else
					_2legged_bearer =null ;
				ForgeLoginIcon.Source =(_2legged_bearer == null ?
					  new BitmapImage (new Uri (@"Images\Login.png", UriKind.Relative))
					: new BitmapImage (new Uri (@"Images\Logout.png", UriKind.Relative))
				) ;
				ForgeLogin.ToolTip =(_2legged_bearer == null ?
					  "Login from the Forge WEB Service."
					: "Logout from the Forge WEB Service."
				) ;
				connectedLabel.Content =(_2legged_bearer == null ?
					  "Not connected to the Forge server"
					: "Connected to the Forge server"
				) ;
				connectedTimeLabel.Content ="" ;
				DateTime dt =DateTime.Now ;
				if ( _2legged_bearer == null ) {
					ForgeRegion.IsEnabled =true ;
					return ;
				}
				dt =dt.AddSeconds ((double)_2legged_bearer.expires_in) ;
				connectedTimeLabel.Content =(_2legged_bearer == null ?
					  ""
					: "Your current token will exprire at: " + dt.ToLocalTime ()
				) ;

				// Let's create the bucket even if it exists already, and next get its content
				await ConfigureBucket () ;
				BucketContentRefresh_Click (sender, e) ;
			} finally {
				ForgeMenu.IsEnabled =true ;
				State =StateEnum.Idle ;
			}
		}

		private async Task ConfigureBucket () {
			try {
				BucketsApi ossBuckets =new BucketsApi () ;
				ossBuckets.Configuration.AccessToken =accessToken ;
				PostBucketsPayload payload =new PostBucketsPayload (_bucket, null, _bucketType) ;
				ApiResponse<dynamic> response =await ossBuckets.CreateBucketAsyncWithHttpInfo (payload, (string)ForgeRegion.SelectedItem) ;
				httpErrorHandler (response, "Failed to create bucket") ;
			} catch ( ApiException apiex ) {
				if ( apiex.ErrorCode != 409 ) // Already exists - we're good
					MessageBox.Show ("Exception when calling BucketsApi.CreateBucketAsyncWithHttpInfo: " + apiex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
			} catch ( Exception ex ) {
				MessageBox.Show ("Exception when calling BucketsApi.CreateBucketAsyncWithHttpInfo: " + ex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
			}
			return ;
		}

		private async void BucketContentRefresh_Click (object sender, RoutedEventArgs e) {
			Handled (e) ;
			try {
				State =StateEnum.Busy ;
				ItemsSource =null ;
				string startAt =null ;
				do {
					startAt =await GetBucketContent (startAt, 10) ;
				} while ( !string.IsNullOrEmpty (startAt) ) ;
				ForgeObjects.Items.Refresh () ;
			} finally {
				State =StateEnum.Idle ;
			}
		}

		private async Task<string> GetBucketContent (string startAt =null, int limit =10) {
			try {
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossObjects.GetObjectsAsyncWithHttpInfo (
					_bucket, limit, null, startAt
				) ;
				httpErrorHandler (response, "Failed to access buckets list") ;
				ObservableCollection<ForgeObjectInfo> items =ItemsSource ;
				foreach ( KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems (response.Data.items) ) {
					objInfo.Value.region =(string)ForgeRegion.SelectedItem ;
					items.Add (new ForgeObjectInfo () {
						Name =objInfo.Value.objectKey,
						Size =SizeSuffix (objInfo.Value.size),
						Image =DEFAULT_IMAGE,
						Properties =objInfo.Value
					}) ;
				}
				ItemsSource =items ;

				if ( !hasOwnProperty (response.Data, "next") )
					return (null) ;
				var uri =new Uri (response.Data.next) ;
				NameValueCollection url_parts =System.Web.HttpUtility.ParseQueryString (uri.Query) ;
				return (url_parts ["startAt"]) ;
			} catch ( Exception ex ) {
				Debug.WriteLine (ex.Message) ;
			}
			return (null) ;
		}

		// In debug, Drag'nDrop will not work if you run Developer Studio as administrator
		private async void ForgeScenes_Drop (object sender, DragEventArgs e) {
			Handled (e) ;
			try {
				//State =StateEnum.Idle ;
				if ( e.Data.GetDataPresent (DataFormats.FileDrop) ) {
					ObservableCollection<ForgeObjectInfo> items =ItemsSource ;
					string [] files =(string [])e.Data.GetData (DataFormats.FileDrop) ;
					await Task.WhenAll (files.Select (filename => UploadExecute (items, System.IO.Path.GetFileName (filename), filename))) ;
					ItemsSource =items ;
					ForgeObjects.SelectAll () ;
				}
			} finally {
				//State =StateEnum.Idle ;
			}
		}

		protected async Task<bool> UploadExecute (ObservableCollection<ForgeObjectInfo> items, string fileKey, string filename) {
			UploadProgress wnd =new UploadProgress (fileKey) ;
			wnd.Show () ;

			try {
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				using ( StreamReader streamReader =new StreamReader (filename) ) {
					ApiResponse<dynamic> response =await ossObjects.UploadObjectAsyncWithHttpInfo (
						_bucket,
						fileKey, (int)streamReader.BaseStream.Length, streamReader.BaseStream,
						"application/octet-stream"
					) ;
					httpErrorHandler (response, "Failed to upload file") ;
					response.Data.region =(string)ForgeRegion.SelectedItem ;
					items.Add (new ForgeObjectInfo () {
						Name =fileKey,
						Size =SizeSuffix (new System.IO.FileInfo (filename).Length),
						Image =DEFAULT_IMAGE,
						Properties =response.Data
					});
				}

				wnd.ReportProgress (new ProgressInfo (100, "File upload succeeded")) ;
			} catch ( Exception ex ) {
				wnd.ReportProgress (new ProgressInfo (0, ex.Message)) ;
				return (false) ;
			}
			return (true) ;
		}

		private async void Item_Refresh (object sender, RoutedEventArgs e) {
			Handled (e) ;
			int i =0 ;
			Task[] tasks =new Task [2 * ForgeObjects.SelectedItems.Count] ;
			foreach ( ForgeObjectInfo item  in ForgeObjects.SelectedItems ) {
				tasks [i++] =RequestProperties (item) ;
				tasks [i++] =RequestManifest (item) ;
			}
			//Task.WaitAll (tasks) ;
			await Task.WhenAll (tasks) ;
		}

		private async void Item_Translate (object sender, RoutedEventArgs e) {
			Handled (e) ;
			ObservableCollection<ForgeObjectInfo> items =ItemsSource ;
			await Task.WhenAll (
				ForgeObjects.SelectedItems.Cast<ForgeObjectInfo> ().Select (item =>
					TranslateObject (items, item))
			) ;
			ItemsSource =items ;
			ForgeObjects.Items.Refresh () ;
		}

		private async Task<bool> TranslateObject (ObservableCollection<ForgeObjectInfo> items, ForgeObjectInfo item) {
			try {
				string urn =URN (_bucket, item, false) ;
				JobPayloadInput jobInput =new JobPayloadInput (
					urn,
					System.IO.Path.GetExtension (item.Name).ToLower () == ".zip",
					item.Name
				) ;
				JobPayloadOutput jobOutput =new JobPayloadOutput (
					new List<JobPayloadItem> (
						new JobPayloadItem [] {
							new JobPayloadItem (
								JobPayloadItem.TypeEnum.Svf,
								new List<JobPayloadItem.ViewsEnum> (
									new JobPayloadItem.ViewsEnum [] {
										JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._3d
									}
								),
								null
							)
						}
					)
				) ;
				JobPayload job =new JobPayload (jobInput, jobOutput) ;
				bool bForce =true ;
				DerivativesApi md =new DerivativesApi () ;
				md.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await md.TranslateAsyncWithHttpInfo (job, bForce) ;
				httpErrorHandler (response, "Failed to register file for translation") ;
				item.TranslationRequested =StateEnum.Busy ;
				item.Manifest =response.Data ;

				JobProgress jobWnd =new JobProgress (item, accessToken) ;
				jobWnd._callback =new JobCompletedDelegate (this.TranslationCompleted) ;
				jobWnd.Owner =this ;
				jobWnd.Show () ;
			} catch ( Exception /*ex*/ ) {
				item.TranslationRequested =StateEnum.Idle ;
				return (false) ;
			}
			return (true) ;
		}

		public void TranslationCompleted (ForgeObjectInfo item) {
			ForgeObjects.Items.Refresh () ;
		}

		private async void Item_Properties (object sender, RoutedEventArgs e) {
			Handled (e) ;
			await Task.WhenAll (
				ForgeObjects.SelectedItems.Cast<ForgeObjectInfo> ().Select (item =>
					RequestProperties (item))
			) ;
		}

		private async Task RequestProperties (ForgeObjectInfo item) {
			try {
				item.PropertiesRequested =StateEnum.Busy ;
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossObjects.GetObjectDetailsAsyncWithHttpInfo (_bucket, item.Name) ;
				httpErrorHandler (response, "Failed to get object details") ;
				response.Data.region =(string)ForgeRegion.SelectedItem ;
				item.Properties =response.Data ;
			} catch ( Exception /*ex*/ ) {
				item.Properties =null ;
			} finally {
				item.PropertiesRequested =StateEnum.Idle ;
			}
		}

		private async Task RequestManifest (ForgeObjectInfo item) {
			try {
				item.ManifestRequested =StateEnum.Busy ;
				DerivativesApi md =new DerivativesApi () ;
				md.Configuration.AccessToken =accessToken ;
				string urn =URN (_bucket, item) ;
				ApiResponse<dynamic> response =await md.GetManifestAsyncWithHttpInfo (urn) ;
				httpErrorHandler (response, "Failed to get manifest") ;
				item.Manifest =response.Data ;
			} catch ( Exception /*ex*/ ) {
				item.Manifest =null ;
			} finally {
				item.ManifestRequested =StateEnum.Idle ;
			}
		}

		private async void Item_Download (object sender, RoutedEventArgs e) {
			Handled (e) ;
			// http://stackoverflow.com/questions/4007882/select-folder-dialog-wpf/17712949#17712949
			CommonOpenFileDialog dlg =new CommonOpenFileDialog () ;
			dlg.Title ="Select Folder where to save your files";
			dlg.IsFolderPicker =true ;
			dlg.InitialDirectory =System.AppDomain.CurrentDomain.BaseDirectory ;
			dlg.AddToMostRecentlyUsedList =false ;
			dlg.AllowNonFileSystemItems =false ;
			dlg.DefaultDirectory =System.AppDomain.CurrentDomain.BaseDirectory ;
			dlg.EnsureFileExists =true ;
			dlg.EnsurePathExists =true ;
			dlg.EnsureReadOnly =false ;
			dlg.EnsureValidNames =true ;
			dlg.Multiselect =false ;
			dlg.ShowPlacesList =true ;
			if ( dlg.ShowDialog () == CommonFileDialogResult.Ok ) {
				string folder =dlg.FileName ;
				await Task.WhenAll (ForgeObjects.SelectedItems.Cast<ForgeObjectInfo> ().Select (item =>
					DownloadExecute (item, folder))) ;
			}
		}

		protected async Task<bool> DownloadExecute (ForgeObjectInfo item, string folder) {
			DownloadProgress wnd =new DownloadProgress (item.Name) ;
			wnd.Show () ;

			try {
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossObjects.GetObjectAsyncWithHttpInfo (_bucket, item.Properties.objectKey) ;
				httpErrorHandler (response, "Failed to download file") ;
				Stream downloadObj =response.Data as Stream ;
				downloadObj.Position =0 ;
				string outputFilename =System.IO.Path.Combine (folder, item.Name) ;
				using ( FileStream outputFile =new FileStream (outputFilename, FileMode.Create) )
					downloadObj.CopyTo (outputFile) ;
				wnd.ReportProgress (new ProgressInfo (100, "File download succeeded")) ;
			} catch ( Exception ex ) {
				wnd.ReportProgress (new ProgressInfo (0, ex.Message)) ;
				return (false) ;
			}
			return (true) ;
		}

		private async void Item_DownloadNoUI (object sender, RoutedEventArgs e) {
			Handled (e) ;
			// http://stackoverflow.com/questions/4007882/select-folder-dialog-wpf/17712949#17712949
			CommonOpenFileDialog dlg =new CommonOpenFileDialog () ;
			dlg.Title ="Select Folder where to save your files";
			dlg.IsFolderPicker =true ;
			dlg.InitialDirectory =System.AppDomain.CurrentDomain.BaseDirectory ;
			dlg.AddToMostRecentlyUsedList =false ;
			dlg.AllowNonFileSystemItems =false ;
			dlg.DefaultDirectory =System.AppDomain.CurrentDomain.BaseDirectory ;
			dlg.EnsureFileExists =true ;
			dlg.EnsurePathExists =true ;
			dlg.EnsureReadOnly =false ;
			dlg.EnsureValidNames =true ;
			dlg.Multiselect =false ;
			dlg.ShowPlacesList =true ;
			if ( dlg.ShowDialog () == CommonFileDialogResult.Ok ) {
				string folder =dlg.FileName ;
				int i =0 ;
				Task[] tasks =new Task [ForgeObjects.SelectedItems.Count] ;
				foreach ( ForgeObjectInfo item in ForgeObjects.SelectedItems )
					tasks [i++] =DownloadFileObjectNoUI (item, folder) ;
				await Task.WhenAll (tasks) ;
			}
		}

		private async Task<bool> DownloadFileObjectNoUI (ForgeObjectInfo item, string folder) {
			try {
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossObjects.GetObjectAsyncWithHttpInfo (_bucket, item.Properties.objectKey) ;
				httpErrorHandler (response, "Failed to download file") ;
				Stream downloadObj =response.Data as Stream ;
				downloadObj.Position =0 ;
				string outputFilename =System.IO.Path.Combine (folder, item.Name) ;
				using ( FileStream outputFile =new FileStream (outputFilename, FileMode.Create) )
					downloadObj.CopyTo (outputFile) ;
			} catch ( Exception ex ) {
				Debug.WriteLine (ex.Message) ;
				return (false) ;
			}
			return (true) ;
		}

		private async void Item_DeleteObject (object sender, RoutedEventArgs e) {
			Handled (e) ;
			ObservableCollection<ForgeObjectInfo> items =ItemsSource ;
			await Task.WhenAll (
				ForgeObjects.SelectedItems.Cast<ForgeObjectInfo> ().Select (item =>
					DeleteObjectOnServer (items, item))
			) ;
			ItemsSource =items ;
			ForgeObjects.Items.Refresh () ;
		}

		private async Task<bool> DeleteObjectOnServer (ObservableCollection<ForgeObjectInfo> items, ForgeObjectInfo item) {
			try {
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossObjects.DeleteObjectAsyncWithHttpInfo (_bucket, item.Name) ;
				httpErrorHandler (response, "Failed to delete file") ;
				items.Remove (item) ;
			} catch ( Exception ex ) {
				Debug.WriteLine (ex.Message) ;
				return (false) ;
			}
			return (true) ;
		}

		private async void Item_DeleteManifest (object sender, RoutedEventArgs e) {
			Handled (e) ;
			await Task.WhenAll (
				ForgeObjects.SelectedItems.Cast<ForgeObjectInfo> ().Select (item =>
					DeleteManifestOnServer (item))
			) ;
		}

		private void Launch_Viewer (object sender, RoutedEventArgs e) {
			Handled (e) ;
			if ( ForgeObjects.SelectedItems.Count != 1 ) {
				MessageBox.Show ("We can launch only one viewer at a time for now!", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information) ;
				return ;
			}
			ForgeObjectInfo item =ForgeObjects.SelectedItem as ForgeObjectInfo ;
			string urn =URN (_bucket, item, true) ;
			string url ="https://models.autodesk.io/view.html?urn=" + urn + "&accessToken=" + accessToken ;
			System.Diagnostics.Process.Start (new System.Diagnostics.ProcessStartInfo (url)) ;
		}

		private void Launch_ViewerEmbedded (object sender, RoutedEventArgs e) {
			Handled (e) ;
			if ( ForgeObjects.SelectedItems.Count != 1 ) {
				MessageBox.Show ("We can launch only one viewer at a time for now!", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information) ;
				return ;
			}
			Viewer wnd =new Viewer (ForgeObjects.SelectedItem as ForgeObjectInfo, accessToken) ;
			wnd.Show () ;
		}

		private async Task<bool> DeleteManifestOnServer (ForgeObjectInfo item) {
			try {
				DerivativesApi md =new DerivativesApi () ;
				md.Configuration.AccessToken =accessToken ;
				string urn =URN (_bucket, item) ;
				ApiResponse<dynamic> response =await md.DeleteManifestAsyncWithHttpInfo (urn) ;
				httpErrorHandler (response, "Failed to delete manifest") ;
				item.Manifest =null ;
				item.TranslationRequested =StateEnum.Idle ;
			} catch ( Exception ex ) {
				item.TranslationRequested =StateEnum.Idle ;
				Debug.WriteLine (ex.Message) ;
				return (false) ;
			}
			return (true) ;
		}

		private void requestNavigate (object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
			Handled (e) ;
			System.Diagnostics.Process.Start (new System.Diagnostics.ProcessStartInfo (e.Uri.AbsoluteUri)) ;
		}

		private /*async*/ void ForgeObjects_SelectionChanged (object sender, SelectionChangedEventArgs e) {
			Handled (e) ;
			propertyGrid.SelectedObject =null ;
			if ( ForgeObjects.SelectedItems.Count != 1 )
				return ;
			ForgeObjectInfo item =ForgeObjects.SelectedItem as ForgeObjectInfo ;
			//propertyGrid.SelectedObject =item.Properties ;
			propertyGrid.SelectedObject =new ItemProperties (item) ;
		}

		#endregion

		#region Utils
		public static bool httpErrorHandler (ApiResponse<dynamic> response, string msg ="", bool bThrowException =true) {
			if ( response.StatusCode < 200 || response.StatusCode >= 300 ) {
				if ( bThrowException )
					throw new Exception (msg + " (HTTP " + response.StatusCode + ")") ;
				return (true) ;
			}
			return (false) ;
		}

		public static bool hasOwnProperty (dynamic obj, string name) {
			try {
				var test =obj [name] ;
				return (true) ;
			} catch ( Exception /*ex*/ ) {
				return (false) ;
			}
		}

		public static string URN (string bucketKey, ForgeObjectInfo item, bool bSafe =true) {
			string urn ="urn:adsk.objects:os.object:" + bucketKey + "/" + item.Name ;
			try {
				if ( item.Properties != null )
					urn =item.Properties.objectId ;
				urn =bSafe ? SafeBase64Encode (urn) : Base64Encode (urn) ;
			} catch ( Exception /*ex*/ ) {
			}
			return (urn) ;
		}

		public static string Base64Encode (string plainText) {
			var plainTextBytes =System.Text.Encoding.UTF8.GetBytes (plainText) ;
			return (System.Convert.ToBase64String (plainTextBytes)) ;
		}

		public static string Base64Decode (string base64EncodedData) {
			var base64EncodedBytes =System.Convert.FromBase64String (base64EncodedData) ;
			return (System.Text.Encoding.UTF8.GetString (base64EncodedBytes)) ;
		}

		private static readonly char [] padding ={ '=' } ;
		public static string SafeBase64Encode (string plainText) {
			var plainTextBytes =System.Text.Encoding.UTF8.GetBytes (plainText) ;
			return (System.Convert.ToBase64String (plainTextBytes)
				.TrimEnd (padding).Replace ('+', '-').Replace ('/', '_')
			) ;
		}

		public static string SafeBase64Decode (string base64EncodedData) {
			string st =base64EncodedData.Replace ('_', '/').Replace ('-', '+') ;
			switch ( base64EncodedData.Length % 4 ) {
				case 2: st +="==" ; break ;
				case 3: st +="=" ; break ;
			}
			var base64EncodedBytes =System.Convert.FromBase64String (st) ;
			return (System.Text.Encoding.UTF8.GetString (base64EncodedBytes)) ;
		}

		protected static void Handled (RoutedEventArgs e) {
			if ( e != null )
				e.Handled =true ;
		}

		protected static readonly string [] SizeSuffixes ={ "b", "kb", "mb", "gb", "tb", "pb", "eb", "zb", "yb" } ;
		public static string SizeSuffix (Int64 value) {
			if ( value < 0 ) return ("-" + SizeSuffix (-value)) ;
			if ( value == 0 ) return ("0.0 bytes") ;
			int mag =(int)Math.Log (value, 1024) ;
			decimal adjustedSize =(decimal)value / (1L << (mag * 10)) ;
			return (string.Format ("{0:n1} {1}", adjustedSize, SizeSuffixes [mag])) ;
		}

		#endregion

		private void mainWnd_Closed (object sender, EventArgs e) {

		}
	}

}
