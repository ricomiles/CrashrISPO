namespace CrashrISPO.Models;

public record PoolInfo
{
    public string pool_name {get; set;}
    public int stakes_count {get; set;}
    public double total_ADA_stakes {get; set;}
}