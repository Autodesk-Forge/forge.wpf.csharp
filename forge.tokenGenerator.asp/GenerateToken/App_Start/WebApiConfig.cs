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
// by Augusto Goncalves, Cyrille Fauvel
// Autodesk Forge Partner Development
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Web.Configuration;
using System.Web.Http;

namespace GenerateToken {

	public static class WebApiConfig {

		public static SecureString _certificatePwd =new SecureString () ;
		public static string PRIVATE_CERTIFICATE =null ;
		public static string FORGE_CLIENT_ID =null ;
		public static string FORGE_CLIENT_SECRET =null ;
		public static Crypto _Crypto =new Crypto (GenerateToken.Crypto.CryptoTypes.encTypeTripleDES) ;

		public static void Register (HttpConfiguration config) {

			// http://stackoverflow.com/questions/29416302/how-do-you-put-environmental-variables-in-web-config
			PRIVATE_CERTIFICATE =Environment.ExpandEnvironmentVariables (WebConfigurationManager.AppSettings ["PRIVATE_CERTIFICATE"]) ;
			FORGE_CLIENT_ID =Environment.ExpandEnvironmentVariables (WebConfigurationManager.AppSettings ["FORGE_CLIENT_ID"]) ;
			FORGE_CLIENT_SECRET =Environment.ExpandEnvironmentVariables (WebConfigurationManager.AppSettings ["FORGE_CLIENT_SECRET"]) ;

			// Web API configuration and services
			foreach ( char c in Environment.ExpandEnvironmentVariables (WebConfigurationManager.AppSettings ["CERTIFICATE_PASSWORD"]) )
			    _certificatePwd.AppendChar (c) ;

			// Web API routes
			config.MapHttpAttributeRoutes () ;

			//config.Routes.MapHttpRoute (
			//	name: "Forge",
			//	routeTemplate: "api/{controller}",
			//	defaults: new { id = RouteParameter.Optional }
			//) ;
		}

	}

}
