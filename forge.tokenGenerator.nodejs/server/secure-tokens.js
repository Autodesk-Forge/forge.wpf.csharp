//
// Copyright (c) Autodesk, Inc. All rights reserved
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
//
// by Cyrille Fauvel - Autodesk Developer Network (ADN)
//
var express =require ('express') ;
var bodyParser =require ('body-parser') ;
var path =require ('path') ;
var moment =require ('moment') ;
var ForgeSDK =require ('forge-apis') ;
var utils =require ('./utils') ;
var crypto =require ('./encryption') ;

var config =require ('./config') ;
var data =require ('./data') ;

var router =express.Router () ;
//router.use (bodyParser.json ()) ;
router.use (bodyParser.raw ()) ;

router.post ('/generateToken/:serial', function (req, res) {
	var serial =req.params.serial ;
	if ( !data.hasOwnProperty (serial) )
		return (res.status (404). end ()) ;
	utils.readFile (config.certificates.private)
		.then (function (text) {
			var st =req.body.toString ('utf8') ;
			var json =null ;
			try {
				json =utils.decrypt (st, text) ;
				//console.log (json) ;
				json =JSON.parse (json) ;
				console.log ('Got a request: ' + JSON.stringify (json)) ;
			} catch ( ex ) {
				return (res.status (500).end ()) ;
			}
			var reqMoment =moment (json.other) ;
			var timestamp =moment.utc ().add (moment.duration ('00:00:30')) ;
			var timestamp2 =moment.utc ().subtract (moment.duration ('00:00:30')) ;
			if (   json.serial !== serial
				|| !reqMoment.isBetween (timestamp2, timestamp)
			) {
				console.log ('Rejecting request!') ;
				console.log (timestamp.toISOString()) ;
				return (res.status (404).end ()) ;
			}

			if ( !data.hasOwnProperty (serial) )
				return (res.status (404).end ()) ;
			var ref =data [serial] ;
			var credentials =config.credentials ;
			credentials.client_id =ref.client_id ;
			credentials.client_secret =ref.client_secret ;

			var oAuth2TwoLegged =new ForgeSDK.AuthClientTwoLegged (credentials.client_id, credentials.client_secret, credentials.scope) ;
			oAuth2TwoLegged.authenticate ()
				.then (function (response) {
					console.log ('Token: ' + response.access_token) ;
					response.access_token =crypto.Encrypt (response.access_token) ;
					res.json (response) ;
				})
				.catch (function (error) {
					console.error ('Token: ERROR! ', error) ;
					res.status (500).end () ;
				}) ;
		}) ;
}) ;

// This endpoint is there only to quickly test the one above, should not appear in production
if ( process.env.NODE_ENV !== "production" ) {
router.get ('/test1', function (req, res) {
	var app =require ('./server') ; // Node.js supports circular dependencies.
	utils.readFile (config.certificates.public)
		.then (function (text) {
			var serialNumber ='123456789' ;
			var obj ={
				serial: serialNumber,
				other: moment.utc ()/*.format ('YYYY-MM-DD')*/
			} ;
			var st =utils.encrypt (JSON.stringify (obj), text) ;

			var http =require ('http') ;
			var options ={
				host: 'localhost',
				port: app.get ('port'),
				path: '/generateToken/' + serialNumber,
				method: 'POST',
				headers: {
					'Content-Type': 'application/octet-stream',
					'Content-Length': Buffer.byteLength (st)
				}
			} ;
			var body ='' ;
			var req =http.request (options, function (res2) {
				res2.setEncoding ('utf8') ;
				res2.on ('data', function (chunk) {
					body +=chunk ;
				}) ;
				res2.on ('end', function () {
					console.log (body) ;
					try {
						var json =JSON.parse (body) ;
						json.tokenServer =json.access_token ;
						json.access_token =crypto.Decrypt (json.access_token) ;
						res.json (json) ;
					} catch ( ex ) {
						res.json ({}) ;
					}
				}) ;
			}) ;
			req.write (st) ;
			req.end () ;
		}) ;
}) ;
}

module.exports =router ;
