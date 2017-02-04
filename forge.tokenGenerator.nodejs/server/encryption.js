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
var utils =require ('./utils') ;

var CryptoJS =require ('crypto-js') ;

// http://stackoverflow.com/questions/34061663/triple-des-encryption-in-javascript-and-decryption-in-php

var Encryption = {
	_Key: CryptoJS.enc.Base64.parse ('m31a39purvhDyhmlW1kGsqkFbmJ1a38n'),
	_IV: CryptoJS.enc.Base64.parse ('6ykFW6/RIxw='),

	Encrypt: function (st) {
		var textWordArray = CryptoJS.enc.Utf8.parse (st);
		var options = {
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7,
			iv: Encryption._IV
		} ;
		var encrypted =CryptoJS.TripleDES.encrypt (textWordArray, Encryption._Key, options) ;
		return (encrypted.toString ());
	},

	Decrypt: function (st) {
		var options = {
			mode: CryptoJS.mode.CBC,
			padding: CryptoJS.pad.Pkcs7,
			iv: Encryption._IV
		} ;
		var decrypted =CryptoJS.TripleDES.decrypt ({
				ciphertext: CryptoJS.enc.Base64.parse (st)
			},
			Encryption._Key,
			options
		) ;
		return (decrypted.toString (CryptoJS.enc.Utf8)) ;
	}

} ;

//var st ="This is a test line" ;
//var result =crypto.Encrypt (st) ;
//console.log ("test encrypted: " + result) ;
//result =crypto.Decrypt (result) ;
//console.log ("test decrypted: " + result) ;

// C# - http://stackoverflow.com/questions/20247953/tripledes-implementation-in-javascript-different-comparing-with-c-sharp
// test encrypted: QnDCoP8NsQDXk6DENe9Gn33Cs/3D/H7C
// test decrypted: This is a test line
// Js
// test encrypted: QnDCoP8NsQDXk6DENe9Gn33Cs/3D/H7C
// test decrypted: This is a test line

module.exports =Encryption ;