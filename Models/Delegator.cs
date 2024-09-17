namespace CrashrISPO.Models;

public record Delegator
{
    public int MainPoolStartEpoch { get; set; }
    public int EndEpoch { get; set; }
    public int StartEpoch { get; set; }
    public Dictionary<string, string> PoolIds { get; set; }
}