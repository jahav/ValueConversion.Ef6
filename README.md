# ValueConverter.Ef6
A class for using custom types in Entity Framework 6, similar to ValueConverter in EFCore.

# Materialize custom types

* A typed Id, but no lock, you are stuck with `GUID` or `int` ect.
* NodaTime types, but no, only `DateTime` or `DateTimeOffset` or `TimeStamp`

# Prerequisites
You must have a persistent model that works as is.

# Steps

1. Get a projection expression
```csharp
ctx.Customers
.Where(x => x.Id == 5)
.Select(x => new Customer 
{
  Id = (Id<Customer>)x.Id, 
  Name = new CustomerName 
  {
    GivenName = x.FirstName, 
    Surname = x.LastName, 
  },
  ContactAddress = new Address
  {
    Street = x.HomeStreet
  },
  WorkAddress = new Address
  {
    Street = x.WorkStreet
    City = x.WorkCity
  },
  BirthNumber = (BirthNumber)x.BirthNumber
});
```

2. Get a list of all types that are instantiated in the select clause in the projection query
In our case, the query materializes following types:
* `Customer`
* `CustomerName`
* `Address`
and uses two custom types that will require custom value converters:
* `Id<Customer>`
* `BirthNumber`

3. Create a new types for each materialized using reflection
Each settable property will be in anon types, because creating a new type is not very cheap, so caching and reusing of anon types between many queries is a must.

4. Transform projection expression materialized types to anon types
* Each type will be 
```csharp
IQueryable<Customer> projectionQuery = ctx.Customers
.Where(x => x.Id == 5)
.Select(x => new /* AnonCustomer */
{
  Id = x.Id, // WAS:int->Id<Customer>; NOW: int -> int
  Name = new /* AnonCustomerName */
  {
    GivenName = x.FirstName, 
    Surname = x.LastName, 
  },
  ContactAddress = new /* AnonAddress */  // Contact address and Work address use same anon type
  {
    Street = x.HomeStreet
  },
  WorkAddress = new /* AnonAddress */
  {
    Street = x.WorkStreet
    City = x.WorkCity
  },
  BirthNumber = x.BirthNumber // WAS:string->BirthNumber; NOW: string -> string
});
```
5. Create a select expression for mapping anon query into target query
Note that names of properties are same, it should be 1:1 mapping with casing only
```csharp
Expression<Func<AnonCustomer,Customer>> selectExpression = /*AnonCustomer*/ x => new Customer
{
  Id = (Id<Customer>)x.Id,
  Name = new CustomerName
  {
    GivenName = x.GivenName, 
    Surname = x.Surname, 
  },
  ContactAddress = new Address
  {
    Street = x.Street
  },
  WorkAddress = new Address
  {
    Street = x.Street
    City = x.City
  },
  BirthNumber = (BirthNumber)x.BirthNumber // PROJECTION:string->BirthNumber; MATERIALIZED: string -> string;
});
```

6. Execute query
```csharp
var projectToAnonQuery = visitor.Visit(projectionQuery);
var result = projectToAnonQuery.ToList().Select(selectExpression).ToList();
```

and you are done (^____^)!


# Roadmap
## 1.0 scope
* Properties, 
* collections, 
* custom types for a single property 
## 1.0 out of scope
* anonymous types (requires on the fly generation)
* inheritance
* polymorphism
* fields

