
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Concurrent;
using System.Text;
namespace CrashrISPO.Helper;


public static class Helper
{
    public static void CreateCsv(List<Dictionary<string, string>> result, int startEpoch, int endEpoch, string apiName)
    {
        using (var writer = new StreamWriter($"{apiName}_{startEpoch}-{endEpoch}_data.csv"))
        {
            if (result.Any())
            {
                var headers = result.SelectMany(dict => dict.Keys).Distinct().ToArray();
                writer.WriteLine(string.Join(",", headers));

                foreach (var dict in result)
                {
                    var row = headers.Select(header => dict.ContainsKey(header) ? dict[header] : string.Empty);
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }
    }

    public static double ConvertLovelaceStringToADA(string lovelace)
    {
        return Convert.ToDouble(lovelace) / 1000000;
    }

    public static Dictionary<string, Dictionary<string, double>> SortByTotalRewards(Dictionary<string, Dictionary<string, double>> dict)
    {
        return dict.OrderByDescending(kv => kv.Value["total_rewards"]).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static Dictionary<string, double> CoinectaSortByTotalRewards(Dictionary<string, double> dict)
    {
        return dict.OrderByDescending(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static Dictionary<string, Dictionary<string, double>> SortByTotalADAStaked(Dictionary<string, Dictionary<string, double>> dict)
    {
        return dict.OrderByDescending(kv => kv.Value["total_ADA_staked"]).ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static List<Dictionary<string, string>> SortByEpoch(List<Dictionary<string, string>> list)
    {
        return list.OrderBy(d => Convert.ToDouble(d["epoch"])).ToList();
    }

    public static List<string> ExtractAddresses(Dictionary<string, Dictionary<string, double>> dict)
    {
        List<string> addresses = new();
        foreach(var item in dict)
        {
            addresses.Add(item.Key);
        }

        return addresses;
    }

     public static void ConvertDictionaryToCsv(Dictionary<string, Dictionary<string, double>> dict, string filePath)
    {
        var csv = new StringBuilder();


        csv.AppendLine("Stake Address,Total ADA Staked,Total Rewards");

     
        foreach (var outerEntry in dict)
        {
            string stakeAddress = outerEntry.Key;
            double totalAdaStaked = outerEntry.Value.ContainsKey("total_ADA_staked") ? outerEntry.Value["total_ADA_staked"] : 0;
            double totalRewards = outerEntry.Value.ContainsKey("total_rewards") ? outerEntry.Value["total_rewards"] : 0;

            csv.AppendLine($"{stakeAddress},{totalAdaStaked},{totalRewards}");
        }

        File.WriteAllText(filePath, csv.ToString());
    }

    
}