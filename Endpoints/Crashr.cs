namespace CrashrISPO.Endpoints;
using CrashrISPO.Functions;
using CrashrISPO.Helper;
using CrashrISPO.Models;
using Microsoft.AspNetCore.Mvc;

public static class Crashr
{
    public static void MapCrashrEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/Koios/{mainPoolStartEpoch}/{startEpoch:int}/{endEpoch:int}/{mainRate:double}/{spoFixedAmount:double}/{generateCsv:bool}", async (IHttpClientFactory httpClientFactory, int mainPoolStartEpoch, int startEpoch, int endEpoch, double mainRate, double spoFixedAmount, bool generateCsv) =>
    {
        var client = httpClientFactory.CreateClient();
        List<Dictionary<string, string>> result = new();
        double totalAda = 0;
        string url = "";

        Dictionary<string, string> poolIds = new()
    {
        {"HAPPY", "pool1a8n7f97dmgtgrnl53exccknjdchqxrr9508hlxlgqp6xvjmzhej"},
        {"KTOP", "pool135h773klt7djljmyawkh88e8qc457wxqfhc6j9h6y9n4y3sgh7t"},
        {"ELEMENTAL","pool19ut4284xy9p82dd0cglzxweddfqw73yennkjk6mmp650chnr6lz"},
        {"RUMOR","pool1c30lqt59t8sjn5lg04r5wk5eucxa5h9cj05xs9gzc8lngl4cmta"},
        {"CRASH", "pool1j8zhlvakd29yup5xmxtyhrmeh24udqrgkwdp99d9tx356wpjarn"},
    };

        string requiredPolicyId = "848838af0c3ab2e3027d420e320c90eb217f25b8b097efb4378e90f5";
        List<Dictionary<string, string>> assetList = new()
        {
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d626572732023313137"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353130"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353034"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353033"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353038"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d626572732023373330"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353039"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353035"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353037"}
            },
            new Dictionary<string,string>
            {
                {"policyId", requiredPolicyId},
                {"assetName", "426f6d62657273202331353036"}
            },
        };


        for (int i = mainPoolStartEpoch; i <= endEpoch; i++)
        {

            foreach (var entry in poolIds)
            {

                List<Dictionary<string, string>> delegatorInfo = new();

                if (i < startEpoch)
                {
                    url = $"https://api.koios.rest/api/v1/pool_delegators_history?_pool_bech32=pool1j8zhlvakd29yup5xmxtyhrmeh24udqrgkwdp99d9tx356wpjarn&_epoch_no={i}";
                }
                else
                {
                    url = $"https://api.koios.rest/api/v1/pool_delegators_history?_pool_bech32={entry.Value}&_epoch_no={i}";
                }



                var data = await Functions.FetchDelegatorHistory(client, url);


                foreach (var dict in data)
                {
                    var delegator = Functions.InitDelegator(i, startEpoch, entry.Key, entry.Value, dict);

                    totalAda += Helper.ConvertLovelaceStringToADA(dict["amount"]);


                    var exist = result.Any(r => r["stake_address"] == delegator["stake_address"] && r["epoch"] == delegator["epoch"]);

                    if (!exist)
                    {
                        result.Add(delegator);
                    }


                }

            }

        }



        Dictionary<string, double> totalRewards = new();
        totalRewards["main_pool_stake_count"] = 0;
        totalRewards["partner_pool_stakes_count"] = 0;
        totalRewards["main_pool_total_ada"] = 0;
        totalRewards["partner_pool_total_ada"] = 0;
        totalRewards["all_pool_total_rewards"] = 0;


        Dictionary<string, Dictionary<string, double>> delegatorRewards = new();

        double partnerRate = spoFixedAmount / totalAda;

        foreach (var delegatorData in result)
        {
            if (delegatorRewards.ContainsKey(delegatorData["stake_address"]))
            {

                delegatorRewards = Functions.UpdateRewards(delegatorData, delegatorRewards, mainRate, partnerRate);

            }
            else
            {
                delegatorRewards[delegatorData["stake_address"]] = Functions.InitRewards(delegatorData, mainRate, partnerRate);

            }
            if (delegatorData["pool_name"] == "CRASH")
            {
                totalRewards["main_pool_stake_count"] += 1;
                totalRewards["main_pool_total_ada"] += Convert.ToDouble(delegatorData["amount"]) / 1000000;
            }
            else
            {
                totalRewards["partner_pool_stakes_count"] += 1;
                totalRewards["partner_pool_total_ada"] += Convert.ToDouble(delegatorData["amount"]) / 1000000;
            }

        }
        if (generateCsv)
        {
            Helper.CreateCsv(Helper.SortByEpoch(result), mainPoolStartEpoch, endEpoch, "Koios");
        }



        List<Dictionary<string, string>> oneOfOneAssetHoldings = new();

        url = "";
        foreach (var asset in assetList)
        {
            List<Dictionary<string, string>> res = new();

            string policyId = asset["policyId"];
            string assetName = asset["assetName"];
            url = $"https://api.koios.rest/api/v1/asset_addresses?_asset_policy={policyId}&_asset_name={assetName}";
            res = await Functions.FetchAssetHoldings(client, url);

            oneOfOneAssetHoldings.AddRange(res);
        }

