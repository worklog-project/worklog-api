using System.Text.Json.Serialization;

namespace worklog_api.Model.dto;

public class FormDTO
{
    
    [JsonPropertyName("hourMeter")]
    [JsonPropertyOrder(1)]
    public int _hourmeter {get; set;}
    
    [JsonPropertyName("startTime")]
    [JsonPropertyOrder(2)]
    public TimeSpan _startTime {get; set;}
    
    [JsonPropertyName("endTime")]
    [JsonPropertyOrder(3)]
    public TimeSpan _endTime {get; set;}
    
    [JsonPropertyName("formType")]
    [JsonPropertyOrder(4)]
    public string _formType {get; set;}
    
    [JsonPropertyName("dailyId")]
    [JsonPropertyOrder(5)]
    public Guid _dailyId {get; set;}
    
    
    [JsonPropertyName("activities")]
    [JsonPropertyOrder(6)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual Dictionary<string, string> _activities { get; set; }
    
    [JsonPropertyName("comsumables")]
    [JsonPropertyOrder(7)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual Dictionary<string, string> _consumables { get; set; }
    
    public FormDTO()
    {
        _activities = new Dictionary<string, string>();
        _consumables = new Dictionary<string, string>();
    }
}
