namespace CrashrISPO.Models;

public record Asset
{
    public string PolicyId { get; set; }
    public List<string> AssetNames { get; set; }
 
}