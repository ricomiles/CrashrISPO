
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


    public static Dictionary<string, Dictionary<string, double>> SortByTotalADAStaked(Dictionary<string, Dictionary<string, double>> dict)
    {
        return dict.OrderByDescending(kv => kv.Value["total_ADA_staked"]).ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}