Microsoft.AspNet.WebApi.MessageHandlers.Compression
===================================================

Drop-in module for ASP.Net WebAPI that enables `GZip` and `Deflate` support


### How to use
**Server side**  
You need to add the compression handler as the last applied message handler on outgoing requests, and the first one on incoming requests.  
To do that, just add the following line to your `App_Start\WebApiConfig.cs` file after adding all your other message handlers:  
```csharp
GlobalConfiguration.Configuration.MessageHandlers.Insert(0, new CompressionHandler(new GZipCompressor(), new DeflateCompressor()));
```
This will insert the `CompressionHandler` to the request pipeline as the first on incoming requests, and the last on outgoing requests.
  
**Client side**  
  
**`JavaScript`**  
If you are doing your requests with `JavaScript` you probably don't have to do do anything.  
Just make sure the `gzip` and `deflate` values are included in the `Accept-Encoding` header. (Most browsers do this by default)  
  
**`C#`**  
You need to apply the following code when creating your `HttpClient`, depending on the request type.  

**GET requests**
```csharp
var client =
    new HttpClient(
        new DecompressionHandler(new HttpClientHandler(), new GZipCompressor(), new DeflateCompressor()));

client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
```
  
**POST, PUT, DELETE requests (anything with a request body)**
```csharp
var client =
    new HttpClient(
        new CompressionHandler(new HttpClientHandler(), new GZipCompressor(), new DeflateCompressor()));

client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
```
  
Thats it! You should now immediately start experiencing smaller payloads when doing GET, POST, PUT, etc.
