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
using System.Runtime.CompilerServices;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Extended WPF Toolkit™ Community Edition - http://wpftoolkit.codeplex.com/
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

// For Folder Browser
using Microsoft.WindowsAPICodePack.Dialogs;

using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;

namespace Autodesk.Forge.WpfCsharp {

	public enum StateEnum {
		Busy,
		Idle
	}

	public class ForgeObjectInfo : INotifyPropertyChanged {
		public string Name { get; set; }
		public string Size { get; set; }
		public string Image { get; set; }
		public dynamic _Properties =null ;
		public dynamic Properties { get { return (_Properties) ; } set { SetField (ref _Properties, value) ; } } 
		private StateEnum _PropertiesRequested =StateEnum.Idle ;
		public StateEnum PropertiesRequested { get { return (_PropertiesRequested) ; } set { SetField (ref _PropertiesRequested, value) ; } } 
		private dynamic _Manifest =null ;
		public dynamic Manifest { get { return (_Manifest) ; } set { SetField (ref _Manifest, value) ; } } 
		private StateEnum _ManifestRequested =StateEnum.Idle ;
		public StateEnum ManifestRequested { get { return (_ManifestRequested) ; } set { SetField (ref _ManifestRequested, value) ; } } 
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

		#region Forge Properties / Initialization
		public enum Region {
			US,
			EMEA
		} ;

		private string FORGE_CLIENT_ID {
			get { return (Properties.Settings.Default.FORGE_CLIENT_ID) ; }
			set { Properties.Settings.Default.FORGE_CLIENT_ID =value ; }
		}

		private Crypto _Crypto =new Crypto (WpfCsharp.Crypto.CryptoTypes.encTypeTripleDES) ;
		private string FORGE_CLIENT_SECRET {
			get { return (_Crypto.Decrypt (Properties.Settings.Default.FORGE_CLIENT_SECRET)) ; }
			set { Properties.Settings.Default.FORGE_CLIENT_SECRET =_Crypto.Encrypt (value) ; }
		}

		private string PORT {
			get { return (Properties.Settings.Default.PORT) ; }
			set { Properties.Settings.Default.PORT =value ; }
		}

		private string FORGE_CALLBACK {
			get { return (Properties.Settings.Default.FORGE_CALLBACK) ; }
			set { Properties.Settings.Default.FORGE_CALLBACK =value ; }
		}

		private string SERIAL_NUMBER {
			get { return (Properties.Settings.Default.SERIAL_NUMBER) ; }
			set { Properties.Settings.Default.SERIAL_NUMBER =value ; }
		}

		private string TOKEN_URL {
			get { return (_Crypto.Decrypt (Properties.Settings.Default.TOKEN_URL)) ; }
			set { Properties.Settings.Default.TOKEN_URL =_Crypto.Encrypt (value) ; }
		}

		// http://stackoverflow.com/questions/154533/best-way-to-bind-wpf-properties-to-applicationsettings-in-c
		private string _2LEGGED {
			get { return (_Crypto.Decrypt (Properties.Settings.Default._2LEGGED)) ; }
			set { Properties.Settings.Default._2LEGGED =_Crypto.Encrypt (value) ; }
		}

		private string _3LEGGED {
			get { return (_Crypto.Decrypt (Properties.Settings.Default._3LEGGED)) ; }
			set { Properties.Settings.Default._3LEGGED =_Crypto.Encrypt (value) ; }
		}

		private void readFromEnvOrSettings (string name, Action<string> setOutput) {
			string st =Environment.GetEnvironmentVariable (name) ;
			if ( !string.IsNullOrEmpty (st) )
				setOutput (st) ;
		}

		private bool readConfigFromEnvOrSettings () {
			readFromEnvOrSettings ("FORGE_CLIENT_ID", value => FORGE_CLIENT_ID =value) ;
			readFromEnvOrSettings ("FORGE_CLIENT_SECRET", value => FORGE_CLIENT_SECRET =value) ;
			readFromEnvOrSettings ("PORT", value => PORT =value) ;
			readFromEnvOrSettings ("FORGE_CALLBACK", value => FORGE_CALLBACK =value) ;
			return (true) ;
		}

