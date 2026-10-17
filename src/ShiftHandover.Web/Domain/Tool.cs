namespace ShiftHandover.Web.Domain;

public class Tool
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;   // e.g. "1101"

    // Tools move between machines. Null means it is in the tool room.
    public int? CurrentMachineId { get; private set; }
    public Machine? CurrentMachine { get; private set; }

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