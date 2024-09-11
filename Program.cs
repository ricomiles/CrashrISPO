
using CrashrISPO.Helper;
using CrashrISPO.Functions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapGet("/Koios/{mainPoolStartEpoch}/{startEpoch:int}/{endEpoch:int}/{mainRate:double}/{spoFixedAmount:double}/{generateCsv:bool}", async (IHttpClientFactory httpClientFactory, int mainPoolStartEpoch, int startEpoch, int endEpoch, double mainRate, double spoFixedAmount, bool generateCsv) =>
{

    List<Dictionary<string, string>> result = new();
    double totalAda = 0;

    Dictionary<string, string> poolIds = new()
    {
        {"HAPPY", "pool1a8n7f97dmgtgrnl53exccknjdchqxrr9508hlxlgqp6xvjmzhej"},
        {"KTOP", "pool135h773klt7djljmyawkh88e8qc457wxqfhc6j9h6y9n4y3sgh7t"},
        {"ELEMENTAL","pool19ut4284xy9p82dd0cglzxweddfqw73yennkjk6mmp650chnr6lz"},
        {"RUMOR","pool1c30lqt59t8sjn5lg04r5wk5eucxa5h9cj05xs9gzc8lngl4cmta"},
        {"CRASH", "pool1j8zhlvakd29yup5xmxtyhrmeh24udqrgkwdp99d9tx356wpjarn"},
    };


    for (int i = mainPoolStartEpoch; i <= endEpoch; i++)
    {
        foreach (var entry in poolIds)
        {
            var client = httpClientFactory.CreateClient();
            List<Dictionary<string, string>> delegatorInfo = new();
            string url = "";

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
                result.Add(delegator);
            }


        }

    }

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

    }
    if (generateCsv)
    {
        Helper.CreateCsv(result, mainPoolStartEpoch, endEpoch, "Koios");
    }
    
    foreach (var delegatorReward in delegatorRewards)
    {
        delegatorReward.Value["total_rewards"] = Functions.AddBonuses(delegatorReward.Value);
    }

    

    return Results.Json(Helper.SortByTotalRewards(delegatorRewards));

})
.WithName("GetDataFromKoios")
.WithOpenApi();


app.Run();

