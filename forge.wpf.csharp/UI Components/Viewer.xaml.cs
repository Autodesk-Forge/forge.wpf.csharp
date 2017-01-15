using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Autodesk.Forge.WpfCsharp {

	// https://weblog.west-wind.com/posts/2011/May/21/Web-Browser-Control-Specifying-the-IE-Version

	/// <summary>
	/// Interaction logic for Viewer.xaml
	/// </summary>
	public partial class Viewer : Window {
		private ForgeObjectInfo _item =null ;
		private string _accessToken =null ;

		protected Viewer () {
			InitializeComponent () ;
		}

		public Viewer (ForgeObjectInfo item, string accessToken) {
			InitializeComponent () ;
			_item =item ;
			_accessToken =accessToken ;
			this.Closing +=Viewer_Closing ;
		}

		private void Viewer_Closing (object sender, System.ComponentModel.CancelEventArgs e) {
			web.Source =null ;
			web.NavigateToString ("<html><body></body></html>") ;
			//web.Dispose () ;
		}

		private void Window_Loaded (object sender, RoutedEventArgs e) {
			e.Handled =true ;
			string urn =MainWindow.URN (_item.Properties.bucketKey, _item, true) ;
			string st =_template.Replace ("__URN__", urn).Replace ("__ACCESS_TOKEN__", _accessToken) ;
			web.NavigateToString (st) ;
			//string url ="https://models.autodesk.io/view.html?urn=" + urn + "&accessToken=" + _accessToken ;
			//web.Navigate (new Uri(url)) ;
		}

		#region HTML template
		protected string _template =@"<!DOCTYPE html>
<html>
<head>
	<meta charset=""UTF-8"">
	<script src=""https://developer.api.autodesk.com/viewingservice/v1/viewers/three.min.css""></script>
	<link rel=""stylesheet"" href=""https://developer.api.autodesk.com/viewingservice/v1/viewers/style.min.css"" />
	<script src=""https://developer.api.autodesk.com/viewingservice/v1/viewers/viewer3D.min.js""></script>
</head>
<body onload=""initialize()"">
<div id=""viewer"" style=""position:absolute; width:90%; height:90%;""></div>
<script>
	function authMe () { return ('__ACCESS_TOKEN__') ; }

	function initialize () {
		var options ={
			'document' : ""urn:__URN__"",
			'env': 'AutodeskProduction',
			'getAccessToken': authMe
		} ;
		var viewerElement =document.getElementById ('viewer') ;
		//var viewer =new Autodesk.Viewing.Viewer3D (viewerElement, {}) ; / No toolbar
		var viewer =new Autodesk.Viewing.Private.GuiViewer3D (viewerElement, {}) ; // With toolbar
		Autodesk.Viewing.Initializer (options, function () {
			viewer.initialize () ;
			loadDocument (viewer, options.document) ;
		}) ;
	}
	function loadDocument (viewer, documentId) {
		// Find the first 3d geometry and load that.
		Autodesk.Viewing.Document.load (
			documentId,
			function (doc) { // onLoadCallback
				var geometryItems =[] ;
				geometryItems =Autodesk.Viewing.Document.getSubItemsWithProperties (
					doc.getRootItem (),
					{ 'type' : 'geometry', 'role' : '3d' },
					true
				) ;
				if ( geometryItems.length <= 0 ) {
					geometryItems =Autodesk.Viewing.Document.getSubItemsWithProperties (
						doc.getRootItem (),
						{ 'type': 'geometry', 'role': '2d' },
						true
					) ;
				}
				if ( geometryItems.length > 0 )
					viewer.load (
						doc.getViewablePath (geometryItems [0])//,
						//null, null, null,
						//doc.acmSessionId /*session for DM*/
					) ;
			},
			function (errorMsg) { // onErrorCallback
				alert(""Load Error: "" + errorMsg) ;
			}//,
			//{
            //	'oauth2AccessToken': authMee (),
            //	'x-ads-acm-namespace': 'WIPDM',
            //	'x-ads-acm-check-groups': 'true',
        	//}
		) ;
	}
</script>
</body>
</html>" ;

		#endregion

	}

}
