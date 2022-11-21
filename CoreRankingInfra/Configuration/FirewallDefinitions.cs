namespace CoreRankingInfra.Configuration;

public class FirewallDefinitions
{
    public bool Active { get; }
    public BroadcastChannel Channel { get; }
    public int CheckInterval { get; }
    public int KillLimit { get; }
    public int TimeLimit { get; }
    public int BanTime { get; }

    public FirewallDefinitions()
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/Firewall.json"));

        this.Active = jsonNodes["ATIVO"].ToObject<bool>();
        this.Channel = (BroadcastChannel)jsonNodes["CANAL"].ToObject<int>();
        this.CheckInterval = jsonNodes["INTERVALO DE CHECAGEM"].ToObject<int>();
        this.KillLimit = jsonNodes["LIMITE DE KILL"].ToObject<int>();
        this.TimeLimit = jsonNodes["LIMITE DE TEMPO"].ToObject<int>();
        this.BanTime = jsonNodes["TEMPO DE BAN"].ToObject<int>();
    }
}