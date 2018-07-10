# DalSoft.RestClient

[![Help and chat on Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/DalSoft-RestClient)

DalSoft.RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RESTFul API's, making it trivial to create REST requests using a lot less code . 

Originally created to remove the boilerplate code involved in making REST requests using code that is testable. I know there are a couple of  REST clients out there but I wanted the syntax to look a particular way with minimal fuss.

> DalSoft.RestClient is biased towards RESTFul API's returning JSON - if you don't provide Accept and Content-Type headers then they are set to `application/json` by default. [See Working with non JSON content](https://github.com/DalSoft/DalSoft.RestClient/wiki/Working-with-non-JSON-content)

## Supported Platforms

DalSoft.RestClient targets .NET Standard 2.0 therefore **supports Windows, Linux, Mac and Xamarin (iOS, Android and UWP)**.

If you need to target .NET Standard 1.4 use version 3.2.2.

## Getting Started

Install via NuGet

```dos
> dotnet add package DalSoft.RestClient
```

You start by new'ing up the RestClient and passing in the base uri for your RESTful API. Then simply chain members that would make up the resource you want to access - ending with the HTTP method you want to use. The example below will perform a GET on https://api.github.com/users/dalsoft/repos getting a list of my repositories: 

```cs
dynamic client = new RestClient("https://api.github.com");

var repositories = await client
   .Headers(new { UserAgent = "MyClient" }) //GitHub requires a User-Agent to be set
   .users.dalsoft.repos.Get();
   
foreach (var repo in repositories)
   Console.WriteLine(repo.name);
```
> Note all HTTP methods are async

## New in Version 3.3.0 IHttpClientFactory Goodness!

In version 3.3.0 we added support for .NET Core's 2.1 [IHttpClientFactory](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore). 

First register your RestClient as a service in Startup.cs.
```cs
public class Startup
{
   public void ConfigureServices(IServiceCollection services)
   {
      services.AddRestClient("https://api.github.com", new Headers(new { UserAgent = "MyClient" }));
   }
}
```

Then inject `IRestClientFactory` into your controller, which is how we support IHttpClientFactory.
```cs
public class GitHubController : Controller
{
   private readonly IRestClientFactory _restClientFactory;
        
   public GitHubController(IRestClientFactory restClientFactory)
   {
      _restClientFactory = restClientFactory;
   }

   [Route("github/users/dalsoft"), HttpGet]
   public async Task<List<Repository>> CreateClient()
   {
      dynamic restClient = _restClientFactory.CreateClient();
            
      var repositories = await restClient.users.dalsoft.repos.Get();
            
      return repositories;
   }        
}
```

See [IHttpClientFactory](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance#ihttpclientfactory) for more examples and details on our IHttpClientFactory support.

## New in Version 3.0 Pipeline Awesomeness!

See [Configuration, Handlers and Pipeline](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Handlers-and-Pipeline)

## Breaking Change Since Version 3.1
Since version 3.1 the DefaultRequestHeaders Dictionary is readonly the only way to add DefaultHeaders is passing a Dictionary to the [constructor](https://github.com/DalSoft/DalSoft.RestClient/wiki/Headers). You can add/override the headers on a per request basis using the [Headers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Headers#per-request-headers) method.

~~client.DefaultRequestHeaders.Add("Accept", "application/json");~~ //Not supported since version 3.1

## Everything You Need To Know

* [Getting Started](https://github.com/DalSoft/DalSoft.RestClient/wiki/Getting-Started)

* [Supported Platforms](https://github.com/DalSoft/DalSoft.RestClient/wiki/Supported-Platforms)

* [How to Access Resources](https://github.com/DalSoft/DalSoft.RestClient/wiki/How-to-Access-Resources)

* [Get, Delete, Head](https://github.com/DalSoft/DalSoft.RestClient/wiki/Get,-Delete,-Head)

* [Query Strings](https://github.com/DalSoft/DalSoft.RestClient/wiki/Query-Strings)

* [Put, Post, Patch](https://github.com/DalSoft/DalSoft.RestClient/wiki/Put,-Post,-Patch)

* [Headers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Headers)
  * [Default Headers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Headers#default-headers)
    * [Breaking Change Since Version 3.1](https://github.com/DalSoft/DalSoft.RestClient/wiki/Headers#breaking-change-since-version-31)
  * [Per Request Headers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Headers#per-request-headers)

* [Dynamic Binding](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding)
  * [Requests](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#requests)
  * [Responses](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#responses)
  * [HttpResponseMessage](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#httpresponsemessage)
  * [Casting](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#casting)
    * [Implicit casting](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#implicit-casting)
    * [Collections](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#collections)
    * [Mapping your models using Json.NET](https://github.com/DalSoft/DalSoft.RestClient/wiki/Dynamic-Binding#mapping-your-models-using-jsonnet)

**Advanced**
* [Working with non JSON content](https://github.com/DalSoft/DalSoft.RestClient/wiki/Working-with-non-JSON-content)
  * [Posting forms](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins#formurlencodedhandler)
  * [Posting files](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins#multipartformdatahandler)

* [Guidance](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance)
  * [Disposing](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance#disposing)
  * [IHttpClientFactory](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance#ihttpclientfactory)
  * [Retrying Transient Errors](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance/#retrying-transient-errors)  
  * [Synchronous Usage](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance#synchronous-usage)

* [Configuration, Handlers and Pipeline](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Handlers-and-Pipeline)
  * [General Configuration](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#general-configuration)
  * [HttpClient Configuration using HttpClientHandler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#httpclient-configuration-using-httpclienthandler)
    * [WebRequestHandler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#webrequesthandler)
  * [DalSoft.RestClient Pipeline](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#dalsoftrestclient-pipeline)
  * [Configuring the Pipeline](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#configuring-the-pipeline)
    * [Using DelegatingHandlers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#using-delegatinghandlers)
    * [Using Func<>'s](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#using-funcs)
    * [Mixing Func<>'s and DelegatingHandlers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#mixing-funcs-and-delegatinghandlers)
    * [Fluently Using PipelineExtensions](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#fluently-using-pipelineextensions)
  * [Getting the Request Content as an Object in a Handler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline#getting-the-request-content-as-an-object-in-a-handler)

* [Available Handlers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins)
  * [UnitTestHandler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins#unittesthandler)
  * [FormUrlEncodedHandler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins#formurlencodedhandler)
  * [MultipartFormDataHandler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins#multipartformdatahandler)
  * [RetryHandler](https://github.com/DalSoft/DalSoft.RestClient/wiki/Available-Plugins#retryhandler)

* [Unit Testing](https://github.com/DalSoft/DalSoft.RestClient/wiki/Unit-Testing)

## Standing on the Shoulders of Giants

DalSoft.RestClient is built using the following great open source projects:
* [Json.NET](http://www.newtonsoft.com/json)
* [System.Net.Http](https://github.com/dotnet/corefx/tree/master/src/System.Net.Http)

DalSoft.RestClient is inspired by and gives credit to:
* [Simple.Data](http://simplefx.org/simpledata/docs/index.html)
* [This Stack Overflow question](http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject)

