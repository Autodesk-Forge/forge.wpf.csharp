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
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using Autodesk.Maya;
using CefSharp;
using CefSharp.Wpf;

namespace Autodesk.Forge.WpfCsharp {

	public partial class App : Application {
		
		public void App_Startup (object sender, StartupEventArgs args) {
			try {
				bool bSuccess =MayaTheme.Initialize (this) ;

				AppDomain.CurrentDomain.AssemblyResolve += Resolver;
				// Any CefSharp references have to be in another method with NonInlining
				// attribute so the assembly rolver has time to do it's thing.
				InitializeCefSharp ();
			} catch ( System.Exception ex ) {
				MessageBox.Show (ex.Message, "Error during initialization. This program will exit") ;
				Application.Current.Shutdown () ;
			}
		}

        // https://github.com/cefsharp/CefSharp/issues/1714
        [MethodImpl (MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp () {
            var settings = new CefSettings ();

            // Set BrowserSubProcessPath based on app bitness at runtime
            settings.BrowserSubprocessPath = Path.Combine (AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                   Environment.Is64BitProcess ? "x64" : "x86",
                                                   "CefSharp.BrowserSubprocess.exe");

            // Make sure you set performDependencyCheck false
            Cef.Initialize (settings, performDependencyCheck: false, browserProcessHandler: null);
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        // Required by CefSharp to load the unmanaged dependencies when running using AnyCPU
        private static Assembly Resolver (object sender, ResolveEventArgs args) {
            if ( args.Name.StartsWith ("CefSharp") ) {
                string assemblyName = args.Name.Split (new [] { ',' }, 2) [0] + ".dll";
                string archSpecificPath = Path.Combine (AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return (File.Exists (archSpecificPath)
                           ? Assembly.LoadFile (archSpecificPath)
                           : null);
            }

            return (null);
        }

    }

}
