using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using worklog_api.Model.dto;

namespace worklog_api.Model.form;

public class DailyRequest
{
    [JsonPropertyName("egi")]
    [JsonPropertyOrder(1)]
    public string _egiId {get; set;}
    
    [JsonPropertyName("codeNumber")]
    [JsonPropertyOrder(2)]
    public string _cnId {get; set;}
    
    [JsonPropertyName("tanggal")]
    [JsonPropertyOrder(3)]
    public string _date {get; set;}
    
    [JsonPropertyName("hourMeter")]
    [JsonPropertyOrder(4)]
    public double _hourmeter {get; set;}
    
    [JsonPropertyName("startTime")]
    [JsonPropertyOrder(5)]
    public TimeSpan _startTime {get; set;}
    
    [JsonPropertyName("finishTime")]
    [JsonPropertyOrder(6)]
    public TimeSpan _endTime {get; set;}
    
    [JsonPropertyName("typeSheet")]
    [JsonPropertyOrder(7)]
    public string _formType {get; set;}
    
    [JsonPropertyName("groupLeader")]
    [JsonPropertyOrder(8)]
    public string _groupLeader { get; set; }
    
    [JsonPropertyName("mechanic")]
    [JsonPropertyOrder(9)]
    public string _mechanic { get; set;}
    
    public Guid _dailyId {get; set;}
    
    [JsonPropertyName("detailSheet")]
    [JsonPropertyOrder(10)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual Dictionary<string, object> _sheetDetail { get; set; }
    
    public DailyRequest()
    {
    }

    public DailyRequest(string egiId, string cnId, string date,  string groupLeader, string mechanic)
    {
        _egiId = egiId;
        _cnId = cnId;
        _date = date;
        _groupLeader = groupLeader;
        _mechanic = mechanic;
        _sheetDetail = new Dictionary<string, object>();
    }
}