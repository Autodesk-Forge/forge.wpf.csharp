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
using System.Windows;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Autodesk.Maya;

namespace Autodesk.Forge.WpfCsharp {

	// Security concerns
	//
	// You should better never store your oAuth client secret or any confidential information
	// in a .Net application as someone can diassemble the application and read the code to 
	// figure out the strings. Even without dissemblying, someone can use a Hex viewer and 
	// parse the binary.
	//
	// Here, we are going to demo few techniques to make hard enough for a hacker to consider
	// it is not worse the effort.
	// - Encrypt few important strings vs leaving them in clear
	// - Obfuscate the exe
	//   https://yck1509.github.io/ConfuserEx/
	//   https://www.codeproject.com/Articles/1040107/Configuring-Visual-Studio-for-Obfuscation
	//   Decompile - http://www.red-gate.com/products/dotnet-development/reflector/
	//               http://ilspy.net/
	// - Digital Sign the exe - we cannot use a strong name certificate because all dependencies
	//   would need to be strong name signed too which is not the case here
	//   https://msdn.microsoft.com/en-us/library/aa559684(v=bts.10).aspx
	//   http://stackoverflow.com/questions/331520/how-to-fix-referenced-assembly-does-not-have-a-strong-name-error
	//   http://ryanfarley.com/blog/archive/2010/04/23/sign-a-.net-assembly-with-a-strong-name-without-recompiling.aspx
	// - Use a WEB service to generate the token, and use the asymetric certificate to encrypt
	//   and decrypt data send to the WEB service - using date and salt appraoches. If not using
	//   a certificate, default back into a less secure symetric approach.

	// App Initialize
	// - Encrypt using serial #, and send a request to the WEB service
	//   -> WEB service replies with encrypted client_id, pwd, offset and rot all in SecureString
	//      -> pwd, offset and rot -> reinit MainWindow._Crypto
	//      -> client_id

	// .settings stored at %userprofile%\appdata\local\Autodesk
	public partial class App : Application {
		public bool IsSigned { get; internal set; } =false ;
		
		public void App_Startup (object sender, StartupEventArgs args) {
			try {
#if !DEBUG
				DebugUtils.StartDebuggerThread () ;
#else
				TestCrypto () ;
#endif
				IsSigned =DigitalSignature.IsSigned () ;
				bool bSuccess =MayaTheme.Initialize (this) ;
			} catch ( System.Exception ex ) {
				MessageBox.Show (ex.Message, "Error during initialization. This program will exit") ;
				Application.Current.Shutdown () ;
			}
		}

#if DEBUG
		public void TestCrypto () {
			Crypto crypto =new Crypto (WpfCsharp.Crypto.CryptoTypes.encTypeTripleDES) ;

			string pwd ="fe45A4!@8" ;
			byte[] bytes =Crypto.PasswordToBytes (pwd, 40, 2, false) ;
			byte[] p1 =bytes.Where ((value, index) => index % 2 == 0).ToArray () ;
			byte[] p2 =bytes.Where ((value, index) => index % 2 != 0).ToArray () ;
			Debug.WriteLine ("->" + Crypto.GetPassword (bytes, 40, 2, false)) ;
			Debug.WriteLine (" internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ " + string.Join (", ", p1) + " }") ;
			Debug.WriteLine (" internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ " + string.Join (", ", p2) + " }") ;
			byte[] test =p1.Zip (p2, (first, second) => new { F =first, S =second })
                  .SelectMany (fs => new byte[] { fs.F, fs.S } )
				  .Concat (p1.Reverse ().TakeWhile ((v, i) => p1.Length % 2 == 1 && i == 0))
				  .ToArray () ;
			Debug.WriteLine ("  x -> " + string.Join (", ", test)) ;

			string salt ="Autodesk Forge Rocks!" ;
			bytes =Crypto.PasswordToBytes (salt, 25, 5, true) ;
			p1 =bytes.Where ((value, index) => index % 2 == 0).ToArray () ;
			p2 =bytes.Where ((value, index) => index % 2 != 0).ToArray () ;
			Debug.WriteLine ("->" + Crypto.GetPassword (bytes, 25, 5, true)) ;
			Debug.WriteLine (" internal static readonly byte[] _SaltByteArray1 ={ " + string.Join (", ", p1) + " }") ;
			Debug.WriteLine (" internal static readonly byte[] _SaltByteArray2 ={ " + string.Join (", ", p2) + " }") ;
			test =p1.Zip (p2, (first, second) => new { F =first, S =second })
                  .SelectMany (fs => new byte[] { fs.F, fs.S } )
				  .Concat (p1.Reverse ().TakeWhile ((v, i) => p1.Length % 2 == 1 && i == 0))
				  .ToArray () ;
			Debug.WriteLine ("  x -> " + string.Join (", ", test)) ;

			string st ="This is a test line" ;
			string result =crypto.Encrypt (st) ;
			Debug.WriteLine ("test encrypted: " + result) ;
			result =crypto.Decrypt (result) ;
			Debug.WriteLine ("test decrypted: " + result) ;

			Debug.WriteLine ("\n") ;
		}
#endif

		// See DigitalSignature.cs #120
		internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ 62, 12, 25, 249, 16 } ;
		//internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD2 ={ 61, 13, 12, 24 } ;
		internal static readonly byte[] _SaltByteArray1 ={ 8, 82, 86, 7, 78, 86, 7, 90, 75, 91, 40 } ;
		//internal static readonly byte[] _SaltByteArray2 ={ 90, 74, 57, 76, 89, 45, 82, 76, 86, 92 } ;

	}

}
