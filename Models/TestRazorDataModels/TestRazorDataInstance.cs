using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace iTextDesignerWithGUI.Models.TestRazorDataModels
{
    public class TestRazorDataInstance
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }
        
        [JsonProperty("orders")]
        public List<Order> Orders { get; set; }
    }

    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class Preferences
    {
        [JsonProperty("notifications")]
        public Notifications Notifications { get; set; }
        
        [JsonProperty("theme")]
        public string Theme { get; set; }
        
        [JsonProperty("language")]
        public string Language { get; set; }
    }

    public class Notifications
    {
        [JsonProperty("email")]
        public bool Email { get; set; }
        
        [JsonProperty("sms")]
        public bool Sms { get; set; }
        
        [JsonProperty("push")]
        public bool Push { get; set; }
    }

    public class Order
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }
        
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        
        [JsonProperty("items")]
        public List<Item> Items { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Item
    {
        [JsonProperty("item_id")]
        public string ItemId { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        
        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
