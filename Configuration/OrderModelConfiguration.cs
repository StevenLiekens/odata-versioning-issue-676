namespace Microsoft.Examples.Configuration
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Models;

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
    }
}