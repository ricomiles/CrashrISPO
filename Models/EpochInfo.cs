namespace CrashrISPO.Models;

public record EpochInfo 
{   
    public double total_ADA_staked {get; set;}
    
    public double active_pools {get; set;}
    public  Dictionary<string, PoolInfo> poolInfo {get; set;}
}