namespace CoreRankingInfra.Configuration;

public class RankingDefinitions
{
    public bool hasReborn { get; }
    public bool isMessageAllowed { get; }
    public bool isTriggerAllowed { get; }
    public bool isTrasferenceAllowed { get; }
    public bool isOnlyPveAllowed { get; }
    public BroadcastChannel Channel { get; }
    public List<string> Messages { get; }
    public List<ItemAward> ItemsReward { get; set; }
    public int KillGold { get; }
    public int LevelDifference { get; }
    public int PointDifference { get; }
    public int MinimumPoints { get; }
    public bool ShowKDA { get; }
    public int AmountPlayersOnPodium { get; }
    public string MessageColor { get; }
    public int QuestIdResetKDA { get; }
    public List<BroadcastChannel> ChannelsTriggersAllowed { get; }
    public bool UpdateLevelProcedure { get; }
    public List<int> WorldTagsAvailable { get; }

    public RankingDefinitions(ItemAwardFactory itemFactory)
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/RankingDefinitions.json"));

        this.ItemsReward = itemFactory.Get();
        this.Messages = jsonNodes["MENSAGENS"].ToObject<List<string>>();
        this.isTrasferenceAllowed = jsonNodes["TRANSFERÊNCIA DE PONTOS"].ToObject<bool>();
        this.KillGold = jsonNodes["GOLD POR KILL"].ToObject<int>();
        this.isMessageAllowed = jsonNodes["AVISO DE MENSAGEM"].ToObject<bool>();
        this.Channel = (BroadcastChannel)jsonNodes["CANAL"].ToObject<int>();
        this.LevelDifference = jsonNodes["TOLERANCIA DE LEVEL"].ToObject<int>();
        this.PointDifference = jsonNodes["TOLERANCIA DE PONTO"].ToObject<int>();
        this.ShowKDA = jsonNodes["MOSTRAR KDA"].ToObject<bool>();
        this.isTriggerAllowed = jsonNodes["ATIVAR TRIGGERS"].ToObject<bool>();
        this.MinimumPoints = jsonNodes["LIMITE MINIMO DE PONTOS"].ToObject<int>();
        this.AmountPlayersOnPodium = jsonNodes["QUANTIDADE DE JOGADORES NO TOPRANK"].ToObject<int>();
        this.MessageColor = jsonNodes["COR DA MENSAGEM"].ToObject<string>();
        this.QuestIdResetKDA = jsonNodes["ID DA MISSAO QUE RESETA KDA"].ToObject<int>();
        this.ChannelsTriggersAllowed = GetChannels(jsonNodes);
        this.UpdateLevelProcedure = jsonNodes["ATUALIZAR NIVEL DE PERSONAGENS AO INICIAR"].ToObject<bool>();
        this.WorldTagsAvailable = jsonNodes["WORLDTAG DE MAPAS PERMITIDOS"].ToObject<List<int>>();
        this.hasReborn = jsonNodes["SERVIDOR COM REBORN"].ToObject<bool>();
        this.isOnlyPveAllowed = jsonNodes["MODO PVE"].ToObject<bool>();
    }
    private List<BroadcastChannel> GetChannels(JObject jsonNodes)
    {
        List<BroadcastChannel> channels = new List<BroadcastChannel>();

        foreach (var package in jsonNodes["TRIGGER CHANNELS"].Children())
        {
            channels.Add((BroadcastChannel)int.Parse(package.First.ToString()));
        }

        return channels;
    }
}
