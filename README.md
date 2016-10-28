# DalSoft.RestClient

Inspired by Simple.Data and angular's $http service, DalSoft.RestClient is a very lightweight wrapper around System.Net.HttpClient that uses the dynamic features of .NET 4 to provide a fluent way of accessing RESTFul API's. 

Originally created to remove the boilerplate code involved in creating integration tests and SDK's for RESTFul API's. I know there are a couple of dynamic rest clients out there but I wanted the syntax to look a particular way, and I wanted it to be particularly useful for testing.

> This library is biased towards JSON content and is setup by default with accept and content headers for JSON. See Working with non JSON content

## Supported Platforms

DalSoft.RestClient targets .NET Standard 1.4 therefore supports Windows, Linux, Mac and Xamarin (iOS, Android and UWP).  

## Getting Started

Install via NuGet

```dos
PM> Install-Package DalSoft.RestClient
```

You start by new'ing up the RestClient and passing in the base uri for your RESTful API. Then simply chain members that would make up the resource you want to access - ending with the HTTP method you want to use. The example below will perform a GET on http://jsonplaceholder.typicode.com/posts/: 

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts.Get();
```
> Note all HTTP methods are async

## Resources

### Accessing a resource by identity 

Accessing a resource by identity works as you would expect for example if your wanted to perform a GET on http://jsonplaceholder.typicode.com/posts/1 you would do the following:

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(1).Get();
```

For GET, HEAD, DELETE you can also pass the resource identity in the method for example:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts().Delete(1);
```

> The resource identity can be any [primitive type](https://msdn.microsoft.com/en-us/library/aa711900%28v=vs.71%29.aspx) 

### Nested resources

Nested resources again work as you would expect for example if your wanted to perform a GET on http://jsonplaceholder.typicode.com/posts/2/comments you would do the following:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(2).Comments.Get()
```

### Awkward resources

You will come across an API that has a resource that isn't valid C# syntax. To escape invalid C# syntax in a resource use the Resource method for example:

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(2).Resource("awkward-resource-with-dashes").Get()
```

## The dynamic return type

The dynamic return type from a HTTP method is a type deserialized from the content of the response to your request. 

Example usage:
```cs
 dynamic client = new RestClient("http://jsonplaceholder.typicode.com");
 
 var post = await client.Posts.Get(1);
 
 Assert.That(post.id, Is.EqualTo(1));
```

The variable post represents a post object with the properties and values returned from GET http://jsonplaceholder.typicode.com/posts/1

> Note this only works for JSON content

The dynamic type has one more trick to make things syntactically convenient - the returned HttpResponseMessage is attached to the dynamic type making it easy to access things like the status code. 

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = await client.Posts.Get(1);

Assert.That(post.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
```

> This works for all content, note that the HttpContent is already disposed (see disposing) if you want access the returned content string call ToString().

## Methods

> You can find more usage examples in the DalSoft.RestClient.Test.Integration project.

###  Get, Delete, Head

Performs a HTTP request on a resource. Takes two parameters both are optional, first parameter is an object (must be a [primitive type](https://msdn.microsoft.com/en-us/library/aa711900%28v=vs.71%29.aspx)  type) representing the resource identity, second parameter is a Dictionary<string,string> the key is a string representing the header field for example "Accept", and the value is a string representing the header field value for example "application/json".

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts.Get(1, new Dictionary<string, string> {{ "Accept", "application/json" }});
```
Members also optionally take an object (must be a [primitive type](https://msdn.microsoft.com/en-us/library/aa711900%28v=vs.71%29.aspx) ) representing the resource identity.

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(1).Get();
```

### Query
Query is used to add a query string to the uri. Takes one mandatory parameter an anonymous object representing the query string. The anonymous object also supports array values representing the query string.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts().Query(new { id = 2 }).Get(); //http://jsonplaceholder.typicode.com/posts?id=2
```

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts().Query(new { id = new[] { 1, 2 } }).Get(); 
//http://jsonplaceholder.typicode.com/posts?id=1&id=2
```

### Put, Post, Patch

Performs a HTTP action on a resource. Takes two parameters both are optional, first parameter is an object (can be a anonymous type or a static object type) representing the data you want to submit, second parameter is a Dictionary<string,string> the key is a string representing the header field for example "Content-Type", and the value is a string representing the header field value for example "application/json".

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = new {  title="foo", body="bar", userId=10 };

await client.Posts.Post(post, new Dictionary<string, string> {{ "Accept", "application/json" }});
```

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = new Post { title = "foo", body = "bar", userId = 10 };

var result = await client.Posts(1).Put(post);
```

```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var post = new Post { title = "foo", body = "bar", userId = 10 };

var result = await client.Posts(1).Patch(post);
```

> Members optionally take an object (must be a [primitive type](https://msdn.microsoft.com/en-us/library/aa711900%28v=vs.71%29.aspx)  type) representing the resource identity.

### Resource
> See awkward resources

## Default Headers

DalSoft.RestClient is setup by default with accept and content headers for JSON. You can add/override the default headers by providing a Dictionary<string,string> to the constructor, the key is a string representing the header field for example "Accept", and the value is a string representing the header field value for example "application/json".

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com", { "Accept", "application/json" });
```
You can add/override the default headers via the DefaultHeaders property too.

Example usage:
```cs
dynamic client = new RestClient("http://headers.jsontest.com/");

client.DefaultRequestHeaders.Add("Accept", "application/json");
```

## Implicit casting

DalSoft.RestClient supports implicit casting to a static object type.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

Post post = await client.Posts.Get(1);
```

For convenience DalSoft.RestClient supports casting to the returned HttpResponseMessage too.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

HttpResponseMessage httpResponseMessage = await client.Posts.Get(1);

Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
```
## Collections

DalSoft.RestClient supports collections either dynamically or as static object types. 

You can iterate over the dynamic type returned from a HTTP method.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var posts = await client.Posts.Get();

foreach (var post in posts)
{

}
```

Using the dynamic type returned you can also access by index.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

var posts = await client.Posts.Get();

Assert.That(posts[0].id, Is.EqualTo(1));
```

The dynamic type returned can be cast to a collection of statically typed objects. DalSoft.RestClient supports deserializing to same types as [Json.NET](http://james.newtonking.com/json/help/index.html?topic=html/SerializationGuide.htm)  IList, IEnumerable, IList<T>, Array, IDictionary, IDictionary<TKey, TValue> etc.

Example usage
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

List<Post> posts = await client.Posts.Get();
```

## Synchronous usage

The HTTP methods don't support synchronous usage but as they return a Task you can just call Result.

Example usage:
```cs
dynamic client = new RestClient(BaseUri);
client.Posts(1).Get().Result;
```
## Working with non JSON content

Although DalSoft.RestClient is biased towards RESTFul API's returning JSON, you can use it to access anything, the only difference is implicit casting isn't supported.

Example usage:
```cs
dynamic google = new RestClient("https://www.google.com", new Dictionary<string, string>{ {"accept", "text/html"} });

var result = await google.News.Get();

Assert.That(result.HttpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
Assert.That(result.ToString(), Is.StringContaining("Top Stories"));
```

> If there is enough appetite to support implicit casting for other content such as XML let me know.

## Disposing

The HttpContent object is disposed for you, so trying to read the Content stream ReadAsStringAsync() etc will throw an exception. You can access the returned content as a string by calling ToString(). Disposing of RestClient and therefore the underline HttpClient is left up to you. [The general advise is to create one instance for the lifetime of your application](http://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/) as the RestClient and HttpClient it wraps are generally stateless and reusable across multiple calls.
