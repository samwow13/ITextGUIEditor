using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace iTextDesignerWithGUI.Models.testerz1Models
{
    public class testerz1Instance
    {
        [JsonProperty("model")]
        public ModelData Model { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public class ModelData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }

    public class Properties
    {
        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }
    }

    public class Section
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("fields")]
        public List<Field> Fields { get; set; }
    }

    public class Field
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("value")]
        public object Value { get; set; }
    }
}
