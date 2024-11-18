using worklog_api.Model.dto;

namespace worklog_api.Model;

public class DailyModel
{

    public Guid _id { get; set; }
    public Guid _egiId { get; set; }
    public Guid _cnId { get; set; }
    public DateTime _date { get; set; }

    public int _hourmeter { get; set; }

    public TimeSpan _startTime { get; set; }

    public TimeSpan _endTime { get; set; }

    public string _formType { get; set; }
    
    public string _cnName { get; set; }
    public string _egiName { get; set; }

    public int _count { get; set; }
    public string _groupLeader { get; set; }
    public string _mechanic { get; set; }

    public virtual Dictionary<string, object> _sheetDetail { get; set; }
    public Guid _dailyId { get; set; }

    public DailyModel()
    {
    }

    public DailyModel(Guid id, Guid egiId, Guid cnId, DateTime date, int count, string groupLeader, string mechanic, Dictionary<string, object> document, int hourMeter, TimeSpan startTime, TimeSpan endTime, string formType)
    {
        _id = id;
        _egiId = egiId;
        _cnId = cnId;
        _date = date;
        _count = count;
        _groupLeader = groupLeader;
        _mechanic = mechanic;
        _sheetDetail = document;
        _hourmeter = hourMeter;
        _startTime = startTime;
        _endTime = endTime;
        _formType = formType;
    }
}