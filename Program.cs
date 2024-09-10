using Newtonsoft.Json.Linq;
using CrashrISPO.Helper;


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


app.MapGet("/Koios/{startEpoch:int}/{endEpoch:int}/{mainRate:double}/{spoFixedAmount:double}/{generateCsv:bool}", async (IHttpClientFactory httpClientFactory, int startEpoch, int endEpoch, double mainRate, double spoFixedAmount, bool generateCsv) =>
{

    var apiKey = Environment.GetEnvironmentVariable("API_KEY");
    List<Dictionary<string, string>> result = new();
    List<string> nonCrashAddresses = new();

    double crashPoolTotal = 0;

    Dictionary<string, string> poolIds = new()
    {
        {"HAPPY", "pool1a8n7f97dmgtgrnl53exccknjdchqxrr9508hlxlgqp6xvjmzhej"},
        {"KTOP", "pool135h773klt7djljmyawkh88e8qc457wxqfhc6j9h6y9n4y3sgh7t"},
        {"ELEMENTAL","pool19ut4284xy9p82dd0cglzxweddfqw73yennkjk6mmp650chnr6lz"},
        {"RUMOR","pool1c30lqt59t8sjn5lg04r5wk5eucxa5h9cj05xs9gzc8lngl4cmta"},
        {"CRASH", "pool1j8zhlvakd29yup5xmxtyhrmeh24udqrgkwdp99d9tx356wpjarn"},
    };

    for (int i = startEpoch; i <= endEpoch; i++)
    {
        foreach (var entry in poolIds)
        {
            var client = httpClientFactory.CreateClient();
            List<Dictionary<string, string>> delegatorInfo = new();


            string url = $"https://api.koios.rest/api/v1/pool_delegators_history?_pool_bech32={entry.Value}&_epoch_no={i}";
            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var json = JArray.Parse(content);

            var data = json.ToObject<List<Dictionary<string, string>>>();

            foreach (var dict in data)
            {
                Dictionary<string, string> delegator = new()
                    {
                        {"PoolName", entry.Key},
                        {"PoolId",entry.Value},
                    };

                delegator["Epoch"] = dict["epoch_no"];
                delegator["stake_address"] = dict["stake_address"];
                delegator["amount"] = dict["amount"];

                if (entry.Key != "CRASH")
                {
                    nonCrashAddresses.Add(delegator["stake_address"]);
                }

                result.Add(delegator);
            }


        }

    }

    Dictionary<string, Dictionary<string, double>> uniqueAdresses = new();
    double spoIndividualReward = spoFixedAmount / nonCrashAddresses.Count;

    foreach (var dict in result)
    {

        var stakeAddress = dict["stake_address"].Trim();
        var amount = Convert.ToDouble(dict["amount"]) / 1000000;

        if (uniqueAdresses.ContainsKey(dict["stake_address"]))
        {
            uniqueAdresses[dict["stake_address"]]["totalADAStaked"] += amount;
            if (dict["PoolName"] == "CRASH")
            {
                uniqueAdresses[dict["stake_address"]]["totalRewards"] += amount * mainRate;
            }
            else
            {
                uniqueAdresses[dict["stake_address"]]["totalRewards"] += spoIndividualReward;
            }
        }
        else
        {
            uniqueAdresses[dict["stake_address"]] = new Dictionary<string, double>();

            uniqueAdresses[dict["stake_address"]]["totalADAStaked"] = amount;
            if (dict["PoolName"] == "CRASH")
            {
                uniqueAdresses[dict["stake_address"]]["totalRewards"] = amount * mainRate;
            }
            else
            {
                uniqueAdresses[dict["stake_address"]]["totalRewards"] = spoIndividualReward;
            }
        }
    }
    if (generateCsv)
    {
        Helper.CreateCsv(result, startEpoch, endEpoch, "Koios");
    }

    return Results.Json(Helper.SortByTotalRewards(uniqueAdresses));

})
.WithName("GetDataFromKoios")
.WithOpenApi();



app.Run();

