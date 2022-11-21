namespace CoreRankingDomain.Interface;

public interface IPrizeRepository
{
    Task ResetRanking();
    Task<DeliverRewardResponse> DeliverTopRankingReward();
    Task Add(Prize prize);
    Task<int> SaveChangesAsync();
    Task<bool> IsFirstRun();
    Task<DateTime> GetLastRewardDate();
}