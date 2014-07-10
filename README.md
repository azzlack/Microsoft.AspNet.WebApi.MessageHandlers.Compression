Microsoft.AspNet.WebApi.MessageHandlers.Compression
===================================================

Drop-in module for ASP.Net WebAPI that enables `GZip` and `Deflate` support


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
var client = new HttpClient(new ClientompressionHandler(new HttpClientHandler(), new GZipCompressor(), new DeflateCompressor()));

client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
```
  
Thats it! You should now immediately start experiencing smaller payloads when doing GET, POST, PUT, etc.

## Advanced use
### Don't compress requests/responses that are smaller than a specified value
By default, both `ServerCompressionHandler` and `ClientCompressionHandler` compress everything.  
However, this can be overriden by inserting a threshold as the first parameter like this:
```csharp
var serverCompressionHandler = new ServerCompressionHandler(4096, new HttpClientHandler(), new GZipCompressor(), new DeflateCompressor());
var clientCompressionHandler = new ClientCompressionHandler(4096, new HttpClientHandler(), new GZipCompressor(), new DeflateCompressor());
```
The above code will skip compression for any request/response that is smaller the `4096 bytes` / `4 kB`.

## Version history
### 1.1 (current)
* Simplified usage
* Added support for setting a minimum content size for compressing

### 1.0
* First release, basic compression of server responses and client requests
* Did not support compressing POSTs and PUTs