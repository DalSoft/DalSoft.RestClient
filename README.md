# DalSoft.RestClient

Inspired by Simple.Data and angular's $http service, DalSoft.RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RestFul resources. 

Originally created to remove the boilerplate code involved in creating integration tests and SDK's for RestFul API's. I know there are a couple of dynamic rest clients out there but I wanted the syntax to look a particular way, and I wanted it to be particularly useful for testing.

> This library is biased towards JSON content see Working with non JSON content

## Getting Started 

You start by new'ing up the RestClient and passing in the base uri for your RESTful API. Then simply chain members that would make up the resource you want to access - ending with the HTTP verb you want to use. The Example below will perform a GET to http://jsonplaceholder.typicode.com/posts/. 

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts.Get();
```
> Note all HTTP methods are async

## Resources

### Accessing resource by identity 

Accessing a resource by identity works as you would expect for example if your wanted to perform a GET against http://jsonplaceholder.typicode.com/posts/1 you would do the following:

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(1).Get();
```

For GET, HEAD, DELETE you can also pass the resource identity in the method for example:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
await client.Posts().Delete(1);
```
### Nested resources

Nested resources again work as you would expect for example if your wanted to perform a GET against http://jsonplaceholder.typicode.com/posts/2/comments you would do the following:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(2).Comments.Get()
```

### Awkward resources

You will always come across an API that has an resource that isn't valid C# syntax. To escape invalid C# syntax in a resource use the Resource method for example:

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(2).Resource("awakward-resource-with-dashes").Get()
```

## The dynamic return type
The dynamic return type from a HTTP verb method is a type that represents the content of the resource you have performed an action on. For example:

```cs
 dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
 
 var post = await client.Posts.Get(1);
 
 Assert.That(post.id, Is.EqualTo(1));
```

The variable post represents a post object with the properties and values returned from GET http://jsonplaceholder.typicode.com/posts/1

> Note only works for JSON content

The dynamic type has one more trick to make things syntactically convenient - the returned HttpResponseMessage is attached to the dynamic type making it easy to access things like the status code for example:

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = await client.Posts.Get(1);

Assert.That(post.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
```

> This works for all payloads, note that the Content is already disposed (see disposing) if you want access the returned Content string call ToString().

## Methods

##  Get, Delete, Head

Performs a HTTP request on a resource. Takes two parameters both are optional, first parameter is an object (must be a primitive type) representing the resource identity, second parameter is a Dictionary<string,string> the key is a string representing the header field for example "Content-Type", and the value is a string representing the header field value for example "application/json".

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = await client.Posts.Get(1);
```

Members can also take the resource an object (must be a primitive type) representing the resource identity.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = await client.Posts(1).Get();
```

## Put, Post

## Default Headers

## Implicit casting

## Collections

## Sync

## Working with non JSON content

## Disposing

Content Already
