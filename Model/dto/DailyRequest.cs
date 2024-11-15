using System.Text.Json.Serialization;
using Azure.Core.Serialization;
using worklog_api.Model.dto;

namespace worklog_api.Model.form;

public class DailyRequest
{
    [JsonPropertyName("egiId")]
    public string _egiId {get; set;}
    [JsonPropertyName("cnId")]
    public string _cnId {get; set;}
    [JsonPropertyName("date")]
    public string _date {get; set;}
    
    [JsonPropertyName("groupLeader")]
    public string _groupLeader { get; set; }
   
    [JsonPropertyName("mechanic")]
    public string _mechanic { get; set;}

    [JsonPropertyName("form")]
    public FormDTO _form {get; set;}
    
    public DailyRequest()
    {
    }

    public DailyRequest(string egiId, string cnId, string date,  string groupLeader, string mechanic, FormDTO form)
    {
        _egiId = egiId;
        _cnId = cnId;
        _date = date;
        _groupLeader = groupLeader;
        _mechanic = mechanic;
        _form = form;
    }
}