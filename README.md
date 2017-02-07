
[![build status](https://api.travis-ci.org/cyrillef/models.autodesk.io.png)](https://travis-ci.org/cyrillef/models.autodesk.io)
[![.Net](https://img.shields.io/badge/.Net-4.5-blue.svg)](https://msdn.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg)](https://www.nuget.org/)
![Platforms](https://img.shields.io/badge/platform-windows%20%7C%20osx%20%7C%20linux-lightgray.svg)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)

*Forge API*:
[![oAuth2](https://img.shields.io/badge/oAuth2-v1-green.svg)](http://developer-autodesk.github.io/)
[![Data-Management](https://img.shields.io/badge/Data%20Management-v1-green.svg)](http://developer-autodesk.github.io/)
[![OSS](https://img.shields.io/badge/OSS-v2-green.svg)](http://developer-autodesk.github.io/)
[![Model-Derivative](https://img.shields.io/badge/Model%20Derivative-v2-green.svg)](http://developer-autodesk.github.io/)
[![Viewer](https://img.shields.io/badge/Forge%20Viewer-v2.12-green.svg)](http://developer-autodesk.github.io/)

# forge.wpf-csharp


<b>Note:</b> For using this sample, you need a valid oAuth credential.
Visit this [page](https://developer.autodesk.com) for instructions to get on-board.


Demonstrates the Autodesk Forge API authorisation and translation process using a C#/.Net WPF application.

* using 2 legged oAuth
* uses asynchronous methods
* includes some security mechanism to protect the Forge keys


## Security considerations

This version unlike the 'master' branch version does implement some basic security measures to protect your
Forge keys. However, a good hacker can still find a way to overcome the protection shown here. The instructions
below tells you to use environment variables vs hardcoding them in the code. Till it is possible to hardcode
them in the code, someone can either:

  - disassemble your code,
  - debug your code
  - use an Hex editor (like this [one](https://www.microsoft.com/en-us/store/p/hex-editor-pro/9wzdncrdq8l3))
  - use .Net Reflection
  - ...

to find your keys and use them in his own application. **You need to use at least a tool like [ConfuserEx](https://yck1509.github.io/ConfuserEx/)** to protect them. This tool will help to protect your keys on the
4 obvious ways to hack your keys mentioned above.

When you use the Configuration dialog and check the 'save in app settings' option, Data are saved on disk in
%LOCALAPPDATA%\Autodesk\Autodesk.Forge.WpfCsharp...\1.0.0.0\user.config. All data is saved there, but this version
of the code will encrypt all the important data such as the access token, secret key, and token url. The technique
used here is a Symetric Algomrithm encryption, so you will need to choose a keywork and SALT string during setup to
guarantee the uniqueness of the encypted data.

Here is a quick description of the techniques used on this version:

  1. There is Node.js token service (or ASP.Net – both example provided) which will request an access_token
     from the Forge server on behalf of the C# application. This web service holds the client_id and client_secret.
     It returns an encrypted (with salt) access_token to the application.
 
  2. The C# application has a serial# (in clear) and an encrypted (with salt) URL. The encryption here is a symmetric
     algorithm (Rijndael or TripleDES) with SALT, byte rotation, and byte offset. The password and SALT strings are
     assembled at runtime from different place in the code and mangled.
 
  3. The C# application is digitally signed with an asymmetric RSA certificate, and the public key is used to encrypt
     the REST body request sent to the token service. When the C# application needs a new access token, it decrypts
     the token URL, builds a json request object which contains the serial#, a timestamp and eventually few other
     parameters. This object is encrypted with the RSA public key, and send to the server via a POST url which
     contains the serial#.
 
  4. The server decrypts the json request using the RSA certificate private key, verifies the parameters, checks the 
     request is sent in an acceptable time compared the the timestamp, finds the Forge keys from the serial# and does
     the oAuth request with the Forge server.
 
  5. While the request is running, the server can track the requester ip address and match the ip address to a black
     list catalog service.
 
  6. If anything goes wrong, a random generated token is sent back to the application, and the application will start 
     a thread to randomly stop working.
 
  7. If everything is ok, an encrypted token will be send back to the application. The C# application will start using 
     this token in future Forge API request.
 
That is to secure the client_secret and access_token.
 
-	Now, the C# application is obfuscated
-	Digitally signed
-	I also added code to prevent debugging (including remote debugging)
-	I am using a tool to prevent reflection on the assembly, so you cannot disassemble it either

If you do not want to use the token service, you can still provide the access token manually in the Configuration dialog,
and or (at risk) configure the application with your client_id and client_secret in the Configuration dialog. The application
will encrypt the data to protect the keys as best it can. The obvious question here is why storing the data in 
%LOCALAPPDATA%\Autodesk\Autodesk.Forge.WpfCsharp...\1.0.0.0\user.config and not hardcode them in the code. This is a 
choice made to not have the key in the distributable executable so in case someone downloads the application from your
website, he cannot hack the keys directly, but you free to choose a better approach.


## Description

This sample exercises the .Net framework as a WPF application to demonstrate the Forge OAuth application
authorisation process and the Model Derivative API mentioned in the Quick Start guide.

In order to make use of this sample, you need to register your consumer key, of course:
* https://developer.autodesk.com > My Apps

This provides the credentials to supply while calling the Forge WEB service API endpoints.


## Dependencies

Visual Studio 2015, .Net Framework 4.5+

Node.js v6.9.2


## Setup/Usage Instructions

Setup instructions are a lot more complex because it requires a certicate and you got a WEB server to initilize as well.

  1. You first need to get hold of a certificate. A development certificate is fine because in this example we do not need
     to get verified by an official authority. The certificate is only to be used by us, but if you have already a 
     certificate, then you can use it here. You can generate a test certificate by:
     
     a- Using openssl
        ```
        openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365
        openssl pkcs12 -export -inkey key.pem -in cert.pem -out cert_key.p12
        cp cert_key.p12 cert_key.pfx
        ```
        
     b- Using Visual Studio
        https://msdn.microsoft.com/en-us/library/ff699202.aspx


  2. Download (fork, or clone) this project.
  3. Request your consumer key/secret keys from [https://developer.autodesk.com](https://developer.autodesk.com).
  4. Open a Visual Studio 2015 Command console window and set 2 environment variables 'CERTIFICATE' and 'PASSWORD'
     to point to your pfx certicate (this is used to sign your application). Then open the Visual Studio IDE,
     typing ``` devenv ```
  5. Load the project 'forge.wpf.csharp' in Visual Studio 2015
  6. Open App.xam.cs

     a- Go line #86, and change the password 
     b- Go line #99, and change the SALT string
     
  7. Build the solution, and run
     In the Debug Output Window, you should see something like:
     ```
       _Key -> 155, 125, 90, 223, 218, 110, 174, 248, 67, 202, 25, 165, 91, 89, 6, 178, 169, 5, 110, 98, 117, 107, 127, 39
       _IV -> 235, 41, 5, 91, 175, 209, 35, 28
       _Key -> m31a39purvhDyhmlW1kGsqkFbmJ1a38n
       _IV -> 6ykFW6/RIxw=
     fe45A4!@8 -> 62, 61, 12, 13, 25, 12, 249, 24, 16
     ->fe45A4!@8
      internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ 62, 12, 25, 249, 16 }
      internal static readonly byte[] _CRYPT_DEFAULT_PASSWORD1 ={ 61, 13, 12, 24 }
       x -> 62, 61, 12, 13, 25, 12, 249, 24, 16
     Autodesk Forge Rocks! -> 8, 90, 82, 74, 86, 57, 7, 76, 78, 89, 86, 45, 7, 82, 90, 76, 75, 86, 91, 92, 40
     ->Autodesk Forge Rocks!
      internal static readonly byte[] _SaltByteArray1 ={ 8, 82, 86, 7, 78, 86, 7, 90, 75, 91, 40 }
      internal static readonly byte[] _SaltByteArray2 ={ 90, 74, 57, 76, 89, 45, 82, 76, 86, 92 }
       x -> 8, 90, 82, 74, 86, 57, 7, 76, 78, 89, 86, 45, 7, 82, 90, 76, 75, 86, 91, 92, 40
     test encrypted: QnDCoP8NsQDXk6DENe9Gn33Cs/3D/H7C
     test decrypted: This is a test line
     ```
     
  8. Stop the application, and copy the 'internal' output to their respective placeholder at:
  
     a. forge.wpf.csharp/App.xaml.cs #123
     b. forge.wpf.csharp/App.xaml.cs #125
     c. forge.wpf.csharp/MainWindow.xaml.cs #1010
     d. forge.wpf.csharp/MainWindow.xaml.cs #1012
     e. forge.tokenGenerator.asp/GenerateToken/Utils/App.cs #34
     f. forge.tokenGenerator.asp/GenerateToken/Utils/App.cs #36
     g. forge.tokenGenerator.asp/GenerateToken/MainWindow.xaml.cs #35
     h. forge.tokenGenerator.asp/GenerateToken/MainWindow.xaml.cs #37
     i. forge.tokenGenerator.nodejs/server/encryption.js #29
     j. forge.tokenGenerator.nodejs/server/encryption.js #30
   
  9. Build the solution, and run (ignore the dialog error for now)
  10. Using Node.js

     a. Open an Node.js command prompt window, and go into the 'forge.tokenGenerator.nodejs' directory
     b. Set 2 environment variables FORGE_CLIENT_ID / FORGE_CLIENT_SECRET with your Forge keys
     c. Set an environment variable PRIVATE_CERTIFICATE to point to the location of your private cer certificate
     d. Start the server by typing: ``` node start.js ```
     e. Go in the 'forge.wpf.csharp' running application and press the Configure button:
        and enter '123456789' as serial number, and 'http://localhost:50010/generateToken' as token URL, 
        choose the 'Save in App settings' option,
        and press OK
     f. Just Log in, you ready to go!

  11. Using ASP.Net 

     a. Open an Visual Studio 2015 command prompt window
     b. Set 2 environment variables FORGE_CLIENT_ID / FORGE_CLIENT_SECRET with your Forge keys
     c. Set an environment variable PRIVATE_CERTIFICATE to point to the location of your private pfx/p12 certificate
     d. Set an environment variable CERTIFICATE_PASSWORD with your certificate password
     e. Start Visual Studio 2015 IDE from the command line by typing 'devenv'
     f. Load the project 'forge.tokenGenerator.asp\GenerateToken' in Visual Studio 2015, build the solution, and run
     g. Go in the 'forge.wpf.csharp' running application and press the Configure button:
        and enter '123456789' as serial number, and 'http://localhost:50010/generateToken' as token URL, 
        choose the 'Save in App settings' option,
        and press OK
     h. Just Log in, you ready to go!


     
A typical workflow is:

    # Do authentication.
    Login to the service via the login button - the application auto-log on start

    # Create a bucket. Bucket name must be lower case and valid characters.
    The application will create automatically a default bucket for you

    # Upload a model.
    Drag'n Drop a file on the main area

    # Register the model to get it translated.
    Right-Click on the file item, and select 'Translate'

    # Wait until the translation completes.
    # Translation is complete when it reaches 'success - 100%'

    # View the result.
    Right-Click on the file item, and select either 'View in Browser' or 'View'


## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). 
Please see the [LICENSE](LICENSE) file for full details.


## Written by

Cyrille Fauvel <br />
Forge Partner Development <br />
http://developer.autodesk.com/ <br />
http://around-the-corner.typepad.com <br />

Augusto Goncalves for the ASP.Net token service<br />
Forge Partner Development <br />
http://developer.autodesk.com/ <br />
