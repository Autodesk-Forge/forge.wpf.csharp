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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Windows;

namespace Autodesk.Forge.WpfCsharp {

	public class ItemProperties : INotifyPropertyChanged {

		#region Properties
		[Category("Bucket")]
		[DisplayName("Name")]
		[Description("Bucket Name on OSS")]
		public string Bucket { get; private set; }
		[Category("Bucket")]
		[DisplayName("Region")]
		[Description("Region where sits the bucket")]
		public string Region { get; private set; }
		[Category("Object")]
		[DisplayName("Object Key")]
		[Description("Object Key used to store the file on OSS")]
		public string ObjectKey { get; private set; }
		[Category("Object")]
		[DisplayName("Object ID")]
		[Description("Object ID used to store the file on OSS")]
		public string ObjectID { get; private set; }
		[Category("Object")]
		[DisplayName("Size")]
		[Description("Object size on OSS")]
		public string Size { get; private set; }
		[Category("Object")]
		[DisplayName("SHA1 hash code")]
		[Description("Object SHA-1 (Secure Hash Algorithm 1) on OSS")]
		public string SHA1 { get; private set; }
		[Category("Object")]
		[DisplayName("Location URL")]
		[Description("Object location URL on OSS")]
		public string Location { get; private set; }

		[Category("Manifest")]
		[DisplayName("Version")]
		[Description("Manifest Version")]
		[Browsable(true)]
		public string Version { get; private set; }
		[Category("Manifest")]
		[DisplayName("Has Thumbnail")]
		[Description("Derivative has Thumbnails")]
		[Browsable(true)]
		public bool HasThumbnail { get; private set; }
		[Category("Manifest")]
		[DisplayName("Status")]
		[Description("Status for requested translation")]
		[Browsable(true)]
		public string Status { get; private set; }
		[Category("Manifest")]
		[DisplayName("Progress")]
		[Description("Progress for requested translation")]
		[Browsable(true)]
		public string Progress { get; private set; }
		[Category("Manifest")]
		[DisplayName("Type")]
		[Description("Derivative Type")]
		[Browsable(true)]
		public string DerivativeType { get; private set; }
		[Category("Manifest")]
		[DisplayName("Name")]
		[Description("Derivative Name")]
		[Browsable(true)]
		public string DerivativeName { get; private set; }
		[Category("Manifest")]
		[DisplayName("Register Key")]
		[Description("Derivative Register Job Key")]
		[Browsable(true)]
		public string RegisterKey { get; private set; }
		#endregion

		#region Constructors
		public ItemProperties (ForgeObjectInfo item) {
			AssignProperties (item) ;
			item.PropertyChanged +=PropertyHasChanged ;
		}

		protected ItemProperties () {
		}

		#endregion

		protected void AssignProperties (ForgeObjectInfo item) {
			Bucket =item.Properties.bucketKey ;
			Region =item.Properties.region ;
			ObjectKey =item.Properties.objectKey ;
			ObjectID =item.Properties.objectId ;
			Size =MainWindow.SizeSuffix (item.Properties.size) ;
			SHA1 =item.Properties.sha1 ;
			Location =item.Properties.location ;

			bool bVersion =false ;
			bool bRegisterKey =false ;
			if ( item.Manifest != null ) {
				bVersion =MainWindow.hasOwnProperty (item.Manifest, "version") ;
				bRegisterKey =MainWindow.hasOwnProperty (item.Manifest, "registerKeys") ;
				if ( bVersion ) {
					Version =item.Manifest.version ;
					HasThumbnail =bool.Parse (item.Manifest.derivatives [0].hasThumbnail) ;
					Status =item.Manifest.status ;
					Progress =item.Manifest.progress ;
					DerivativeType =item.Manifest.derivatives [0].outputType ;
					DerivativeName =item.Manifest.derivatives [0].name ;
				} else if ( bRegisterKey ) {
					RegisterKey =item.Manifest.registerKeys [0] ;
					Status =item.Manifest.result ;
					DerivativeType =item.Manifest.acceptedJobs.output.formats [0].type ;
				}
			}
			ShowProperty ("Version", bVersion) ;
			ShowProperty ("HasThumbnail", bVersion) ;
			ShowProperty ("Status", bVersion || bRegisterKey) ;
			ShowProperty ("Progress", bVersion) ;
			ShowProperty ("DerivativeType", bVersion || bRegisterKey) ;
			ShowProperty ("DerivativeName", bVersion) ;
			ShowProperty ("RegisterKey", bRegisterKey) ;
		}

		protected void PropertyHasChanged (object sender, PropertyChangedEventArgs e) {
			Debug.WriteLine ("PropertyHasChanged: " + e.PropertyName) ;
			AssignProperties (sender as ForgeObjectInfo) ;
			//(Application.Current.MainWindow as MainWindow).propertyGrid.Update () ;
			(Application.Current.MainWindow as MainWindow).propertyGrid.SelectedObject =null ;
			(Application.Current.MainWindow as MainWindow).propertyGrid.SelectedObject =this ;
		}

		#region Utils
		private void ShowProperty (string name, bool bShow =true) {
			PropertyDescriptor descriptor =TypeDescriptor.GetProperties (this.GetType ()) [name] ;
			BrowsableAttribute attrib =(BrowsableAttribute)descriptor.Attributes [typeof (BrowsableAttribute)] ;
			FieldInfo isBrowsable =attrib.GetType ().GetField ("browsable", BindingFlags.NonPublic | BindingFlags.Instance) ;
			isBrowsable.SetValue (attrib, bShow) ;
		}

		#endregion

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


}
