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
var path =require ('path') ;

var config ={
	credentials: {
		// Replace placeholder below by the Consumer Key and Consumer Secret you got from
		// http://developer.autodesk.com/ for the production server
		client_id: process.env.FORGE_CLIENT_ID || '<replace with your consumer key>',
		client_secret: process.env.FORGE_CLIENT_SECRET || '<replace with your consumer secret>',
		grant_type: 'client_credentials',
		scope: ['data:read', 'data:write', 'data:create', 'data:search',
				'bucket:read', 'bucket:create', 'bucket:update', 'bucket:delete' ]
	},

	certificates: {
		'private': process.env.PRIVATE_CERTIFICATE || path.normalize (__dirname + '/../../../private.cer'),
		'public': process.env.PUBLIC || path.normalize (__dirname + '/../../../public.cer')
	}
} ;

module.exports =config ;
