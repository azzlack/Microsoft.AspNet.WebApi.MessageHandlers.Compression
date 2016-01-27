Microsoft.AspNet.WebApi.MessageHandlers.Compression
===================================================
[![TeamCity Build Status](https://img.shields.io/teamcity/https/teamcity.knowit.no/e/External_MicrosoftAspNetWebApiExtensionsCompression_General_Release.svg?style=flat-square)](https://teamcity.knowit.no/viewType.html?buildTypeId=External_MicrosoftAspNetWebApiExtensionsCompression_General_Release)  

Drop-in module for ASP.Net WebAPI that enables `GZip` and `Deflate` support.  
This module is based on [this blog post by Ben Foster](http://benfoster.io/blog/aspnet-web-api-compression) which in turn is based on this blog post by [Kiran Challa](http://blogs.msdn.com/b/kiranchalla/archive/2012/09/04/handling-compression-accept-encoding-sample.aspx).  
This code improves on their work by adding several new options, as well as fixing some issues with the original code.

![NuGet Package Version](http://img.shields.io/nuget/v/Microsoft.AspNet.WebApi.MessageHandlers.Compression.svg?style=flat-square)&nbsp;&nbsp;![NuGet Package Downloads](http://img.shields.io/nuget/dt/Microsoft.AspNet.WebApi.MessageHandlers.Compression.svg?style=flat-square)

## How to use
### Server side
You need to add the compression handler as the last applied message handler on outgoing requests, and the first one on incoming requests.  
To do that, just add the following line to your `App_Start\WebApiConfig.cs` file after adding all your other message handlers:  
```csharp
GlobalConfiguration.Configuration.MessageHandlers.Insert(0, new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
```
This will insert the `ServerCompressionHandler` to the request pipeline as the first on incoming requests, and the last on outgoing requests.
  
### Client side
  
#### JavaScript
If you are doing your requests with `JavaScript` you probably don't have to do do anything.  
Just make sure the `gzip` and `deflate` values are included in the `Accept-Encoding` header. (Most browsers do this by default)  
  
#### C\# 
You need to apply the following code when creating your `HttpClient`.  
```csharp
var client = new HttpClient(new ClientCompressionHandler(new GZipCompressor(), new DeflateCompressor()));

client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
```
  
Thats it! You should now immediately start experiencing smaller payloads when doing GET, POST, PUT, etc.

## Advanced use
#### Skip compression of requests/responses that are smaller than a specified value
By default, both `ServerCompressionHandler` and `ClientCompressionHandler` compress everything larger tham 860 bytes.  
However, this can be overriden by inserting a threshold as the first parameter like this:
```csharp
var serverCompressionHandler = new ServerCompressionHandler(4096, new GZipCompressor(), new DeflateCompressor());
var clientCompressionHandler = new ClientCompressionHandler(4096, new GZipCompressor(), new DeflateCompressor());
```
The above code will skip compression for any request/response that is smaller than `4096 bytes` / `4 kB`.

## Version history
#### [1.3.0](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.3.0)  (current)
* Added attribute for disable compression for certain routes
* Fixed clearing of non-standard properties when compressing and decompressing

#### [1.2.2](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.2.2)
* Stop trying to compress requests/responses with no content

#### [1.2.1](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.2.1)
* Properly copy HTTP headers from the content that is going to be compressed

#### [1.2.0](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.2.0)
* Fixed 504 timeout error when returning `ByteArrayContent` from an `ApiController`
* Fixed bug with content stream sometimes being disposed before returning
* Added better test coverage

#### [1.1.2](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.1.2)
* Changed default threshold for compression to 860 bytes (what Akamai uses)
* Now reports proper Content-Length

#### [1.1.0](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.1.0)
* Simplified usage
* Added support for setting a minimum content size for compressing

#### [1.0.3](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.MessageHandlers.Compression/1.0.3)
* First release, basic compression of server responses and client requests
* Did not support compressing POSTs and PUTs
