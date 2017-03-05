# DalSoft.RestClient

Inspired by Simple.Data and AngularJS's $http service, DalSoft.RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RESTFul API's. 

Originally created to remove the boilerplate code involved in creating integration tests and SDK's for RESTFul API's. I know there are a couple of dynamic rest clients out there but I wanted the syntax to look a particular way, and I wanted it to be particularly useful for testing.

> DalSoft.RestClient is biased towards RESTFul API's returning JSON - if you don't provide Accept and Content-Type headers then they are set to `application/json`. [See Working with non JSON content](https://github.com/DalSoft/DalSoft.RestClient/wiki/Working-with-non-JSON-content)

## Supported Platforms

DalSoft.RestClient targets .NET Standard 1.4 therefore supports Windows, Linux, Mac and Xamarin (iOS, Android and UWP).  

## Getting Started

Install via NuGet

```dos
PM> Install-Package DalSoft.RestClient
```

You start by new'ing up the RestClient and passing in the base uri for your RESTful API. Then simply chain members that would make up the resource you want to access - ending with the HTTP method you want to use. The example below will perform a GET on http://jsonplaceholder.typicode.com/users/: 

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Users.Get();
```
> Note all HTTP methods are async

## New in Version 3.0 Pipeline Awesomeness!

See [Configuration, Plugins and Pipeline](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline)

## Everything You Need To Know

* [Getting Started](https://github.com/DalSoft/DalSoft.RestClient/wiki/Getting-Started)

* [Supported Platforms](https://github.com/DalSoft/DalSoft.RestClient/wiki/Supported-Platforms)

* [How to Access Resources](https://github.com/DalSoft/DalSoft.RestClient/wiki/How-to-Access-Resources)

* [The Dynamic Return Type and HttpResponseMessage](https://github.com/DalSoft/DalSoft.RestClient/wiki/The-Dynamic-Return-Type-and-HttpResponseMessage)

* [Get, Delete, Head](https://github.com/DalSoft/DalSoft.RestClient/wiki/Get,-Delete,-Head)

* [Query Strings](https://github.com/DalSoft/DalSoft.RestClient/wiki/Query-Strings)

* [Put, Post, Patch](https://github.com/DalSoft/DalSoft.RestClient/wiki/Put,-Post,-Patch)

* [Default Headers](https://github.com/DalSoft/DalSoft.RestClient/wiki/Default-Headers)

* [Casting](https://github.com/DalSoft/DalSoft.RestClient/wiki/Casting)

* [Synchronous Usage](https://github.com/DalSoft/DalSoft.RestClient/wiki/Synchronous-Usage)

* [Working with non JSON content](https://github.com/DalSoft/DalSoft.RestClient/wiki/Working-with-non-JSON-content)

* [Disposing](https://github.com/DalSoft/DalSoft.RestClient/wiki/Disposing)

* [Configuration, Plugins and Pipeline](https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline)
  * [Available plugins] (https://github.com/DalSoft/DalSoft.RestClient/wiki/Configuration,-Plugins-and-Pipeline)

* [Unit Testing](https://github.com/DalSoft/DalSoft.RestClient/wiki/Unit-Testing)

* [Guidance](https://github.com/DalSoft/DalSoft.RestClient/wiki/Guidance)

## Standing on the Shoulders of Giants

DalSoft.RestClient is built using the following great open source projects:
* [Json.NET](http://www.newtonsoft.com/json)
* [System.Net.Http](https://github.com/dotnet/corefx/tree/master/src/System.Net.Http)

DalSoft.RestClient is inspired by and gives credit to:
* [Simple.Data](http://simplefx.org/simpledata/docs/index.html)
* [This Stack Overflow question](http://stackoverflow.com/questions/12634250/possible-to-get-chained-value-of-dynamicobject)
* [AngularJS (version 1.x) $http service](https://docs.angularjs.org/api/ng/service/$http)



