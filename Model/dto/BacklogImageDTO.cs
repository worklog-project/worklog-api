namespace worklog_api.Model.dto
{
    public class BacklogImageDTO
    {
        public IFormFile? ImageFile { get; set; }
        public string? Description { get; set; }
        public string? AltText { get; set; }
    }
}
