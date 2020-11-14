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
using CefSharp;
using CefSharp.Wpf;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Autodesk.Forge.WpfCsharp {

	// https://weblog.west-wind.com/posts/2011/May/21/Web-Browser-Control-Specifying-the-IE-Version

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
			//web.Source =null ;
			//web.Load("https://forge.autodesk.com/");
			web.Dispose () ;
		}

		private void Window_Loaded (object sender, RoutedEventArgs e) {
			e.Handled =true ;
			string urn =MainWindow.URN (_item.Properties.bucketKey, _item, true) ;
			
			//string html = _template.Replace ("__URN__", urn).Replace ("__ACCESS_TOKEN__", _accessToken) ;

			string url = "https://models.autodesk.io/view.html?urn=" + urn + "&token=" + _accessToken;
			web.Load (url);
		}

		#region HTML template
		protected string _template =@"<!DOCTYPE html>
<html>
<head>
	<meta charset=""UTF-8"">
	<link rel=""stylesheet"" href=""https://developer.api.autodesk.com/modelderivative/v2/viewers/style.min.css?v=v7.*"" />
	<script src=""https://developer.api.autodesk.com/modelderivative/v2/viewers/viewer3D.min.js?v=v7.*""></script>
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
