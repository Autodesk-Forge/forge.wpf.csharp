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
var data ={
	'123456789': {
		serial: '123456789',
		client_id: '',
		client_secret: ''
	},
	'987654321': {
		serial: '987654321',
		client_id: '',
		client_secret: ''
	}
} ;

console.log ('Forge client id: ' + process.env.FORGE_CLIENT_ID) ;
console.log ('TOKEN_URL: http://192.168.1.16:3000/generateToken/') ;
Object.keys (data).forEach (function (key) {
	var val =data [key] ;
	val.client_id =val.client_id || process.env.FORGE_CLIENT_ID ;
	val.client_secret =val.client_secret || process.env.FORGE_CLIENT_SECRET ;
}) ;

module.exports =data ;