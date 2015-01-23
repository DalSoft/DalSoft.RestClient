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
The dynamic return type from a HTTP method is a type deserialized from the content of the resource you have performed a request on. For example:

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

You can find more  examples of usage in DalSoft.RestClient.Test.Integration project.

###  Get, Delete, Head

Performs a HTTP request on a resource. Takes two parameters both are optional, first parameter is an object (must be a primitive type) representing the resource identity, second parameter is a Dictionary<string,string> the key is a string representing the header field for example "Accept", and the value is a string representing the header field value for example "application/json".

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts.Get(1, new Dictionary<string, string> {{ "Accept", "application/json" }});
```

Members can also optionally take an object (must be a primitive type) representing the resource identity.

Example usage:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts(1).Get();
```

### Query
Query is used to add a query string to the uri. Takes one mandortory parameter an anoyumous object representing the query string.
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

await client.Posts().Query(new { id = 2 }).Get(); //http://jsonplaceholder.typicode.com/posts?id=2
```

### Put, Post

Performs a HTTP action on a resource. Takes two parameters both are optional, first parameter is an object (can be anonymous type or a static object type) representing the data you want to submit, second parameter is a Dictionary<string,string> the key is a string representing the header field for example "Content-Type", and the value is a string representing the header field value for example "application/json".

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

Members optionally take an object (must be a primitive type) representing the resource identity.

### Resource
> See awkward resources

## Default Headers

DalSoft.RestClient is setup for JSON by default. You can add/override default headers by providing a Dictionary<string,string> the key is a string representing the header field for example "Accept", and the value is a string representing the header field value for example "application/json".

Example:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com", { "Accept", "application/json" });
```
You can add/override default headers via the DefaultHeaders property too.

Example:
```cs
dynamic client = new RestClient("http://headers.jsontest.com/");

client.DefaultRequestHeaders.Add("Accept", "application/json");
```

## Implicit casting

DalSoft.RestClient supports implicit casting to a static object type.

Example:
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

Post post = await client.Posts.Get(1);
```

For convenience DalSoft.RestClient supports casting to HttpResponseMessage.
```cs
dynamic client = new RestClient("http://jsonplaceholder.typicode.com");

HttpResponseMessage httpResponseMessage = await client.Posts.Get(1);

Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
```
## Collections

DalSoft.RestClient supports collections either dynamically or as static object types. 

You can iterate the dynamic type returned from HTTP method.

Example:
```cs
dynamic client = new RestClient(BaseUri);
var posts = await client.Posts.Get();

foreach (var post in posts)
{

}
```

Using the dynamic type returned you can also also by index.

Example:
```cs
dynamic client = new RestClient(BaseUri);

var posts = await client.Posts.Get();

Assert.That(posts[0].id, Is.EqualTo(1));
```

DalSoft.RestClient supports deserializing to same types as [Json.NET](http://james.newtonking.com/json/help/index.html?topic=html/SerializationGuide.htm)  IList, IEnumerable, IList<T>, Array, IDictionary, IDictionary<TKey, TValue> etc.

Using implicit casting the dynamic type returned can be cast to a List of statically typed objects.

Example:
```cs
dynamic client = new RestClient(BaseUri);

List<Post> posts = await client.Posts.Get();
```

## Synchronous usage

The HTTP methods don't support synchronous usage but as they return Task you can just call the Result method.

Example:
```cs
dynamic client = new RestClient(BaseUri);
client.Posts(1).Get().Result;
```
## Working with non JSON content

Although DalSoft.RestClient is biased towards JSON you can use it to access any content, the only different is implicit casting isn't supported.

Example:
 

## Disposing

HttpContent is disposed for you, so trying to read the Content stream ReadAsStringAsync() etc will throw an exception, you can access the Content as a string by calling ToString(). Disposing of RestClient and therefore the underline HttpClient is left up to you. The general advise is to create one for the lifetime of your application as the RestClient and HttpClient it wraps are generally stateless and reusable across multiple calls.
