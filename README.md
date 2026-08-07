# DalSoft .NET REST Client for all platforms

### `If you find this repo / package useful all I ask is you please star it ⭐`
> ### Do you or the company you work for benefit from the tools I build? <br /> If so please consider [Becoming a Sponsor](https://github.com/sponsors/dalsoft) it would be greatly appreciated ❤️ 

![Nuget](https://img.shields.io/nuget/v/DalSoft.RestClient)
[![StackOverflow](https://img.shields.io/badge/questions-on%20StackOverflow-orange.svg?style=flat)](http://stackoverflow.com/questions/tagged/dalsoft.restclient)
[![Docs](https://img.shields.io/badge/Docs-Website-yellow)](https://restclient.dalsoft.io/)

> ## **For everything you need to know, please head over to [https://restclient.dalsoft.io](https://restclient.dalsoft.io)**
> ## **👉 [New Static Typing and Resource Expressions in 4.0](http://www.dalsoft.co.uk/blog/index.php/2019/08/04/csharp-rest-client-now-with-static-typing)**

![alt text](https://www.dalsoft.co.uk/blog/wp-content/uploads/2019/08/intellisense.gif)

## Just some of the things you can do with DalSoft.RestClient

* [Easliy Create Fluent SDK's](https://www.dalsoft.co.uk/blog/index.php/2019/08/04/csharp-rest-client-now-with-static-typing/#Extending_Using_Resource_Classes)
* [Unit Testing](https://restclient.dalsoft.io/docs/unit-testing/)
* [Post Json](https://restclient.dalsoft.io/docs/put-post-patch/)
* [Post Forms](https://restclient.dalsoft.io/docs/formurlencodedhandler/)
* [Post Files](https://restclient.dalsoft.io/docs/multipartformdatahandler/)
* [Retry Requests](https://restclient.dalsoft.io/docs/retrying-transient-errors/)
* [Twitter SDK](https://restclient.dalsoft.io/docs/twitterandler/)
* [Raw HTTP](https://restclient.dalsoft.io/docs/content-other-than-json/)
* [Passthrough HttpClient](https://www.dalsoft.co.uk/blog/index.php/2019/08/04/csharp-rest-client-now-with-static-typing/#HttpClient)
* [Authorization](https://www.dalsoft.co.uk/blog/index.php/2019/08/04/csharp-rest-client-now-with-static-typing/#Authorization_method)


## Supported Platforms

RestClient targets .NET Standard 2.0 therefore **supports Windows, Linux, Mac and Xamarin (iOS, Android and UWP)**.

.NET 5.0 and .NET 6.0 supported
All versions of .NET Core supported 
All versions of legacy .NET Framework > 4.6.1 supported

## Getting Started

## Install via .NET CLI

```bash
> dotnet add package DalSoft.RestClient
```

## Install via NuGet

```bash
PM> Install-Package DalSoft.RestClient
```

## Example calling a REST API 

You start by new'ing up the RestClient and passing in the base uri for your RESTful API. 

For example if your wanted to perform a GET on [https://jsonplaceholder.typicode.com/users/1](https://jsonplaceholder.typicode.com/users/1) you would do the following:

**Static Typed Rest Client**

For the Static typed Rest Client just pass a string representing the resource you want access to the Resource method, and then call the HTTP method you want to use. 
```cs
var client = new RestClient("https://jsonplaceholder.typicode.com");

User user = await client.Resource("users/1").Get();
   
Console.WriteLine(user.Name);
```

**Dynamicaly Typed Rest Client**

For the Dynamicaly typed Rest Client chain members that would make up the resource you want to access - ending with the HTTP method you want to use. 
```cs
dynamic client = new RestClient("https://jsonplaceholder.typicode.com");

var user = await client.Users(1).Get();
   
Console.WriteLine(user.name);
```
> Note all HTTP methods are async
 
## Recent Releases 
 
* Version 5.0 System.Text.Json by default - see breaking changes below
* [Version 4.0 Static Typing and Resource Expressions](http://www.dalsoft.co.uk/blog/index.php/2019/08/04/csharp-rest-client-now-with-static-typing)
* [Version 3.3.0 IHttpClientFactory Goodness](https://restclient.dalsoft.io/docs/ihttpclientfactory/)
* [Version 3.0 Pipeline Awesomeness](https://restclient.dalsoft.io/docs/about-the-handler-pipeline/)

## About
RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RESTFul API's, making it trivial to create REST requests using a lot less code. 

Originally created to remove the boilerplate code involved in making REST requests using code that is testable. I know there are a couple of  REST clients out there but I wanted the syntax to look a particular way with minimal fuss.

RestClient is biased towards posting and returning JSON - if you don't provide Accept and Content-Type headers then they are set to application/json by default [See Working with non JSON content](https://restclient.dalsoft.io/docs/content-other-than-json/).


## Version 5.0 Breaking Changes

From version 5.0 RestClient uses [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) to serialize requests and deserialize responses - with stock System.Text.Json behaviour, so `[JsonPropertyName]` attributes are honoured and property matching is case sensitive. The only leniency added on top of the stock defaults is that trailing commas and comments are accepted when reading, because real world systems are less than perfect.

To customise serialization pass your `JsonSerializerOptions`:

```cs
var config = new Config().SetJsonSerializerOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web)); 
```

If you need the legacy Json.NET behaviour (`[JsonProperty]` attributes, `JsonSerializerSettings` etc.) opt in to the fallback - this restores the 4.x behaviour exactly:

```cs
var config = new Config().UseNewtonsoftJson(); // optionally takes your JsonSerializerSettings
```

`SetJsonSerializerSettings` and `Config.JsonSerializerSettings` have been removed - use `UseNewtonsoftJson(jsonSerializerSettings)` instead. Note that failed typed casts now throw `System.Text.Json.JsonException` unless you opt in to Json.NET.

## Performance

**Near-native performance** - typed requests allocate the same as using HttpClient and System.Text.Json by hand, and several times leaner than RestSharp.

Version 5.0 had a performance pass guided by [BenchmarkDotNet](https://benchmarkdotnet.org/) - requests and responses are serialized straight to and from utf-8 bytes skipping intermediate strings, the JSON DOM is only built if you use dynamic access (typed casts skip it entirely), and arrays are wrapped lazily.

Before and after the performance pass, 10,000 item JSON payload, in process (no network), .NET 8:

| Scenario                                | 4.x            | 5.0            |
|---------------------------------------- |---------------:|---------------:|
| Typed response `List<User>`             | 22.1 ms / 9.87 MB | 13.5 ms / 5.10 MB |
| Dynamic access `result[0].id`           | 8.86 ms / 6.59 MB | 5.28 ms / 5.46 MB |
| Typed response single object            | 5.98 µs / 4.59 KB | 3.66 µs / 2.95 KB |
| POST `List<User>` body                  | 5.83 ms / 3.40 MB | 3.69 ms / 1.13 MB |

Measured against using HttpClient and System.Text.Json by hand, POST request bodies and typed responses now allocate about the same (1.0x), down from 3.0x and 3.5x respectively.

### Real world comparison

A real GET request to the GitHub API (`https://api.github.com/repos/DalSoft/DalSoft.RestClient`) deserialized to a typed model, compared to native HttpClient and other popular REST clients, .NET 8:

| Client                          | Median   | Mean     | Allocated | Alloc Ratio |
|-------------------------------- |---------:|---------:|----------:|------------:|
| HttpClient + GetFromJsonAsync   | 51.01 ms | 49.64 ms |  11.38 KB |       1.00x |
| **DalSoft.RestClient 5.0**      | 41.38 ms | 40.76 ms |  23.24 KB |       2.04x |
| RestSharp 114.0                 | 45.45 ms | 45.57 ms | 135.74 KB |      11.93x |
| Flurl.Http 4.0.2                | 19.64 ms | 22.83 ms |  19.79 KB |       1.74x |
| Refit 15.0                      | 39.55 ms | 42.98 ms |  17.80 KB |       1.56x |
| RestClient.Net 7.2.1 *          | 51.07 ms | 91.76 ms |  15.27 KB |       1.34x |

Time over a real network is dominated by latency so every client lands within noise of native HttpClient - don't read the time column as a ranking, the differences (including where a client appears faster than native HttpClient) are statistical noise. The allocations column is what shows the overhead each library adds per request. DalSoft.RestClient stays close to native and the lightweight clients while giving you a full dynamic API, and allocates about 6x less than RestSharp.

\* RestClient.Net 7 doesn't deserialize for you - you supply your own System.Text.Json delegate, so its numbers exclude the library-side JSON handling every other row includes.

To run the benchmarks yourself: `dotnet run -c Release` in the `DalSoft.RestClient.Benchmarks` project. The real world benchmark makes around 50 requests - GitHub allows 60 unauthenticated requests an hour, set the `GITHUB_TOKEN` environment variable to raise the limit.

## Standing on the Shoulders of Giants

DalSoft.RestClient is built using the following great open source projects:
* [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
* [Json.NET](http://www.newtonsoft.com/json)
* [System.Net.Http](https://github.com/dotnet/corefx/tree/master/src/System.Net.Http)

DalSoft.RestClient is inspired by and gives credit to:
* [Simple.Data](http://simplefx.org/simpledata/docs/index.html)
* [This Stack Overflow question](http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject)

