using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Maya;

namespace Autodesk.Forge.WpfCsharp {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		
		public void App_Startup (object sender, StartupEventArgs args) {
			try {
				bool bSuccess =MayaTheme.Initialize (this) ;
			} catch ( System.Exception ex ) {
				MessageBox.Show (ex.Message, "Error during initialization. This program will exit") ;
				Application.Current.Shutdown () ;
			}
		}

	}

}
