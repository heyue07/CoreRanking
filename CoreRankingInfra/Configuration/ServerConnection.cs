namespace CoreRankingInfra.Configuration;

public record ServerConnection
{
    public string logsPath { get; private init; }
    public PwVersion PwVersion { get; private init; }
    public Gamedbd gamedbd { get; private init; }
    public GProvider gprovider { get; private init; }
    public GDeliveryd gdeliveryd { get; private init; }

    public ServerConnection()
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/ServerConnection.json"));

        this.logsPath = jsonNodes["LOGS_PATH"].ToObject<string>(); ;
        this.PwVersion = (PwVersion)jsonNodes["PW_VERSION"].ToObject<int>();
        gamedbd = new Gamedbd(jsonNodes["GAMEDBD"]["HOST"].ToObject<string>(), jsonNodes["GAMEDBD"]["PORT"].ToObject<int>());
        gprovider = new GProvider(jsonNodes["GPROVIDER"]["HOST"].ToObject<string>(), jsonNodes["GPROVIDER"]["PORT"].ToObject<int>());
        gdeliveryd = new GDeliveryd(jsonNodes["GDELIVERYD"]["HOST"].ToObject<string>(), jsonNodes["GDELIVERYD"]["PORT"].ToObject<int>());
    }
}