        url = $"https://api.koios.rest/api/v1/policy_asset_addresses?_asset_policy={requiredPolicyId}";
        var boomerAssetHoldings = await Functions.FetchAssetHoldings(client, url);

        foreach (var delegatorReward in delegatorRewards)
        {

            delegatorReward.Value["total_rewards"] = Functions.AddBonuses(delegatorReward.Value, oneOfOneAssetHoldings, boomerAssetHoldings, delegatorReward.Key);
            totalRewards["all_pool_total_rewards"] += delegatorReward.Value["total_rewards"];
        }


        return Results.Json(delegatorRewards);

    })
    .WithName("GetDataFromKoios")
    .WithOpenApi();

        app.MapPost("/fetch-delegator-data", async (
            [FromBody] Delegator delegator,
            [FromServices] IHttpClientFactory httpClientFactory) =>
        {

            var client = httpClientFactory.CreateClient();
            List<Dictionary<string, string>> result = new();
            string url = "";
            double totalAda = 0;

            object lockOject = new();


            for (int i = delegator.MainPoolStartEpoch; i <= delegator.EndEpoch; i++)
            {
                foreach (var entry in delegator.PoolIds)
                {

                    List<Dictionary<string, string>> delegatorInfo = new();

                    if (i > delegator.StartEpoch && entry.Value == "pool1ds7rtjqxauff54ny4avs7qxmvchepuv6u79kgg0sf84mjlhxznp")
                    {
                        continue;
                    }

                    url = $"https://api.koios.rest/api/v1/pool_delegators_history?_pool_bech32={entry.Value}&_epoch_no={i}";



                    var data = await Functions.FetchDelegatorHistory(client, url);


                    foreach (var dict in data)
                    {
                        var delegatorData = Functions.InitDelegator(i, delegator.StartEpoch, entry.Key, entry.Value, dict);

                        totalAda += Helper.ConvertLovelaceStringToADA(dict["amount"]);


                        var exist = result.Any(r => r["stake_address"] == delegatorData["stake_address"] && r["epoch"] == delegatorData["epoch"]);

                        if (!exist)
                        {
                            result.Add(delegatorData);
                        }


                    }

                }

            }
            Helper.CreateCsv(result, delegator.MainPoolStartEpoch, delegator.EndEpoch, "Coinecta");
            return Results.Ok(new { result, totalAda });

        })
        .WithName("FetchDelegatorHistory")
        .WithOpenApi();

        app.MapPost("/calculate", async (
            [FromBody] RewardParams reward
            ) =>
        {
            Dictionary<string, Dictionary<string, double>> delegatorRewards = new();

            double partnerRate = reward.PartnerRate;

            foreach (var delegator in reward.DelegatorData)
            {
                if (delegatorRewards.ContainsKey(delegator["stake_address"]))
                {

                    delegatorRewards = Functions.UpdateRewards(delegator, delegatorRewards, reward.MainRate, partnerRate);

                }
                else
                {
                    delegatorRewards[delegator["stake_address"]] = Functions.InitRewards(delegator, reward.MainRate, partnerRate);

                }


            }

            return Results.Ok(Helper.SortByTotalRewards(delegatorRewards));

        })
        .WithName("calculate")
        .WithOpenApi();

        app.MapPost("/fetch-assets", async (
            [FromBody] Asset assetList,
            [FromServices] IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient();
            List<Dictionary<string, string>> oneOfOneAssetHoldings = new();

            string url = "";

            string policyId = assetList.PolicyId;

            foreach (var asset in assetList.AssetNames)
            {
                List<Dictionary<string, string>> res = new();

                string assetName = asset;
                url = $"https://api.koios.rest/api/v1/asset_addresses?_asset_policy={policyId}&_asset_name={asset}";
                res = await Functions.FetchAssetHoldings(client, url);

                oneOfOneAssetHoldings.AddRange(res);
            }

            url = $"https://api.koios.rest/api/v1/policy_asset_addresses?_asset_policy={policyId}";
            var boomerAssetHoldings = await Functions.FetchAssetHoldings(client, url);

            var holdings = new AssetHoldingsResponse
            {
                OneOfOneAssetHoldings = oneOfOneAssetHoldings,
                BoomerAssetHoldings = boomerAssetHoldings
            };

            return Results.Ok(new
            {
                OneOfOneAssetHoldings = oneOfOneAssetHoldings,
                BoomerAssetHoldings = boomerAssetHoldings
            });

        })
        .WithName("fetch-assets")
        .WithOpenApi();

        app.MapPost("/add-bonus", async ([FromBody] BonusParameters param) =>
        {
            foreach (var delegatorReward in param.Rewards)
            {

                delegatorReward.Value["total_rewards"] = Functions.AddBonuses(delegatorReward.Value, param.AssetHoldings.OneOfOneAssetHoldings, param.AssetHoldings.BoomerAssetHoldings, delegatorReward.Key);

            }

            return param.Rewards;

        })
        .WithName("add-bonus")
        .WithOpenApi();
    }
}