## Repro steps

1. Start the API
2. GET <http://localhost:5000/api/v1/orders/42>
3. GET <http://localhost:5000/api/v2/orders/42>

**Expected result**

```
/api/v1/orders/42

{
  "@odata.context": "http://localhost:5000/api/v%7Bversion:apiVersion%7D/$metadata#Orders/$entity",
  "id": 0,
  "createdDate": "2000-12-12",
  "customer": "Bill Mei"
}

/api/v2/orders/42

{
  "@odata.context": "http://localhost:5000/api/v%7Bversion:apiVersion%7D/$metadata#Orders/$entity",
  "id": 0,
  "createdDate": "2012-12-12T12:12:12.121+02:00",
  "customer": "Bill Mei"
}
```

**Actual result**

The call to v2 seems to use the model property for v1. Other than that it does actually use the correct EDM type.

```
/api/v1/orders/42 (OK)

{
  "@odata.context": "http://localhost:5000/api/v%7Bversion:apiVersion%7D/$metadata#Orders/$entity",
  "id": 0,
  "createdDate": "2000-12-12",
  "customer": "Bill Mei"
}

/api/v2/orders/42 (Not OK)

{
  "@odata.context": "http://localhost:5000/api/v%7Bversion:apiVersion%7D/$metadata#Orders/$entity",
  "id": 0,
  "createdDate": "2000-12-12T00:00:00+01:00",
  "customer": "Bill Mei"
}
```

This bug also manifests itself in another way.

1. Restart the API
2. GET <http://localhost:5000/api/v2/orders/42>
3. GET <http://localhost:5000/api/v1/orders/42>

The call to v1 crashes with an exception.

```
Microsoft.OData.ODataException: An incompatible primitive type 'Edm.DateTimeOffset[Nullable=False]' was found for an item that was expected to be of type 'Edm.Date[Nullable=False]'.
```

## Points of interest

**Order.cs** has a _CreatedDate_ as `Edm.Date` in V1 which is replaced by `Edm.DateTimeOffset` in V2

Values are constants for demonstration purposes. I changed the year part to better understand what's happening.

``` cs
public class Order
{
    public int Id { get; set;  }

    // 2000-12-12
    public DateTime CreatedDate { get; set; } = new DateTime(2000, 12, 12);

    // 2012-12-12T12:12:12.121+02:00
    public DateTimeOffset CreatedDateV2 { get; set; } = new DateTimeOffset(2012, 12, 12, 12, 12, 12, 121, TimeSpan.FromHours(+2));

    [Required]
    public string Customer { get; set; }
}

public class OrderModelConfiguration : IModelConfiguration
{
    private static readonly ApiVersion V1 = new ApiVersion( 1, 0 );

    private static readonly ApiVersion V2 = new ApiVersion( 2, 0 );

    private EntityTypeConfiguration<Order> ConfigureCurrent( ODataModelBuilder builder )
    {
        var order = builder.EntitySet<Order>( "Orders" ).EntityType;

        order.HasKey( p => p.Id );

        return order;
    }

    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string routePrefix )
    {
        var order = ConfigureCurrent( builder );

        if ( apiVersion == V1 )
        {
            order.Property( o => o.CreatedDate ).AsDate();
            order.Ignore( o => o.CreatedDateV2 );
        }
        else
        {
            order.Ignore( o => o.CreatedDate );
            order.Property( o => o.CreatedDateV2 ).Name = nameof( Order.CreatedDate );
        }
    }
```