namespace ShiftHandover.Web.Domain;

public class Machine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Issue> Issues { get; set; } = new();
}