		private bool putConfig (
			string serial, string url, 
			string clientId, string clientSecret, string port, string callback,
			string _2legged, string _3legged,
			bool bSaveInSettings =false
		) {
			SERIAL_NUMBER =serial ;
			if ( !string.IsNullOrEmpty (url) )
				TOKEN_URL =url ;
			FORGE_CLIENT_ID =clientId ;
			FORGE_CLIENT_SECRET =clientSecret ;
			PORT =port ;
			FORGE_CALLBACK =callback ;
			if ( !string.IsNullOrEmpty (_2legged) )
				_2LEGGED =_2legged ;
			if ( !string.IsNullOrEmpty (_3legged) )
				_3LEGGED =_3legged ;
			if ( bSaveInSettings )
				Properties.Settings.Default.Save () ;
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
		protected string accessToken { get { return (_2LEGGED) ; } }
		protected string userToken { get { return (_3LEGGED) ; } }

		private async Task oauthExecAsyncFromTokenServer () {
			try {
				RestClient client =new RestClient (TOKEN_URL.TrimEnd (new char [] { '/' })) ;
				string url ="/" + SERIAL_NUMBER ;
				RestRequest request =new RestRequest (url, Method.POST) ;
				DateTime dt =DateTime.UtcNow ;
				string obj ="{ \"serial\": \"" + SERIAL_NUMBER + "\", \"other\": \"" + dt.ToString ("o") + "\" }" ;
				byte[] data =DigitalSignature.Encrypt (obj) ;
				request.AddParameter ("application/octet-stream", Convert.ToBase64String (data), ParameterType.RequestBody) ;
				//await client.ExecuteAsync (request, response => {
				//	if ( response.StatusCode == System.Net.HttpStatusCode.OK ) {
				//		// OK
				//	} else {
				//		// NOK
				//	}
				//}) ;
				_2LEGGED ="" ;
				var restResponse =await client.ExecuteTaskAsync (request) ;
				if ( restResponse.StatusCode == System.Net.HttpStatusCode.OK ) {
					string json =restResponse.Content ;
					JObject bearer =JObject.Parse (json) ;
					// Token server encrypted the token
					_2LEGGED =_Crypto.Decrypt (bearer ["access_token"].ToString ()) ;
				} else {
					MessageBox.Show ("Could not get an access token from the server. Sorry!", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
				}
			} catch ( Exception ex ) {
				MessageBox.Show ("Exception when requesting tokens: " + ex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
			}
		}

		private async Task<ApiResponse<dynamic>> oauthExecAsync () {
			Scope[] scope =new Scope[] {
				Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch,
				Scope.BucketCreate, Scope.BucketRead, Scope.BucketUpdate, Scope.BucketDelete } ;
			//Scope[] scopeViewer =new Scope[] { Scope.DataRead } ;

			try {
				_2LEGGED ="" ;
				TwoLeggedApi _twoLeggedApi =new TwoLeggedApi () ;
				ApiResponse<dynamic> bearer =await _twoLeggedApi.AuthenticateAsyncWithHttpInfo (
					FORGE_CLIENT_ID, FORGE_CLIENT_SECRET, oAuthConstants.CLIENT_CREDENTIALS, scope) ;
				httpErrorHandler (bearer, "Failed to get your token") ;
				_2LEGGED =bearer.Data.access_token ;
				return (bearer) ;
			} catch ( Exception ex ) {
				MessageBox.Show ("Exception when calling TwoLeggedApi.AuthenticateAsyncWithHttpInfo : " + ex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
				return (null) ;
			}
		}

		private bool isApplicationConfigured () {
			return (
				   !string.IsNullOrEmpty (TOKEN_URL)
				|| !string.IsNullOrEmpty (_2LEGGED)
				|| (   !string.IsNullOrEmpty (FORGE_CLIENT_ID)
					&& !string.IsNullOrEmpty (FORGE_CLIENT_SECRET)
				   )
			) ;
		}

		private async Task<bool> AutoLog () {
			// We should prefer getting a new token
			if ( !string.IsNullOrEmpty (TOKEN_URL) ) {
				/*dynamic bearer =*/await oauthExecAsyncFromTokenServer () ;
				if ( !string.IsNullOrEmpty (_2LEGGED) )
					return (true) ;
			}

			// Verify we got a client_id and a client_secret
			if (   string.IsNullOrEmpty (TOKEN_URL)
				&& !string.IsNullOrEmpty (FORGE_CLIENT_ID)
				&& !string.IsNullOrEmpty (FORGE_CLIENT_SECRET)
			) {
				/*dynamic bearer =*/await oauthExecAsync () ;
				if ( !string.IsNullOrEmpty (_2LEGGED) )
					return (true) ;
			}

			// Verify this token is still valid
			if (   string.IsNullOrEmpty (TOKEN_URL)
				&& !string.IsNullOrEmpty (_2LEGGED) ) {
				try {
					DerivativesApi md =new DerivativesApi () ;
					md.Configuration.AccessToken =accessToken ;
					dynamic response =await md.GetFormatsAsync () ;
					return (true) ;
				} catch ( Exception /*ex*/ ) {
					_2LEGGED ="" ;
				}
			}

			//MessageBox.Show ("No auto-log...", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information) ;
			return (false) ;
		}

		#endregion

		public MainWindow () {
			readConfigFromEnvOrSettings () ;

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

		private async void Window_Loaded (object sender, RoutedEventArgs e) {
			Handled (e) ;
			bool isNetworkAvailable =System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable () ;
			if ( !isNetworkAvailable ) {
				MessageBox.Show ("Network Error: Check your network connection and try again...", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
				return ;
			}

			try {
				if ( !isApplicationConfigured () ) {
					if ( !ConfigureKeys () )
						return ;
				}

				State =StateEnum.Busy ;
				ForgeMenu.IsEnabled =false ;

				bool success =await AutoLog () ;
				UpdateLoginUI (success) ;
				if ( success )
					RefreshBucketList (sender, e) ;
			} finally {
				ForgeMenu.IsEnabled =true ;
				State =StateEnum.Idle ;
			}
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
			Handled (e) ;
			ConfigureKeys () ;
		}

		private bool ConfigureKeys () {
			Configuration wnd =new Configuration () ;
			wnd.Owner =this ;
			wnd.SERIAL_NUMBER.Text =SERIAL_NUMBER ;
			wnd.TOKEN_URL.Password ="" ;
			wnd.CLIENT_ID.Text =FORGE_CLIENT_ID ;
			wnd.CLIENT_SECRET.Password =FORGE_CLIENT_SECRET ;
			wnd.PORT.Text =PORT ;
			wnd.CALLBACK.Text =FORGE_CALLBACK ;
			wnd._2DLEGGED.Text ="" ;
			wnd._3DLEGGED.Text ="" ;

			Nullable<bool> dialogResult =wnd.ShowDialog () ;
			if ( dialogResult.Value == false )
				return (false) ;
			bool? nullableBool =wnd.SaveInSettings.IsChecked ;
			//wnd.tabControl.SelectedIndex
			putConfig (
				wnd.SERIAL_NUMBER.Text,
				wnd.TOKEN_URL.Password,
				wnd.CLIENT_ID.Text,
				wnd.CLIENT_SECRET.Password,
				wnd.PORT.Text,
				wnd.CALLBACK.Text,
				wnd._2DLEGGED.Text,
				wnd._3DLEGGED.Text,
				nullableBool == true
			) ;

			if ( !string.IsNullOrEmpty (_2LEGGED) ) {
				UpdateLoginUI (true) ;
				RefreshBucketList (null, null) ;
			}

			return (true) ;
		}

		private bool UpdateLoginUI (bool iamlogged =false, string expireAt =null) {
			ForgeLoginIcon.Source =(!iamlogged ?
				  new BitmapImage (new Uri (@"Images\Login.png", UriKind.Relative))
				: new BitmapImage (new Uri (@"Images\Logout.png", UriKind.Relative))
			) ;
			ForgeLogin.ToolTip =(!iamlogged ?
				  "Login from the Forge WEB Service."
				: "Logout from the Forge WEB Service."
			) ;
			connectedLabel.Content =(!iamlogged ?
				  "Not connected to the Forge server"
				: "Connected to the Forge server"
			) ;
			connectedTimeLabel.Content ="" ;
			if ( !string.IsNullOrEmpty (expireAt) ) {
				connectedTimeLabel.Content = (!iamlogged ?
					  ""
					: "Your current token will exprire at: " + expireAt
				) ;
			}
			if ( !iamlogged ) {
				BucketsInRegion.Items.Clear () ;
				ItemsSource =null ;
			}
			return (iamlogged) ;
		}

		private async void LoginMenu_Click (object sender, RoutedEventArgs e) {
			Handled (e) ;
			try {
				bool has2LeggedToken =!string.IsNullOrEmpty (_2LEGGED) ;
				_2LEGGED ="" ;
				UpdateLoginUI (false) ;

				if ( !isApplicationConfigured () )
					return ;

				State =StateEnum.Busy ;
				ForgeMenu.IsEnabled =false ;
				
				if ( !has2LeggedToken ) {
					if ( !string.IsNullOrEmpty (TOKEN_URL) )
						/*dynamic bearer =*/await oauthExecAsyncFromTokenServer () ;
					else if (
						   string.IsNullOrEmpty (_2LEGGED)
						&& !string.IsNullOrEmpty (FORGE_CLIENT_ID)
						&& !string.IsNullOrEmpty (FORGE_CLIENT_SECRET)
					)
						/*dynamic bearer =*/await oauthExecAsync () ;
				}
				if ( !UpdateLoginUI (!string.IsNullOrEmpty (_2LEGGED)) )
					return ;

				RefreshBucketList (sender, e) ;
			} finally {
				ForgeMenu.IsEnabled =true ;
				State =StateEnum.Idle ;
			}
		}

		private async void CreateBucket_Click (object sender, RoutedEventArgs e) {
			Handled (e) ;
			CreateBucket wnd =new CreateBucket () ;
			wnd.Owner =this ;
			Nullable<bool> dialogResult =wnd.ShowDialog () ;
			if ( dialogResult.Value == false )
				return ;

			try {
				BucketsApi ossBuckets =new BucketsApi () ;
				ossBuckets.Configuration.AccessToken =accessToken ;
				PostBucketsPayload.PolicyKeyEnum bucketType =(PostBucketsPayload.PolicyKeyEnum)Enum.Parse (typeof (PostBucketsPayload.PolicyKeyEnum), wnd.BucketType.Text, true) ;
				string region =wnd.BucketRegion.Text ;
				PostBucketsPayload payload =new PostBucketsPayload (wnd.BucketName.Text, null, bucketType) ;
				ApiResponse<dynamic> response =await ossBuckets.CreateBucketAsyncWithHttpInfo (payload, region) ;
				httpErrorHandler (response, "Failed to create bucket") ;
				if ( region == (string)ForgeRegion.SelectedItem ) {
					BucketsInRegion.Items.Add (wnd.BucketName.Text) ;
					BucketsInRegion.SelectedItem =wnd.BucketName.Text ;
				} else {
					MessageBox.Show ("Bucket successfully created!", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information) ;
				}
			} catch ( ApiException apiex ) {
				if ( apiex.ErrorCode != 409 ) // Already exists - we're good
					MessageBox.Show ("Exception when calling BucketsApi.CreateBucketAsyncWithHttpInfo: " + apiex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
				else
					MessageBox.Show ("This bucket already exist, choose another name!", APP_NAME, MessageBoxButton.OK, MessageBoxImage.Warning) ;
			} catch ( Exception ex ) {
				MessageBox.Show ("Exception when calling BucketsApi.CreateBucketAsyncWithHttpInfo: " + ex.Message, APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error) ;
			}
		}

		private void ForgeRegion_SelectionChanged (object sender, SelectionChangedEventArgs e) {
			RefreshBucketList (sender, e) ;
		}

		private Dictionary<string, List<string>> _bucketsCache =new Dictionary<string, List<string>> () ;
		private async void RefreshBucketList (object sender, RoutedEventArgs e) {
			Handled (e) ;
			string region =(string)ForgeRegion.SelectedItem ;
			try {
				State =StateEnum.Busy ;
				string startAt =null ;
				_bucketsCache [region] =new List<string> () ;
				do {
					startAt =await GetBuckets (region, startAt, 10) ;
				} while ( !string.IsNullOrEmpty (startAt) ) ;
			} finally {
				State =StateEnum.Idle ;
			}
			if ( (string)ForgeRegion.SelectedItem == region ) {
				BucketsInRegion.Items.Clear () ;
				foreach ( string name in _bucketsCache [region] )
					BucketsInRegion.Items.Add (name) ;
				BucketsInRegion.SelectedIndex =0 ;
			}
		}

		private async Task<string> GetBuckets (string region, string startAt =null, int limit =10) {
			try {
				BucketsApi ossBuckets =new BucketsApi () ;
				ossBuckets.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossBuckets.GetBucketsAsyncWithHttpInfo (
					region, limit, startAt
				) ;
				httpErrorHandler (response, "Failed to access bucket list") ;
				foreach ( KeyValuePair<string, dynamic> objInfo in new DynamicDictionaryItems (response.Data.items) ) {
					_bucketsCache [region].Add (objInfo.Value.bucketKey) ;
				}
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

		private void BucketsInRegion_SelectionChanged (object sender, SelectionChangedEventArgs e) {
			BucketContentRefresh_Click (sender, e) ;
		}

		private async void BucketContentRefresh_Click (object sender, RoutedEventArgs e) {
			Handled (e) ;
			try {
				State =StateEnum.Busy ;
				string bucket =(string)BucketsInRegion.SelectedItem ;
				ItemsSource =null ;
				string startAt =null ;
				do {
					startAt =await GetBucketContent (bucket, startAt, 10) ;
				} while ( !string.IsNullOrEmpty (startAt) ) ;
				ForgeObjects.Items.Refresh () ;
			} finally {
				State =StateEnum.Idle ;
			}
		}

		private async Task<string> GetBucketContent (string bucket, string startAt =null, int limit =10) {
			try {
				ObjectsApi ossObjects =new ObjectsApi () ;
				ossObjects.Configuration.AccessToken =accessToken ;
				ApiResponse<dynamic> response =await ossObjects.GetObjectsAsyncWithHttpInfo (
					bucket, limit, null, startAt
				) ;
				httpErrorHandler (response, "Failed to access bucket content") ;
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
						(string)BucketsInRegion.SelectedItem,
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
				string urn =URN ((string)BucketsInRegion.SelectedItem, item, false) ;
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
				ApiResponse<dynamic> response =await ossObjects.GetObjectDetailsAsyncWithHttpInfo ((string)BucketsInRegion.SelectedItem, item.Name) ;
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
				string urn =URN ((string)BucketsInRegion.SelectedItem, item) ;
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
				ApiResponse<dynamic> response =await ossObjects.GetObjectAsyncWithHttpInfo ((string)BucketsInRegion.SelectedItem, item.Properties.objectKey) ;
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
				ApiResponse<dynamic> response =await ossObjects.GetObjectAsyncWithHttpInfo ((string)BucketsInRegion.SelectedItem, item.Properties.objectKey) ;
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
				ApiResponse<dynamic> response =await ossObjects.DeleteObjectAsyncWithHttpInfo ((string)BucketsInRegion.SelectedItem, item.Name) ;
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
			string urn =URN ((string)BucketsInRegion.SelectedItem, item, true) ;
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
				string urn =URN ((string)BucketsInRegion.SelectedItem, item) ;
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

		// See DigitalSignature.cs #120
		//internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ 62, 12, 25, 249, 16 } ;
		internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD2 ={ 61, 13, 12, 24 } ;
		//internal static readonly byte[] _SaltByteArray1 ={ 8, 82, 86, 7, 78, 86, 7, 90, 75, 91, 40 } ;
		internal static readonly byte[] _SaltByteArray2 ={ 90, 74, 57, 76, 89, 45, 82, 76, 86, 92 } ;

	}

}
