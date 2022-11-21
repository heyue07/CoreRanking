namespace CoreRankingInfra.Configuration;

public record PrizeDefinitions
{
    public bool Active { get; private init; }
    public EPrizeFrequencyOption PrizeFrequencyOption { get; private init; }
    public EWinCriteria WinCriteria { get; private init; }
    public EPrizeReward PrizeRewardType { get; private init; }
    public int DeliveryPrizeHour { get; private init; }    
    public int DeliverRewardToFirstAmountPlayers { get; private init; }
    public bool ResetRankingAfterPrizeDelivery { get; private init; }
    public int CashCount { get; private init; }
    public ItemAward ItemAward { get; private init; }

    public PrizeDefinitions()
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/PrizeDefinitions.json"));

        this.Active = jsonNodes["ATIVO"].ToObject<bool>();
        this.PrizeFrequencyOption = jsonNodes["FREQUÊNCIA DE RECOMPENSA"].ToObject<EPrizeFrequencyOption>();
        this.WinCriteria = jsonNodes["CRITÉRIO DE CLASSIFICAÇÃO"].ToObject<EWinCriteria>();
        this.PrizeRewardType = jsonNodes["TIPO DE PREMIAÇÃO"].ToObject<EPrizeReward>();
        this.DeliveryPrizeHour = jsonNodes["HORA PARA ENTREGA DE RECOMPENSA"].ToObject<int>();
        this.DeliverRewardToFirstAmountPlayers = jsonNodes["MÁXIMO DE JOGADORES BENEFICIADOS"].ToObject<int>();
        this.ResetRankingAfterPrizeDelivery = jsonNodes["RESET DE RANKING"].ToObject<bool>();
        this.CashCount = jsonNodes["QUANTIA DE CASH"].ToObject<int>();
        this.ItemAward = jsonNodes["ITEM PARA RECOMPENSA"].ToObject<ItemAward>();
    }
}