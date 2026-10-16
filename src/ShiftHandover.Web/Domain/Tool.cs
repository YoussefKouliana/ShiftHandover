namespace ShiftHandover.Web.Domain;

public class Tool
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;   // e.g. "V-2041"
    public string Name { get; set; } = string.Empty;     // e.g. "Klippverktyg"

    // One tool makes exactly one part
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;

    // Tools move between machines. Null means it is in the tool room.
    public int? CurrentMachineId { get; private set; }
    public Machine? CurrentMachine { get; private set; }

    public string DisplayName => $"{Number} {Name}";

    public void MountOn(Machine machine)
    {
        CurrentMachine = machine;
    }

    public void Unmount()
    {
        CurrentMachine = null;
        CurrentMachineId = null;
    }
}