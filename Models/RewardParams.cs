namespace CrashrISPO.Models;

public record RewardParams
{
    public double MainRate { get; set; }
    public double PartnerRate { get; set; }
    public List<Dictionary<string, string>> DelegatorData { get; set; }
}