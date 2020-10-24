namespace Microsoft.Examples.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

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
}