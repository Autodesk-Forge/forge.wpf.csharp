
[![build status](https://api.travis-ci.org/cyrillef/models.autodesk.io.png)](https://travis-ci.org/cyrillef/models.autodesk.io)
[![.Net](https://img.shields.io/badge/.Net-4.5-blue.svg)](https://msdn.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/Nuget.Core.svg)](https://www.nuget.org/)
![Platforms](https://img.shields.io/badge/platform-windows%20%7C%20osx%20%7C%20linux-lightgray.svg)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)
[![oAuth2](https://img.shields.io/badge/oAuth2-v1-green.svg)](http://developer-autodesk.github.io/)
[![Data-Management](https://img.shields.io/badge/Data%20Management-v1-green.svg)](http://developer-autodesk.github.io/)
[![OSS](https://img.shields.io/badge/OSS-v2-green.svg)](http://developer-autodesk.github.io/)
[![Model-Derivative](https://img.shields.io/badge/Model%20Derivative-v2-green.svg)](http://developer-autodesk.github.io/)
[![Viewer](https://img.shields.io/badge/Forge%20Viewer-v2.17-green.svg)](http://developer-autodesk.github.io/)

# forge.wpf-csharp


<b>Note:</b> For using this sample, you need a valid oAuth credential.
Visit this [page](https://developer.autodesk.com) for instructions to get on-board.


Demonstrates the Autodesk Forge API authorisation and translation process using a C#/.Net WPF application.

* using 2 legged oAuth
* uses asynchronous methods

### Thumbnail
![thumbnail](Images/thumbnail_default.png)

## Security considerations

This version does not implement any security measures to protect your Forge keys, this is why the description 
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
%LOCALAPPDATA%\Autodesk\Autodesk.Forge.WpfCsharp...\1.0.0.0\user.config. All data is saved in clear text. This
is why on this version the Forge client secret key is never saved, otherwise it would compromise your keys as
well. This is for this reason, the setup instructions uses environment variables, but it is very clear that you
should not use this approach when delivering the application to your customer(s).

If you are interrested to learn few technics to help on protecting your application and your Forge keys, checkout 
the 'secure-dev' branch. This 'secure-dev' branch is the one you need to consider to deliver your application to a 
customer. The 'master' branch is present and 'unsecure' to keep the code simple and ease of read to learn the Forge
API.


## Description

This sample exercises the .Net framework as a WPF application to demonstrate the Forge OAuth application
authorisation process and the Model Derivative API mentioned in the Quick Start guide.

In order to make use of this sample, you need to register your consumer key, of course:
* https://developer.autodesk.com > My Apps

This provides the credentials to supply while calling the Forge WEB service API endpoints.


## Dependencies

Visual Studio 2015, .Net Framework 4.5+


## Setup/Usage Instructions

  1. Download (fork, or clone) this project.
  2. Request your consumer key/secret key from [https://developer.autodesk.com](https://developer.autodesk.com).
  3. Open an Visual Studio 2015 command prompt window
  4. Set 2 environment variables FORGE_CLIENT_ID / FORGE_CLIENT_SECRET  with your Forge keys, or skip this 
     step and use the 'Configure' button from the menu after starting the application.
  5. Start Visual Studio 2015 IDE from the command line by typing 'devenv'
  6. Load the project in Visual Studio 2015, build the solution, and run
  

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
