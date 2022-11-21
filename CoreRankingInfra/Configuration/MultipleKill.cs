namespace CoreRankingInfra.Configuration;

public class MultipleKill
{
    public bool IsActive { get; set; }
    public bool IsMessageAllowed { get; set; }
    public BroadcastChannel Channel { get; set; }
    public DoubleKill DoubleKill { get; set; }
    public TripleKill TripleKill { get; set; }
    public QuadraKill QuadraKill { get; set; }
    public PentaKill PentaKill { get; set; }

    public MultipleKill()
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/MultipleKill.json"));

        List<List<string>> messages = new List<List<string>>();

        foreach (var multiplicador in new string[] { "DOUBLEKILL", "TRIPLEKILL", "QUADRAKILL", "PENTAKILL" })
        {
            List<string> messagePerMultiplier = new List<string>();

            foreach (var package in jsonNodes[multiplicador]["MENSAGENS"].Children())
            {
                messagePerMultiplier.Add(package.First.ToString());
            }

            messages.Add(messagePerMultiplier);
        }

        this.IsActive = jsonNodes["ATIVO"].ToObject<bool>();

        this.IsMessageAllowed = jsonNodes["MENSAGEM INGAME"].ToObject<bool>();

        this.Channel = (BroadcastChannel)jsonNodes["CANAL"].ToObject<int>();

        this.DoubleKill = new DoubleKill
        {
            Time = jsonNodes["DOUBLEKILL"]["TEMPO"].ToObject<double>(),
            Points = jsonNodes["DOUBLEKILL"]["PONTOS"].ToObject<int>(),
            Messages = messages[0]
        };

        this.TripleKill = new TripleKill
        {
            Time = jsonNodes["TRIPLEKILL"]["TEMPO"].ToObject<double>(),
            Points = jsonNodes["TRIPLEKILL"]["PONTOS"].ToObject<int>(),
            Messages = messages[1]
        };

        this.QuadraKill = new QuadraKill
        {
            Time = jsonNodes["QUADRAKILL"]["TEMPO"].ToObject<double>(),
            Points = jsonNodes["QUADRAKILL"]["PONTOS"].ToObject<int>(),
            Messages = messages[2]
        };

        this.PentaKill = new PentaKill
        {
            Time = jsonNodes["PENTAKILL"]["TEMPO"].ToObject<double>(),
            Points = jsonNodes["PENTAKILL"]["PONTOS"].ToObject<int>(),
            Messages = messages[3]
        };
    }
}
