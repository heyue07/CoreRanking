namespace CoreRankingInfra.Model;

public record DeliverRewardResponse
{
    public List<int> PlayersIdRewarded { get; private init; }
    public dynamic Reward { get; private init; }

    public DeliverRewardResponse(List<int> playersIdRewarded, object reward)
    {
        this.PlayersIdRewarded = playersIdRewarded;
        this.Reward = reward;
    }
}