namespace CrashrISPO.Models;

public record BonusParameters
{
    
   
    public Dictionary<string, Dictionary<string, double>> Rewards { get; set; }
    public AssetHoldingsResponse AssetHoldings{get; set;}
}