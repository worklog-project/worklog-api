namespace worklog_api.Model.form;

public class AllDailyWorkLogDTO
{
    public Guid Id { get; set; }
    public string EgiName { get; set; }
    public string CodeNumber { get; set; }
    public DateTime Date { get; set; }
    public string GroupLeader { get; set; }
    public string Mechanic { get; set; }
    public List<FormIdDTO> FormId { get; set; } = new List<FormIdDTO>();
}