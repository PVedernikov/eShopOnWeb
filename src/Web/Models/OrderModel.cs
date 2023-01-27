namespace Microsoft.eShopWeb.Web.Models;

public class OrderModel
{
    //[JsonProperty("id")]
    public int OrderId { get; set; }

    //[JsonProperty("address")]
    public AddressModel Address { get; set; }

    //[JsonProperty("items")]
    public Dictionary<string, int> Items { get; set; }

    public decimal Total { get; set; }
}
