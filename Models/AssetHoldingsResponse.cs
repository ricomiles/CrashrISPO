namespace CrashrISPO.Models;

public record AssetHoldingsResponse
{
    public List<Dictionary<string, string>> OneOfOneAssetHoldings { get; set; }
    public List<Dictionary<string, string>> BoomerAssetHoldings { get; set; }
}