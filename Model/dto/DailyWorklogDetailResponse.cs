using System.Text.Json.Serialization;

namespace worklog_api.Model.dto;

public class DailyWorklogDetailResponse
{
    [JsonPropertyName("id")]
    public string _id {get; set;}
    
    [JsonPropertyName("hourMeter")]
    public int _hourmeter {get; set;}
    
    [JsonPropertyName("startTime")]
    public TimeSpan _startTime {get; set;}
    
    [JsonPropertyName("finishTime")]
    public TimeSpan _endTime {get; set;}
    
    [JsonPropertyName("typeSheet")]
    public string _formType {get; set;}
    
    [JsonPropertyName("tanggal")]
    public string _date {get; set;}

    [JsonPropertyName("cnName")]
    public string _cnName {get; set;}

    [JsonPropertyName("egiName")]
    public string _egiName {get; set;}

    
    [JsonPropertyName("cnId")]
    public string _cnid {get; set;}

    
    [JsonPropertyName("groupLeader")]
    public string _groupLeader {get; set;}
    
    [JsonPropertyName("mechanic")]
    public string _mechanic {get; set;}

    [JsonPropertyName("backlogs")]
    public List<BacklogModel> _backlogs { get; set; }

    [JsonPropertyName("detailSheet")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual Dictionary<string, object> _sheetDetail { get; set; }
    
}