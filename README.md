# Unawares

Just some AspNetCore middlewares.

Currently there's only one.

## Exception Mapper

Go

```csharp
app.UseExceptionMapper(
	builder => builder
		.Map<ArgumentException>(HttpStatusCode.BadRequest)
		.Map<EntityNotFoundException>(HttpStatusCode.NotFound)
		.Map<Exception>(
			status: HttpStatusCode.InternalServerError, 
			criteria: exception => exception.Message.StartsWith("oh no!")
		)
);
```

to map some exceptions to some HTTP status codes.

Please register it BEFORE your web app so it can catch its exceptions:

```csharp
app.UseExceptionMapper(...);

// (...)

app.UseEndpoints(...);

```


## Landing Page redirector

It's just

```csharp
app.UseLandingPage("swagger");
```

to redirect (HTTP 302) to e.g. the swagger UI.