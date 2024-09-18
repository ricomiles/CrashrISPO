namespace CrashrISPO.Endpoints;
using CrashrISPO.Functions;
using CrashrISPO.Helper;
using CrashrISPO.Models;
using Microsoft.AspNetCore.Mvc;

public static class Coinecta
{
    public static void MapCoinectaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/Coinecta/{startEpoch:int}/{endEpoch:int}/{fixedTotalReward:double}/{generateCsv:bool}", async (IHttpClientFactory httpClientFactory, int startEpoch, int endEpoch, double fixedTotalReward, bool generateCsv) =>
        {
            var client = httpClientFactory.CreateClient();
            List<Dictionary<string, string>> result = new();
            double totalAda = 0;
            int epoch_count = endEpoch - startEpoch + 1;
            double perEpochReward = fixedTotalReward / epoch_count;
            string url = "";

            Dictionary<string, string> poolIds = new()
            {
                {"A3C", "pool1zkdaju2rjefa52uh6yh6etsxla0x6aqs6p6wm245y5szk7k3msd"},
                {"Reet Good Cardano", "pool1ds7rtjqxauff54ny4avs7qxmvchepuv6u79kgg0sf84mjlhxznp"},
                {"Nova","pool1sf784g6eje8vzq9f5wu583eurhfq2ee9hzyhl46yezkuqqtmw07"},
                {"AKYO","pool1jsxk3ymqv2gdc6mhqk52544g2aun4zhq5wgx6n32l5s3jlne70n"},
                {"Upstream", "pool1keasvddt9vndl8jyhg204s6kqusv5zgzg3kk3l3g949ew402ahe"},
                {"FLUID", "pool18rpw5dwfywfrzjuy7uzn2cetrw9v85aan8a80p3fdvtqcxykkqu"},
                {"Open Source Software Fund", "pool1cpr59c88ps8499gtgegr3muhclr7dln35g9a3rqmv4dkxg9n3h8"},
                {"HAPPY", "pool1a8n7f97dmgtgrnl53exccknjdchqxrr9508hlxlgqp6xvjmzhej"},
                {"Stake ADA", "pool1p82vmqednsalje23mpnz9u3qt9ruj79xu83mr6p8t93fw5fcu3y"},
                {"WISE", "pool1p42f6kw9lkgw5tn77kqyrc22zrs4uq298p468gacwzw6gq0ckq2"},
                {"ELEMENTAL", "pool19ut4284xy9p82dd0cglzxweddfqw73yennkjk6mmp650chnr6lz"},
                {"Politikoz", "pool1psr42v6rvv3raeqsg9ru2zy5dp4cchlde8hw9xthrpstx5wz7wk"},
                {"Southtyrol", "pool19k4azhj8tye5wwy5pfj7q6m938m8fg5nglds5dkhtfwnvcg3s0k"},
                {"RUMOR", "pool1c30lqt59t8sjn5lg04r5wk5eucxa5h9cj05xs9gzc8lngl4cmta"},
                {"CRCI", "pool1lzupr43772kfux007sdt02vlx7uzmfcplrwmxntdztxmwv9myrj"},
            };

            Dictionary<double, EpochInfo> epochInfos= new();

            Dictionary<string, double> poolTotalStakes = new();

            int poolCount = poolIds.Count;


            for (int i = startEpoch; i <= endEpoch; i++)
            {
                
                epochInfos[i] = new EpochInfo 
                {
                    total_ADA_staked = 0,
                    active_pools = i <= 450 ? 15 : 14,
                    poolInfo = new Dictionary<string, PoolInfo>()
                };

                foreach (var entry in poolIds)
                {

                    List<Dictionary<string, string>> delegatorInfo = new();

                    
                    url = $"https://api.koios.rest/api/v1/pool_delegators_history?_pool_bech32={entry.Value}&_epoch_no={i}";
                   
                   if (i > 450 && entry.Value == "pool1ds7rtjqxauff54ny4avs7qxmvchepuv6u79kgg0sf84mjlhxznp")
                    {
                        continue;
                    }



                    var data = await Functions.FetchDelegatorHistory(client, url);


                    foreach (var dict in data)
                    {
                        var delegator = Functions.InitDelegator(i, startEpoch, entry.Key, entry.Value, dict);

                        


                        var exist = result.Any(r => r["stake_address"] == delegator["stake_address"] && r["epoch"] == delegator["epoch"]);

                        if (!exist)
                        {
                            double amount = Helper.ConvertLovelaceStringToADA(dict["amount"]);
                            totalAda += amount;

                            if(amount > 0)
                            {
                                result.Add(delegator);
                            }

                            if(poolTotalStakes.ContainsKey(delegator["pool_id"]))
                            {
                                poolTotalStakes[delegator["pool_id"]] += Helper.ConvertLovelaceStringToADA(dict["amount"]);
                                
                            }
                            else
                            {
                                poolTotalStakes[delegator["pool_id"]] = Helper.ConvertLovelaceStringToADA(dict["amount"]);
                                
                            }


                            if(!epochInfos[i].poolInfo.ContainsKey(delegator["pool_id"]))
                            {
                                 epochInfos[i].poolInfo[delegator["pool_id"]] = new PoolInfo
                                {
                                    stakes_count = 0,
                                    total_ADA_stakes = 0
                                };
                                epochInfos[i].poolInfo[delegator["pool_id"]].pool_name = delegator["pool_name"];
                            }
                            epochInfos[i].poolInfo[delegator["pool_id"]].stakes_count += amount > 0 ? 1 : 0;
                            epochInfos[i].poolInfo[delegator["pool_id"]].total_ADA_stakes += amount;
                            epochInfos[i].total_ADA_staked += amount;
                        }   


                    }

                }

            }

            
            

            Dictionary<string, Dictionary<string,double>> delegatorRewards = new();
;

            foreach (var delegatorData in result)
            {
                int activePoolCount = poolIds.Count;
                int currentEpoch = Convert.ToInt32(delegatorData["epoch"]);
                if(currentEpoch > 450)
                {
                    activePoolCount -= 1;
                }
                double amountStaked = Helper.ConvertLovelaceStringToADA(delegatorData["amount_staked(lovelace)"]);
                double perPoolReward = perEpochReward / activePoolCount;
                double indivReward = (amountStaked / poolTotalStakes[delegatorData["pool_id"]]) * perPoolReward;
                
                if (delegatorRewards.ContainsKey(delegatorData["stake_address"]))
                {
                    delegatorRewards[delegatorData["stake_address"]]["total_ADA_staked"] += amountStaked;
                    delegatorRewards[delegatorData["stake_address"]]["total_rewards"] += indivReward;
                }
                else
                {
                    delegatorRewards[delegatorData["stake_address"]] = new Dictionary<string, double>()
                    {
                        {"total_ADA_staked", amountStaked},
                        {"total_rewards", indivReward}
                    };
            
                }
               

            }
            if (generateCsv)
            {
                Helper.CreateCsv(Helper.SortByEpoch(result), startEpoch, endEpoch, "Coinecta");
            }
            // Helper.ConvertDictionaryToCsv(Helper.SortByTotalRewards(delegatorRewards), "rewards.csv" );

            Dictionary<string, double> totalRewards = new()
            {
                {"total_delegators_count",  delegatorRewards.Count},
                {"total_stakes_count", result.Count},
                {"total_ADA_staked", totalAda}
            };

            return Results.Ok(totalRewards);

            return Results.Ok(Helper.SortByTotalRewards(delegatorRewards));
            
        })
  .WithName("GetDataForCoinecta")
  .WithOpenApi();
    }
}