// (C) Copyright 2014 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.

//- Written by Cyrille Fauvel, Autodesk Developer Network (ADN)
//- http://www.autodesk.com/joinadn
//- January 20th, 2014
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
using System.Net;

using RestSharp;

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
