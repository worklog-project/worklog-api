namespace worklog_api.Model.form;

public class DailyModel
{
    
    public Guid _id {get; set;}
    public Guid _egiId {get; set;}
    public Guid _cnId {get; set;}
    public DateTime _date {get; set;}
    public int _count {get; set;}
    public string _groupLeader { get; set; }
    public string _mechanic { get; set;}
    public string document {get; set;}

    public DailyModel()
    {
    }

    public DailyModel(Guid id, Guid egiId, Guid cnId, DateTime date, int count, string groupLeader, string mechanic, string document)
    {
        _id = id;
        _egiId = egiId;
        _cnId = cnId;
        _date = date;
        this._count = count;
        _groupLeader = groupLeader;
        _mechanic = mechanic;
        this.document = document;
    }
}