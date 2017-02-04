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
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GenerateToken {

	public static class Data {

		private static string _db =@"{
			""123456789"": {
				""serial"": ""123456789"",
				""client_id"": """",
				""client_secret"": """"
			},
			""987654321"": {
				""serial"": ""987654321"",
				""client_id"": """",
				""client_secret"": """"
			}
		}" ;

		public static JObject _data =JObject.Parse (_db) ;

		public static JObject FindCustomer (string serial) {
			JObject obj =_data [serial] as JObject ;
			if ( obj == null )
				return (null) ;
			if ( string.IsNullOrEmpty (obj ["client_id"].ToString ()) )
				obj ["client_id"] =WebApiConfig.FORGE_CLIENT_ID ;
			if ( string.IsNullOrEmpty (obj ["client_secret"].ToString ()) )
				obj ["client_secret"] =WebApiConfig.FORGE_CLIENT_SECRET ;
			return (obj) ;
		}

	}

}
