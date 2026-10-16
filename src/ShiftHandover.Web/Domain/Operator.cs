namespace ShiftHandover.Web.Domain;

public class Operator
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}