namespace CrashrISPO.Functions;

using Newtonsoft.Json.Linq;
using CrashrISPO.Helper;
using System.Net.Http;
using System.Text;

public static class Functions
{
    public static async Task<List<Dictionary<string, string>>> FetchDelegatorHistory(HttpClient client, string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var json = JArray.Parse(content);

        var data = json.ToObject<List<Dictionary<string, string>>>();

        return data;
    }

    public static async Task<List<Dictionary<string, string>>> FetchAssets(HttpClient client, string stakeAddress)
    {
        string url = "https://api.koios.rest/api/v1/account_assets";
        var jsonBody = $"{{\"_stake_addresses\":[\"{stakeAddress}\"]}}";

        var body = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, body);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var json = JArray.Parse(content);

        var data = json.ToObject<List<Dictionary<string, string>>>();

        return data;
    }

    public static Dictionary<string, string> InitDelegator(int currentIndex, int startEpoch, string poolName, string poolId, Dictionary<string, string> delegatorData)
    {
        Dictionary<string, string> delegator = new();

        if (currentIndex < startEpoch)
        {
            delegator["pool_name"] = "CRASH";
            delegator["pool_id"] = "pool1j8zhlvakd29yup5xmxtyhrmeh24udqrgkwdp99d9tx356wpjarn";

        }
        else
        {
            delegator["pool_name"] = poolName;
            delegator["pool_id"] = poolId;
        }

        delegator["epoch"] = delegatorData["epoch_no"];
        delegator["stake_address"] = delegatorData["stake_address"];
        delegator["amount"] = delegatorData["amount"];

        return delegator;

    }

    public static Dictionary<string, double> InitRewards(Dictionary<string, string> delegatorData, double mainRate, double partnerRate)
    {
        Dictionary<string, double> delegatorReward = new();

        var stakeAddress = delegatorData["stake_address"];
        var amount = Helper.ConvertLovelaceStringToADA(delegatorData["amount"]);


        delegatorReward["total_ADA_staked"] = amount;
        if (delegatorData["pool_name"] == "CRASH")
        {
            delegatorReward["total_rewards"] = amount * mainRate;
            delegatorReward["main_pool_epoch_count"] = 1;
        }
        else
        {
            delegatorReward["total_rewards"] = amount * partnerRate;
            delegatorReward["main_pool_epoch_count"] = 0;
        }

        return delegatorReward;
    }

    public static Dictionary<string, Dictionary<string, double>> UpdateRewards(Dictionary<string, string> delegatorData, Dictionary<string, Dictionary<string, double>> delegatorRewards, double mainRate, double partnerRate)
    {
        var stakeAddress = delegatorData["stake_address"];
        var amount = Helper.ConvertLovelaceStringToADA(delegatorData["amount"]);

        delegatorRewards[stakeAddress]["total_ADA_staked"] += amount;
        if (delegatorData["pool_name"] == "CRASH")
        {
            delegatorRewards[stakeAddress]["total_rewards"] += amount * mainRate;
            delegatorRewards[stakeAddress]["main_pool_epoch_count"] += 1;
        }
        else
        {
            delegatorRewards[stakeAddress]["total_rewards"] += amount * partnerRate;

        }

        return delegatorRewards;
    }

    public static double AddBonuses(Dictionary<string, double> delegatorReward, List<Dictionary<string, string>> assetList)
    {
        string requiredPolicyId = "5f3a47fa83522c6eece53bbfe0a77374bd748d6ac5b8892d3d5b3ff3"; //temporary
        string requiredAssetName = "5365636972697479"; // temporary

        int validAssetCount = 0;
        bool isBoomer = false;

        double epochCount = delegatorReward["main_pool_epoch_count"];
        double totalADAStaked = delegatorReward["total_ADA_staked"];
        double totalRewards = delegatorReward["total_rewards"];

        if (epochCount >= 24 && totalADAStaked >= 5000)
        {
            if (totalADAStaked >= 50000)
            {
                totalRewards += totalRewards * 0.20;
            }
            else
            {
                totalRewards += totalRewards * 0.12;
            }
        }
        else if (epochCount >= 23)
        {
            totalRewards += totalRewards * 0.08;
        }
        else if (epochCount >= 20)
        {
            totalRewards += totalRewards * 0.05;
        }

        if (assetList.Count > 0)
        {

            foreach (var asset in assetList)
            {
                if (asset["policy_id"] == requiredPolicyId)
                {
                    validAssetCount += 1;

                    if (asset["asset_name"] == requiredAssetName)
                    {
                        isBoomer = true;
                    }
                }
            }

        }
        if (epochCount >= 23 && totalADAStaked >= 500)
        {
            if (validAssetCount >= 12)
            {
                totalRewards += totalRewards * 0.1;
            }
            else if (isBoomer)
            {
                totalRewards += totalRewards * 0.005;
            }
        }

        return totalRewards;

    }

}