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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Autodesk.Forge;

namespace GenerateToken.Controllers {

	public class TokenController : ApiController {

		[HttpPost]
		[Route ("generateToken/{serial}")]
		public async Task<JObject> generateToken (string serial) {
			HttpRequest req =HttpContext.Current.Request ;
			if ( string.IsNullOrWhiteSpace (serial) )
				throw new HttpResponseException (Request.CreateErrorResponse (HttpStatusCode.NotFound, "Invalid serial")) ;

			//string path =Path.GetFullPath (HttpContext.Current.Server.MapPath ("~/") + WebConfigurationManager.AppSettings ["CERTIFICATE"]) ;
			string path =Path.GetFullPath (HttpContext.Current.Server.MapPath ("~/") + WebApiConfig.PRIVATE_CERTIFICATE) ;
			if ( !File.Exists (path) )
				throw new HttpResponseException (Request.CreateErrorResponse (HttpStatusCode.InternalServerError, "Certification error?")) ;

			// Get the body and encode UTF8
			byte[] st =await Request.Content.ReadAsByteArrayAsync () ;
			string responseString =Encoding.UTF8.GetString (st) ;
			byte[] base64 =Convert.FromBase64String (responseString) ;
			// Run decrypt
			byte[] jsonBytes =DigitalSignature.Decrypt (base64, path, WebApiConfig._certificatePwd) ;
			string jsonString =Encoding.UTF8.GetString (jsonBytes) ;
			// Use newtonsoft.json to deserialize it
			//dynamic jsonObject =JsonConvert.DeserializeObject (jsonString) ;
			JObject jsonObject =JObject.Parse (jsonString) ;
			// Check the object?
			DateTime reqMoment =DateTime.Parse (jsonObject ["other"].ToString ()) ;
			DateTime timestamp =DateTime.UtcNow.Add (new TimeSpan (0, 0, 30)) ;
			DateTime timestamp2 =DateTime.UtcNow.Subtract (new TimeSpan (0, 0, 30)) ;

			if (   jsonObject ["serial"].ToString () != serial
				|| reqMoment < timestamp2
				|| reqMoment > timestamp
			)
				throw new HttpResponseException (Request.CreateErrorResponse (HttpStatusCode.NotFound, "This is not valid")) ;

			JObject customer =Data.FindCustomer (serial) ;
			if ( customer == null )
				throw new HttpResponseException (Request.CreateErrorResponse (HttpStatusCode.NotFound, "This is not valid")) ;

			// Getting credentials from web.config file...
			TwoLeggedApi apiInstance =new TwoLeggedApi () ;
			Scope[] scope =new Scope[] {
				Scope.DataRead, Scope.DataWrite, Scope.DataCreate, Scope.DataSearch,
				Scope.BucketCreate, Scope.BucketRead, Scope.BucketUpdate, Scope.BucketDelete } ;

			try {
				dynamic bearer =await apiInstance.AuthenticateAsync (
				  customer ["client_id"].ToString (),
				  customer ["client_secret"].ToString (),
				  oAuthConstants.CLIENT_CREDENTIALS,
				  scope
				) ;
				bearer.access_token =WebApiConfig._Crypto.Encrypt (bearer.access_token) ;
				return (bearer.ToJson ()) ;
			} catch {
				throw new HttpResponseException (Request.CreateErrorResponse (HttpStatusCode.InternalServerError, "Token: ERROR!")) ;
			}
		}

	}

}

