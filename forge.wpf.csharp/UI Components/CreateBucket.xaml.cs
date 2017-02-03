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

using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;

namespace Autodesk.Forge.WpfCsharp {

	public partial class CreateBucket : Window {

		public CreateBucket () {
			InitializeComponent () ;
		}

		private void Window_Initialized (object sender, EventArgs e) {
			var values =Enum.GetValues (typeof (PostBucketsPayload.PolicyKeyEnum)) ;
			foreach ( var value in values )
				BucketType.Items.Add (value.ToString ()) ;
			BucketType.SelectedItem =PostBucketsPayload.PolicyKeyEnum.Persistent.ToString () ;

			values =Enum.GetValues (typeof (MainWindow.Region)) ;
			foreach ( var value in values )
				BucketRegion.Items.Add (value.ToString ()) ;
			BucketRegion.SelectedItem =MainWindow.Region.US.ToString () ;
		}

		private void OKButton_Click (object sender, RoutedEventArgs e) {
			e.Handled =true ;
			DialogResult =true ;
			Close () ;
		}

		private void BucketName_TextChanged (object sender, TextChangedEventArgs e) {
			string input =(sender as TextBox).Text ;
			 if ( !System.Text.RegularExpressions.Regex.IsMatch (input, @"^[-_.a-z0-9]{3,128}$") )
				(sender as TextBox).Background =Brushes.Red ;
			 else
				(sender as TextBox).Background =null ;
		}

	}

}
