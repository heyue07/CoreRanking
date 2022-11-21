namespace CoreRankingDomain.Interfaces;

public interface IServerRepository
{
    Task<int> GetWorldTag(int roleId);
    Task SendMessage(BroadcastChannel channel, string message, int roleId = 0);
    Task SendPrivateMessage(int roleId, string message);
    Task<GRoleData> GetRoleByID(int roleId);
    Task<GRoleData> GetRoleByName(string characterName);
    Task<int> GetRoleIdByName(string characterName);
    Task<List<GRoleData>> GetRolesFromAccount(int userId);
    Task<bool> SendMail(int roleId, string title, string message, GRoleInventory item, int coinCount);
    Task<List<int>> GetGmID();
    string GetLogsPath();
    PwVersion GetPwVersion();
    List<BroadcastChannel> GetChannelsAllowed();
    Task<string> GetRoleNameByID(int roleId);
    Task DeliverReward(ItemAward item, int amount, int roleId, string title, string message);
    Task<bool> GiveCash(int accountId);
    Task<bool> GiveCash(int accountId, int cashAmount);
    BroadcastChannel GetMainChannel();
    Task<List<int>> GetOnlineAccountId();
    GRoleInventory GenerateItem(int itemId, int itemCount);
    Task<int> GetUserIDByRoleID(int roleId);
}