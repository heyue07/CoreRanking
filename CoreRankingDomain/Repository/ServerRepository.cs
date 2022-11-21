namespace CoreRankingDomain.Repository;

public class ServerRepository : IServerRepository
{
    private readonly ServerConnection _server;
    private readonly RankingDefinitions _definitions;
    private readonly LogWriter _logWriter;
    public ServerRepository(ServerConnection server, LogWriter logWriter, RankingDefinitions definitions)
    {
        this._server = server;
        this._definitions = definitions;
        this._logWriter = logWriter;

        PWGlobal.UsedPwVersion = _server.PwVersion;
    }

    public async Task<List<int>> GetOnlineAccountId()
    {
        var onlinePlayers = GMListOnlineUser.Get(_server.gdeliveryd);

        return onlinePlayers?.Select(x => x.UserId)?.ToList();
    }

    public async Task<GRoleData> GetRoleByID(int roleId)
    {
        try
        {
            GRoleData roleData = GetRoleData.Get(_server.gamedbd, roleId);
            return roleData;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task<int> GetUserIDByRoleID(int roleId)
    {
        return GetRoleData.Get(_server.gamedbd, roleId).GRoleBase.UserId;
    }

    public async Task<GRoleData> GetRoleByName(string characterName)
    {
        try
        {
            GRoleData roleData = await GetRoleByID(GetRoleId.Get(_server.gamedbd, characterName));
            return roleData;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    public async Task SendPrivateMessage(int roleId, string message)
    {
        try
        {
            await Task.Run(() => PrivateChat.Send(_server.gdeliveryd, roleId, message));
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }
    }

    public async Task SendMessage(BroadcastChannel channel, string message, int roleId = 0)
    {
        try
        {
            await Task.Run(() =>
            {
                _ = channel.Equals(BroadcastChannel.Private) ?
                    PrivateChat.Send(_server.gdeliveryd, roleId, message)
                    :
                    ChatBroadcast.Send(_server.gprovider, _definitions.Channel, $"{(_definitions.Channel.Equals(BroadcastChannel.System) ? _definitions.MessageColor : default)}{message}");
            });
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }
    }

    public async Task<List<GRoleData>> GetRolesFromAccount(int userId)
    {
        try
        {
            List<int> idRoles = GetUserRoles.Get(_server.gamedbd, userId).Select(x => x.Item1).ToList();

            List<GRoleData> roles = new List<GRoleData>();

            idRoles.ForEach(x => roles.Add(GetRoleData.Get(_server.gamedbd, x)));

            return roles;
        }
        catch (Exception e)
        {
            _logWriter.Write($"Erro para userID {userId}: {e}");
        }

        return default;
    }

    public async Task<bool> SendMail(int roleId, string title, string message, GRoleInventory item, int coinCount = 0)
    {
        try
        {
            return await Task.Run(() => SysSendMail.Send(_server.gdeliveryd, roleId, title, message, item, coinCount));
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }

        return false;
    }

    public async Task<List<int>> GetGmID()
    {
        try
        {
            List<int> gmAccountIds = (await File.ReadAllLinesAsync("./Configurations/GMAccounts.conf"))
                .Select(x => int.Parse(x))
                .ToList();

            List<int> gmRoleIds = new List<int>();

            foreach (int currentId in gmAccountIds)
            {
                gmRoleIds.AddRange(GetUserRoles.Get(_server.gamedbd, currentId).Select(x => x.Item1).ToList());
            }

            return gmRoleIds;
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }

        return default;
    }

    public string GetLogsPath()
    {
        return _server.logsPath;
    }

    public PwVersion GetPwVersion()
    {
        return _server.PwVersion;
    }

    public async Task<int> GetRoleIdByName(string characterName)
    {
        try
        {
            return await Task.Run(() => GetRoleId.Get(_server.gamedbd, characterName));
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }

        return -1;
    }

    public async Task<string> GetRoleNameByID(int roleId)
    {
        return GetRoleBase.Get(_server.gamedbd, roleId)?.Name;
    }

    public async Task DeliverReward(ItemAward itemChoosed, int amount, int roleId, string title = "RECOMPENSA DE PVP", string message = "Parabéns pela conquista!")
    {
        try
        {
            GRoleInventory item = new GRoleInventory()
            {
                Id = itemChoosed.Id,
                MaxCount = 99999,
                Pos = GetRolePocket.Get(_server.gamedbd, roleId).Items.Length + 1,
                Proctype = int.Parse(itemChoosed.Proctype),
                Octet = itemChoosed.Octet,
                Mask = int.Parse(itemChoosed.Mask),
            };

            //Estrutura condicional para determinar se o stack do item é maior/igual à quantidade requisitada
            if (itemChoosed.Stack >= amount)
            {
                item.Count = amount;

                await SendMail(roleId, title, message, item);
            }
            else
            {
                item.Count = 1;

                for (int i = 0; i < amount; i++)
                {
                    await SendMail(roleId, title, message, item);
                }
            }
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }
    }

    public List<BroadcastChannel> GetChannelsAllowed()
    {
        return _definitions.ChannelsTriggersAllowed;
    }

    public BroadcastChannel GetMainChannel()
    {
        return _definitions.Channel;
    }

    public async Task<bool> GiveCash(int accountId)
    {
        try
        {
            return await Task.Run(() => DebugAddCash.Add(_server.gamedbd, accountId, _definitions.KillGold * 100));
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }

        return false;
    }
    public async Task<bool> GiveCash(int accountId, int cashAmount)
    {
        try
        {
            return await Task.Run(() => DebugAddCash.Add(_server.gamedbd, accountId, cashAmount * 100));
        }
        catch (Exception e)
        {
            _logWriter.Write(e.ToString());
        }

        return false;
    }

    public GRoleInventory GenerateItem(int itemId, int itemCount)
        => new GRoleInventory
        {
            Id = itemId,
            Count = itemCount,
            MaxCount = itemCount,
            Proctype = 0,
        };

    public async Task<int> GetWorldTag(int roleId)
        => await Task.Run(() => GetRoleStatus.Get(_server.gamedbd, roleId).WorldTag);
    
}