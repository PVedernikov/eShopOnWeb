using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeliveryOrderProcessor.Models;
public class Order
{
    //[JsonProperty("id")]
    public Guid? id { get; set; }

    public int OrderId { get; set; }

    //[JsonProperty("address")]
    public Address Address { get; set; }

    //[JsonProperty("items")]
    public Dictionary<string, int> Items { get; set; }

    public decimal Total { get; set; }
}
