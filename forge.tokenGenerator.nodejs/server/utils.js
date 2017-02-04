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
var fs =require ('fs') ;
var path =require ('path') ;
var crypto =require ('crypto') ;

var utils ={

	path: function (pathname) {
		return (path.normalize (__dirname + '/../' + pathname)) ;
	},

	readFile: function (filename, enc) {
		return (new Promise (function (fulfill, reject) {
			fs.readFile (filename, enc, function (err, res) {
				if ( err )
					reject (err) ;
				else
					fulfill (res) ;
			}) ;
		})) ;
	},

	writeFile: function (filename, content, enc, bRaw) {
		return (new Promise (function (fulfill, reject) {
			var pathname =path.dirname (filename) ;
			utils.mkdirp (pathname)
				.then (function (pathname) {
					fs.writeFile (filename, !bRaw && typeof content !== 'string' ? JSON.stringify (content) : content, enc, function (err) {
						if ( err )
							reject (err) ;
						else
							fulfill (content) ;
					}) ;
				})
			;
		})) ;
	},

	filesize: function (filename) {
		return (new Promise (function (fulfill, reject) {
			fs.stat (filename, function (err, stat) {
				if ( err )
					reject (err) ;
				else
					fulfill (stat.size) ;
			}) ;
		})) ;
	},

	fileexists: function (filename) {
		return (new Promise (function (fulfill, reject) {
			fs.stat (filename, function (err, stat) {
				if ( err ) {
					if ( err.code === 'ENOENT' )
						fulfill (false) ;
					else
						reject (err) ;
				} else {
					fulfill (true) ;
				}
			}) ;
		})) ;
	},

	unlink: function (filename) {
		return (new Promise (function (fulfill, reject) {
			fs.stat (filename, function (err, stat) {
				if ( err ) {
					if ( err.code === 'ENOENT' )
						fulfill (false) ;
					else
						reject (err) ;
				} else {
					fs.unlink (filename, function (err) {}) ;
					fulfill (true) ;
				}
			}) ;
		})) ;
	},

	isCompressed: function (filename) {
		return (   path.extname (filename).toLowerCase () == '.zip'
				|| path.extname (filename).toLowerCase () == '.rar'
				|| path.extname (filename).toLowerCase () == '.gz'
		) ;
	},

	safeBase64encode: function (st) {
		return (new Buffer (st).toString ('base64')
			.replace (/\+/g, '-') // Convert '+' to '-'
			.replace (/\//g, '_') // Convert '/' to '_'
			.replace (/=+$/, '')
		) ;
	},

	safeBase64decode: function (base64) {
		// Add removed at end '='
		base64 +=Array (5 - base64.length % 4).join('=') ;
		base64 =base64
			.replace (/\-/g, '+')   // Convert '-' to '+'
			.replace (/\_/g, '/') ; // Convert '_' to '/'
		return (new Buffer (base64, 'base64').toString ()) ;
	},

	readdir: function (pathname) {
		return (new Promise (function (fulfill, reject) {
			fs.readdir (pathname, function (err, files) {
				if ( err )
					reject (err) ;
				else
					fulfill (files) ;
			}) ;
		})) ;
	},

	encrypt: function (toEncrypt, publicKey) {
		var buffer =new Buffer (toEncrypt) ;
		var encrypted =crypto.publicEncrypt (publicKey, buffer) ;
		return (encrypted.toString ('base64')) ;
	},

	decrypt: function (toDecrypt, privateKey) {
		var buffer =new Buffer (toDecrypt, 'base64') ;
		var decrypted =crypto.privateDecrypt (privateKey, buffer) ;
		return (decrypted.toString ('utf8')) ;
	}

} ;

module.exports =utils ;
