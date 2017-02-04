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
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace Autodesk.Forge.WpfCsharp {

	public class DigitalSignature {

		public static bool IsSigned (string file =null) {
			try {
				if ( string.IsNullOrEmpty (file) )
					file =System.Reflection.Assembly.GetExecutingAssembly().Location ;
				X509Certificate cert =X509Certificate.CreateFromSignedFile (file) ;
				return (cert != null) ;
			} catch ( CryptographicException ex ) {
				Debug.WriteLine ("Error {0} : {1}", ex.GetType (), ex.Message) ;
				Debug.WriteLine ("Couldn’t parse the certificate. Be sure it is a X.509 certificate") ;
				return (false) ;
			}
		}

		public static byte[] HashValue (object obj, string file =null) {
			try {
				if ( string.IsNullOrEmpty (file) )
					file =System.Reflection.Assembly.GetExecutingAssembly().Location ;
				X509Certificate cert =X509Certificate.CreateFromSignedFile (file) ;
				X509Certificate2 cert2 =new X509Certificate2 (cert) ;
				RSACryptoServiceProvider csp =(RSACryptoServiceProvider)cert2.PublicKey.Key ;
				SHA1Managed sha1 =new SHA1Managed () ;
				string text =obj.ToString () ;
				byte[] data =Encoding.Unicode.GetBytes (text) ;
				byte[] hash =sha1.ComputeHash (data) ;
				return (csp.SignHash (hash, CryptoConfig.MapNameToOID ("SHA1"))) ;
			} catch ( CryptographicException /*cex*/ ) {
				return (null) ;
			} catch ( Exception /*ex*/ ) {
				return (null) ;
			}
		}

		public static byte[] Encrypt (object obj, string file =null) {
			try {
				if ( string.IsNullOrEmpty (file) )
					file =System.Reflection.Assembly.GetExecutingAssembly().Location ;
				X509Certificate cert =X509Certificate.CreateFromSignedFile (file) ;
				X509Certificate2 cert2 =new X509Certificate2 (cert) ;
				RSACryptoServiceProvider csp =(RSACryptoServiceProvider)cert2.PublicKey.Key ;
				string text =obj.ToString () ;
				byte[] data =Encoding.UTF8.GetBytes (text) ;
				byte[] encryptedData =csp.Encrypt (data, true) ;
				return (encryptedData) ;
				//return (Convert.ToBase64String (encryptedData)) ;
			} catch ( CryptographicException /*cex*/ ) {
				return (null) ;
			} catch ( Exception /*ex*/ ) {
				return (null) ;
			}
		}

		public static byte[] Decrypt (byte[] data, string file, SecureString pwd) {
			try {
				X509Certificate cert =new X509Certificate (file, pwd) ;
				X509Certificate2 cert2 =new X509Certificate2 (cert) ;
				RSACryptoServiceProvider csp =(RSACryptoServiceProvider)cert2.PrivateKey ;
				byte[] decryptedData =csp.Decrypt (data, true) ;
				return (decryptedData) ;
				//return (Encoding.UTF8.GetString (decryptedData)) ;
			} catch ( CryptographicException /*cex*/ ) {
				return (null) ;
			} catch ( Exception /*ex*/ ) {
				return (null) ;
			}
		}

		public static bool CheckToken (string assembly, byte[] expectedToken) {
			if ( assembly == null )
				throw new ArgumentNullException ("assembly") ;
			if ( expectedToken == null )
				throw new ArgumentNullException ("expectedToken") ;
			try {
				// Get the public key token of the given assembly 
				Assembly asm =Assembly.LoadFrom (assembly) ;
				byte[] asmToken =asm.GetName ().GetPublicKeyToken () ;
				// Compare it to the given token
				if ( asmToken.Length != expectedToken.Length )
					return (false) ;
				for ( int i =0 ; i < asmToken.Length ; i++ )
					if ( asmToken [i] != expectedToken [i] )
						return (false) ;
				return (true) ;
			} catch ( System.IO.FileNotFoundException ) {
				return (false) ;
			} catch ( BadImageFormatException ) {
				return (false) ;
			}
		}

	}

	// https://www.codeproject.com/questions/304187/how-to-create-password-encrypt-decrypt-in-csharp
	public class Crypto {
		#region enums, constants & fields
		// Symmetric Encyption Types
		public enum CryptoTypes {
			encTypeDES =0,
			encTypeRC2,
			encTypeRijndael,
			encTypeTripleDES
		}

		// Both values below will be assemble in construction vs encoded here (security measure)
		// fe45A4!@8 (off=40, rot=2, reverse=false) -> 62, 61, 12, 13, 25, 12, 249, 24, 16
		private readonly byte[] CRYPT_DEFAULT_PASSWORD =null ; // See App.xaml.cs #123 & MainWindow.xaml.cs #958
		// Autodesk Forge Rocks! (off=25, rot=5, reverse=true) -> 8, 90, 82, 74, 86, 57, 7, 76, 78, 89, 86, 45, 7, 82, 90, 76, 75, 86, 91, 92, 40
		private readonly byte[] _SaltByteArray =null ; // See App.xaml.cs #125 & MainWindow.xaml.cs #960
		private const CryptoTypes CRYPT_DEFAULT_METHOD =CryptoTypes.encTypeRijndael ;
		private byte[] _Key ={ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 } ; // secret key for the symmetric algorithm
		private byte[] _IV ={ 65, 110, 68, 26, 69, 178, 200, 219 } ; // vector (IV) for the symmetric algorithm

		#endregion

		#region Properties
		private CryptoTypes _CryptoType =CRYPT_DEFAULT_METHOD ;
		public CryptoTypes CryptoType {
			get {
				return (_CryptoType) ;
			}
			set {
				if ( _CryptoType != value ) {
					_CryptoType =value ;
					calculateNewKeyAndIV () ;
				}
			}
		}

		private string _Password =null ;
		public string Password {
			get {
				return (_Password) ;
			}
			set {
				if ( _Password != value ) {
					_Password =value ;
					calculateNewKeyAndIV () ;
				}
			}
		}

		#endregion

		#region Constructors

		private byte[] __rebuild__keys__ (byte[] p1, byte[] p2) {
			return (p1
				.Zip (p2, (first, second) => new { F =first, S =second })
                .SelectMany (fs => new byte[] { fs.F, fs.S } )
				.Concat (p1.Reverse ().TakeWhile ((v, i) => p1.Length % 2 == 1 && i == 0))
				.ToArray ()
			) ;
		}

		public Crypto () {
			CRYPT_DEFAULT_PASSWORD =__rebuild__keys__ (App._CRYPT_DEFAULT_PASSWORD1, MainWindow._CRYPT_DEFAULT_PASSWORD2) ;
			_SaltByteArray =__rebuild__keys__ (App._SaltByteArray1, MainWindow._SaltByteArray2) ;

			Password =GetPassword (CRYPT_DEFAULT_PASSWORD, 40, 2, false) ;
		}

		public Crypto (CryptoTypes cryptoType) {
			CRYPT_DEFAULT_PASSWORD =__rebuild__keys__ (App._CRYPT_DEFAULT_PASSWORD1, MainWindow._CRYPT_DEFAULT_PASSWORD2) ;
			_SaltByteArray =__rebuild__keys__ (App._SaltByteArray1, MainWindow._SaltByteArray2) ;

			_CryptoType =cryptoType ;
			Password =GetPassword (CRYPT_DEFAULT_PASSWORD, 40, 2, false) ;
		}

		public static string GetPassword (byte[] pwd, byte off =0, int rot =0, bool bReverse =false) {
			for ( int i =0 ; i < pwd.Length ; i++ )
				pwd [i] +=off ;
			pwd =pwd
				.SkipWhile (i => i != rot)
				.Concat (pwd.TakeWhile (i => i != rot))
				.ToArray () ;
			if ( bReverse )
				pwd =pwd.Reverse ().ToArray () ;
			return (System.Text.Encoding.UTF8.GetString (pwd)) ;
		}

		public static byte[] PasswordToBytes (string st, byte off =0, int rot =0, bool bReverse =false) {
			byte[] pwd =System.Text.Encoding.UTF8.GetBytes (st) ;
			for ( int i =0 ; i < pwd.Length ; i++ )
				pwd [i] -=off ;
			pwd =pwd
				.SkipWhile (i => i != pwd.Length - rot)
				.Concat (pwd.TakeWhile (i => i != pwd.Length - rot))
				.ToArray () ; 
			if ( bReverse )
				pwd =pwd.Reverse ().ToArray () ;
			Debug.WriteLine (st + " -> " + string.Join (", ", pwd)) ;
			return (pwd) ;
		}

		#endregion

		#region Encryption
		public string Encrypt (string inputText) {
			if ( string.IsNullOrEmpty (inputText) )
				return (inputText) ;
			UTF8Encoding UTF8Encoder =new UTF8Encoding () ;
			byte[] inputBytes =UTF8Encoder.GetBytes (inputText) ;
			return (Convert.ToBase64String (EncryptDecrypt (inputBytes, true))) ;
		}

		public string Encrypt (string inputText, string password) {
			Password =password ;
			return (Encrypt (inputText)) ;
		}

		public string Encrypt (string inputText, string password, CryptoTypes cryptoType) {
			_CryptoType =cryptoType ; // not using the Property to save time
			return (Encrypt (inputText, password)) ;
		}

		public string Encrypt (string inputText, CryptoTypes cryptoType) {
			CryptoType =cryptoType ;
			return (Encrypt (inputText)) ;
		}

		#endregion

		#region Decryption
		public string Decrypt (string inputText) {
			if ( string.IsNullOrEmpty (inputText) )
				return (inputText) ;
			byte[] inputBytes =Convert.FromBase64String (inputText) ;
			UTF8Encoding UTF8Encoder =new UTF8Encoding () ;
			return (UTF8Encoder.GetString (EncryptDecrypt (inputBytes, false))) ;
		}

		public string Decrypt (string inputText, string password) {
			Password =password ;
			return (Decrypt (inputText)) ;
		}

		public string Decrypt (string inputText, string password, CryptoTypes cryptoType) {
			_CryptoType =cryptoType ; // not using the Property to save time
			return (Decrypt (inputText, password)) ;
		}

		public string Decrypt (string inputText, CryptoTypes cryptoType) {
			CryptoType =cryptoType ;
			return (Decrypt (inputText)) ;
		}

		#endregion

		#region Symmetric Engine
		private byte[] EncryptDecrypt (byte[] inputBytes, bool Encrpyt) {
			ICryptoTransform transform =getCryptoTransform (Encrpyt) ;
			MemoryStream memStream =new MemoryStream () ;
			try {
				CryptoStream cryptStream =new CryptoStream (memStream, transform, CryptoStreamMode.Write) ;
				cryptStream.Write (inputBytes, 0, inputBytes.Length) ;
				cryptStream.FlushFinalBlock () ;
				byte[] output =memStream.ToArray () ;
				cryptStream.Close () ;
				return (output) ;
			} catch ( Exception e ) {
				throw new Exception ("Error in symmetric engine. Error : " + e.Message, e) ;
			}
		}

		private ICryptoTransform getCryptoTransform (bool encrypt) {
			SymmetricAlgorithm SA =selectAlgorithm () ;
			SA.Key =_Key ;
			SA.IV =_IV ;
			if ( encrypt )
				return (SA.CreateEncryptor ()) ;
			else
				return (SA.CreateDecryptor ()) ;
		}

		private SymmetricAlgorithm selectAlgorithm () {
			SymmetricAlgorithm SA ;
			switch ( _CryptoType ) {
				case CryptoTypes.encTypeDES:
					SA =DES.Create () ;
					break ;
				case CryptoTypes.encTypeRC2:
					SA =RC2.Create () ;
					break ;
				case CryptoTypes.encTypeRijndael:
					SA =Rijndael.Create () ;
					break ;
				case CryptoTypes.encTypeTripleDES:
					SA =TripleDES.Create () ;
					break ;
				default:
					SA =TripleDES.Create () ;
					break ;
			}
			return (SA) ;
		}

		private void calculateNewKeyAndIV () {
			// Use salt so that key cannot be found with dictionary attack
			byte[] saltByteArray =System.Text.Encoding.UTF8.GetBytes (GetPassword (_SaltByteArray, 25, 5, true)) ;
			PasswordDeriveBytes pdb =new PasswordDeriveBytes (Password, saltByteArray) ;
			SymmetricAlgorithm algo =selectAlgorithm () ;
			_Key =pdb.GetBytes (algo.KeySize / 8) ;
			_IV =pdb.GetBytes (algo.BlockSize / 8) ;
#if DEBUG
			Debug.WriteLine ("  _Key -> " + string.Join (", ", _Key)) ;
			Debug.WriteLine ("  _IV -> " + string.Join (", ", _IV)) ;

			Debug.WriteLine ("  _Key -> " + Convert.ToBase64String (_Key)) ;
			Debug.WriteLine ("  _IV -> " + Convert.ToBase64String (_IV)) ;
#endif
		}

		#endregion

	}

}
