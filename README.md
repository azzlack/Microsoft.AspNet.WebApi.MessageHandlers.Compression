Microsoft.AspNet.WebApi.MessageHandlers.Compression
===================================================

Drop-in module for ASP.Net WebAPI that enables `GZip` and `Deflate` support


### How to use
**Server side**:  
You need to add the compression handler as the last applied message handler on outgoing requests, and the first one on incoming requests.  
To do that, just add the following line to your `App_Start\WebApiConfig.cs` file after adding all your other message handlers:  
```csharp
config.MessageHandlers.Insert(0, new CompressionHandler(new GZipCompressor(), new DeflateCompressor()));
```
This will insert the `CompressionHandler` to the request pipeline as the first one on incoming requests, and the last one on outgoing requests.
  
**Client side**:  
You need to apply the following code when creating your `HttpClient`:  
```csharp
var client =
    new HttpClient(
        new DecompressionHandler(new HttpClientHandler(), new GZipCompressor(), new DeflateCompressor()));

client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
```