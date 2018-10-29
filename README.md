# RestClient

[![Help and chat on Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/DalSoft-RestClient)

> ## **For everything you need to know, please head over to [https://restclient.dalsoft.io](https://restclient.dalsoft.io)**

RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RESTFul API's, making it trivial to create REST requests using a lot less code . 

Originally created to remove the boilerplate code involved in making REST requests using code that is testable. I know there are a couple of  REST clients out there but I wanted the syntax to look a particular way with minimal fuss.

RestClient is biased towards posting and returning JSON - if you don't provide Accept and Content-Type headers then they are set to application/json by default [See Working with non JSON content](https://restclient.dalsoft.io/docs/content-other-than-json/).

## Supported Platforms

RestClient targets .NET Standard 2.0 therefore **supports Windows, Linux, Mac and Xamarin (iOS, Android and UWP)**.

If you need to target .NET Standard 1.4 use version 3.2.2.

## Getting Started

## Install via .NET CLI

```bash
> dotnet add package DalSoft.RestClient
```

## Install via NuGet

```bash
PM> Install-Package DalSoft.RestClient
```

## Example call a REST API in two lines of code

You start by new'ing up the RestClient and passing in the base uri for your RESTful API. 

Then simply chain members that would make up the resource you want to access - ending with the HTTP method you want to use. 

For example if your wanted to perform a GET on [https://jsonplaceholder.typicode.com/users/1](https://jsonplaceholder.typicode.com/users/1) you would do the following:

```cs
dynamic client = new RestClient("https://jsonplaceholder.typicode.com");

var user = await client.Users(1).Get();
   
Console.WriteLine(user.name);
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

> **See [IHttpClientFactory](/docs/ihttpclientfactory/) for more examples and details on our IHttpClientFactory support.**

## New in Version 3.0 Pipeline Awesomeness!

```cs
var config = new Config() //Build a pipeline using extension methods
                    .UseHttpClientHandler(new HttpClientHandler())
                    .UseHandler((request, token, next) => next(request, token))
                    .UseHandler(new UnitTestHandler())
                    .UseUnitTestHandler(request => { })
                    .UseUnitTestHandler(request => new HttpResponseMessage());

dynamic restClient = new RestClient("http://headers.jsontest.com/", config);
```

See [Pipeline Docs](https://restclient.dalsoft.io/docs/about-the-handler-pipeline/)

## Standing on the Shoulders of Giants

DalSoft.RestClient is built using the following great open source projects:
* [Json.NET](http://www.newtonsoft.com/json)
* [System.Net.Http](https://github.com/dotnet/corefx/tree/master/src/System.Net.Http)

DalSoft.RestClient is inspired by and gives credit to:
* [Simple.Data](http://simplefx.org/simpledata/docs/index.html)
* [This Stack Overflow question](http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject)

