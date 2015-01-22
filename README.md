# DalSoft.RestClient

Inspired by Simple.Data and angular's $http service, DalSoft.RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RestFul resources. 

Originally created to remove the boilerplate code involved in creating integration tests and SDK's for RestFul API's. I know there are a couple of dynamic rest clients out there but I wanted the syntax to look a particular way, and I wanted it to be particularly useful for testing.

## Getting Started 

You start by new'ing up the RestClient passing in the base uri for your RESTful API. Then simply chain members that would make up the resource you want to access, ending with the HTTP verb you want to use. The Example below will perform a HTTP GET to http://jsonplaceholder.typicode.com/posts/. 
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
await client.Posts.Get();
```
> Note all HTTP methods are async

## Resources

### Accessing resource by indentity 

Accessing a resource by indentity works as you would expect for example if your wanted to perform a GET against http://jsonplaceholder.typicode.com/posts/1 you would do the following:

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
await client.Posts(1).Get();
```

For GET, HEAD, DELETE you can also pass the resource indentity in the method for example:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
await client.Posts().Delete(1);
```
### Nested resources

Nested resources again work as your would expect for example if your wanted to perform a GET against http://jsonplaceholder.typicode.com/posts/2/comments you would do the following:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
await client.Posts(2).Comments.Get()
```

### Awakward resources

You will always come across an API that has an resource that isn't valid C# syntax. To escape invalid C# syntax in an resource use the Resource method for example.

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
await client.Posts(2).Resource("awakward-resource-with-dashes").Get()
```

## The dynamic return value
The dynamic return type is 

## Methods

## Implict casting

## Collections

## HttpResponseMessage

## Headers

## 

