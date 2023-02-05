namespace CoreRankingInfra.Model.Table;

public record Prize
{
    [Key]
    public int Id { get; set; }
    public DateTime PrizeDeliveryDate { get; set; }
    public EPrizeFrequencyOption PrizeOption { get; set; }
    public EPrizeReward PrizeRewardType { get; set; }
    public EWinCriteria WinCriteria { get; set; }
    public int DeliveryCount { get; set; }
    public string DeliveryRoleIdListAsJson { get; set; }
    public int? CashCount { get; set; }
    public int? ItemRewardId { get; set; }
    public int? ItemRewardCount { get; set; }
}