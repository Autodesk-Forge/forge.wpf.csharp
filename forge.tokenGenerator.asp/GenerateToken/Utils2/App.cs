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
// by Augusto Goncalves
// Autodesk Forge Partner Development
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Autodesk.Forge.WpfCsharp {

	public class App {
		// See DigitalSignature.cs #120
		internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ 62, 12, 25, 249, 16 } ;
		//internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD2 ={ 61, 13, 12, 24 } ;
		internal static readonly byte[] _SaltByteArray1 ={ 8, 82, 86, 7, 78, 86, 7, 90, 75, 91, 40 } ;
		//internal static readonly byte[] _SaltByteArray2 ={ 90, 74, 57, 76, 89, 45, 82, 76, 86, 92 } ;
	}

}
