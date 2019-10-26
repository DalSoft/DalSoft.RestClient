# DalSoft C# RestClient 

![Nuget](https://img.shields.io/nuget/v/DalSoft.RestClient)
[![Help and chat on Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/DalSoft-RestClient)
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
 
* [Version 4.0 Static Typing and Resource Expressions](http://www.dalsoft.co.uk/blog/index.php/2019/08/04/csharp-rest-client-now-with-static-typing)
* [Version 3.3.0 IHttpClientFactory Goodness](https://restclient.dalsoft.io/docs/ihttpclientfactory/)
* [Version 3.0 Pipeline Awesomeness](https://restclient.dalsoft.io/docs/about-the-handler-pipeline/)

## About
RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RESTFul API's, making it trivial to create REST requests using a lot less code. 

Originally created to remove the boilerplate code involved in making REST requests using code that is testable. I know there are a couple of  REST clients out there but I wanted the syntax to look a particular way with minimal fuss.

RestClient is biased towards posting and returning JSON - if you don't provide Accept and Content-Type headers then they are set to application/json by default [See Working with non JSON content](https://restclient.dalsoft.io/docs/content-other-than-json/).


## Standing on the Shoulders of Giants

DalSoft.RestClient is built using the following great open source projects:
* [Json.NET](http://www.newtonsoft.com/json)
* [System.Net.Http](https://github.com/dotnet/corefx/tree/master/src/System.Net.Http)

DalSoft.RestClient is inspired by and gives credit to:
* [Simple.Data](http://simplefx.org/simpledata/docs/index.html)
* [This Stack Overflow question](http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject)

