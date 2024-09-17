namespace CrashrISPO.Models;

public record Reward
{
    public string StakeAddress { get; set; }
   
    public Dictionary<string, double> DelegatorRewards { get; set; }
}