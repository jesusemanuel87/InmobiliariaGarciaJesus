namespace InmobiliariaGarciaJesus.Models
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string Search { get; set; } = string.Empty;
        public int OrderColumn { get; set; }
        public string OrderDirection { get; set; } = "asc";
    }
}
