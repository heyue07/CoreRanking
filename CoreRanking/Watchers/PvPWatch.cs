namespace CoreRanking.Watchers;

public class PvPWatch : BackgroundService
{
    private long lastSize;

    private readonly string path;

    private readonly RankingDefinitions _definitions;

    private readonly ClassPointConfigFactory _classPointFactory;

    private readonly ILogger<PvPWatch> _logger;

    private readonly IServerRepository _serverContext;

    private readonly IRoleRepository _roleContext;

    private readonly IAccountRepository _accountContext;

    private readonly IBattleRepository _battleContext;

    private readonly IBannedRepository _bannedContext;

    private readonly LogWriter _logWriter;
    public PvPWatch(ILogger<PvPWatch> logger, LogWriter logWriter, IServerRepository serverContext, IRoleRepository roleContext,
        IAccountRepository accountContext, IBattleRepository battleContext, IBannedRepository bannedContext,
        RankingDefinitions definitions, ClassPointConfigFactory classPointFactory)
    {
        this._logger = logger;

        this._logWriter = logWriter;

        this._definitions = definitions;

        this._serverContext = serverContext;

        this._roleContext = roleContext;

        this._accountContext = accountContext;

        this._battleContext = battleContext;

        this._bannedContext = bannedContext;

        this._classPointFactory = classPointFactory;

        PWGlobal.UsedPwVersion = _serverContext.GetPwVersion();

        this.path = Path.Combine(_serverContext.GetLogsPath(), ELogFile.Formatlog.GetDescription());

        lastSize = GetFileSize(path);
    }
    protected override async Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
    {
        _logger.LogInformation("MÓDULO DE PVP INICIADO");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                long fileSize = GetFileSize(path);

                if (fileSize > lastSize)
                {
                    var battles = await ReadTail(path, UpdateLastFileSize(fileSize));

                    foreach (var battle in battles)
                    {
                        _logWriter.Write($"At {battle.Date}, player {battle.KillerId} killed {battle.KilledId}");

                        Tuple<Role, Role, int> battleResult = await UploadPvPEvent(battle);

                        if (battleResult != null)
                            await MultipleKillWatch.Trigger(battleResult.Item2);

                        //Se o resultado da batalha não for nulo e mensagem estiver permitida, envia mensagem no canal especificado.
                        if (_definitions.isMessageAllowed && battleResult != null)
                        {
                            await _serverContext.SendMessage(_definitions.Channel, await BuildMessage(battleResult.Item2.CharacterName, battleResult.Item1.CharacterName, battleResult.Item3), battleResult.Item2.RoleId);
                        }
                    }
                }

                await Task.Delay(250);
            }
            catch (Exception ex)
            {
                _logWriter.Write(ex.ToString());
            }
        }
    }
    public override Task StartAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
    public async Task<List<Battle>> ReadTail(string filename, long offset)
    {
        try
        {
            byte[] bytes;

            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Seek(offset * -1, SeekOrigin.End);

                bytes = new byte[offset];
                fs.Read(bytes, 0, (int)offset);
            }

            List<string> logs = Encoding.Default.GetString(bytes).Split(new string[] { "\n" }[0]).ToList();

            List<Battle> battlesResponse = new List<Battle>();

            foreach (var log in logs)
            {
                battlesResponse.Add(await GatherPvPData(log));
            }

            return battlesResponse.Where(x => x != default).ToList();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    public async Task<Tuple<Role, Role, int>> UploadPvPEvent(Battle battleData)
    {
        try
        {
            if (battleData is null)
                return default;

            if (await CheckMaps(battleData))
                return default;

            if (await CheckBan(battleData))
                return default;

            if (await CheckIP(battleData))
                return default;

            if (await CheckLevelRange(battleData))
                return default;

            if (await CheckPointLimit(battleData))
                return default;

            return await LastChecks(battleData);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }

    private async Task<bool> CheckMaps(Battle battleData)
    {
        try
        {
            //Caso não tenha nenhum mapa definido, pula a verificação
            if (!_definitions.WorldTagsAvailable.Any())
                return false;

            var killerIsOnAllowedMap = _definitions.WorldTagsAvailable.Contains(await _serverContext.GetWorldTag(battleData.KillerId));
            var killedIsOnAllowedMap = _definitions.WorldTagsAvailable.Contains(await _serverContext.GetWorldTag(battleData.KilledId));

            if (!killerIsOnAllowedMap | !killedIsOnAllowedMap)
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, "Você não ganhou pontos no ranking porque o mapa atual não é válido.");
                await _serverContext.SendPrivateMessage(battleData.KilledId, "Você não perdeu pontos no ranking porque o mapa atual não é válido.");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return false;
    }

    private async Task<Tuple<Role, Role, int>> LastChecks(Battle battleData)
    {
        try
        {
            Role KillerRole = await _roleContext.GetRoleFromId(battleData.KillerId);

            Role KilledRole = await _roleContext.GetRoleFromId(battleData.KilledId);

            if (KilledRole.Points > _definitions.MinimumPoints)
            {
                KillerRole.Points += _classPointFactory.Get().Where(x => x.Ocuppation.Equals(KillerRole.CharacterClass.ConvertClassFromGameStructure())).Select(x => x.onKill).FirstOrDefault();
                KillerRole.Kill += 1;
            }
            else
            {
                await _serverContext.SendPrivateMessage(battleData.KilledId, "Você não contará pontos no PvP porque atingiu o limite mínimo de pontos para ser válido no PvP. PS: você ainda perderá pontos ao morrer.");
                await _serverContext.SendPrivateMessage(battleData.KillerId, $"Você não ganhará pontos no PvP ao matar {KilledRole.CharacterName} porque o(a) jogador(a) atingiu o limite mínimo de pontos para ser válido no PvP.");

                _logWriter.Write($"Personagem {KilledRole.CharacterName} atingiu o limite mínimo de pontos ({_definitions.MinimumPoints} e por isso não perdeu mais pontos por ter morrido.");
            }

            KilledRole.Death += 1;

            KilledRole.Points -= _classPointFactory.Get().Where(x => x.Ocuppation.Equals(KilledRole.CharacterClass.ConvertClassFromGameStructure())).Select(x => x.onDeath).FirstOrDefault();

            await _roleContext.SaveChangesAsync();

            if (_definitions.KillGold > 0)
                await _serverContext.GiveCash(KillerRole.AccountId);

            await _battleContext.AddByModel(battleData);

            await _battleContext.SaveChangesAsync();

            return new Tuple<Role, Role, int>(KilledRole, KillerRole, KillerRole.Kill);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    /// <summary>
    /// Checa se o personagem já alcançou o limite mínimo de pontos pré-estipulado para que se pare de perder pontos.
    /// </summary>
    /// <param name="battleData"></param>
    /// <returns></returns>
    private async Task<bool> CheckPointLimit(Battle battleData)
    {
        try
        {
            if (battleData.KilledRole.Points <= _definitions.PointDifference)
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, $"Você não ganhará pontos por matar {battleData.KilledRole.CharacterName} porque a pontuação dele no ranking está abaixo que a necessária.");

                await _serverContext.SendPrivateMessage(battleData.KilledId, $"Você não perdeu pontos por ser morto por {battleData.KillerRole.CharacterName} porque a sua pontuação está abaixo que a necessária.");

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return true;
    }
    /// <summary>
    /// Checa se há conflito de IP, retornando true caso haja e false caso não haja
    /// </summary>
    /// <param name="battleData"></param>
    /// <returns></returns>
    private async Task<bool> CheckIP(Battle battleData)
    {
        try
        {
            string killerIp = await _accountContext.GetIpByAccountID(battleData.KillerRole.AccountId);
            string killedIp = await _accountContext.GetIpByAccountID(battleData.KilledRole.AccountId);

            bool someoneHasntIp = false;
            bool someoneHasZeroIP = false;
            bool sameIp = false;

            if (killedIp is null)
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, $"Você não ganhará pontos por matar {battleData.KilledRole.CharacterName} até que ele(a) relogue a conta para entrar no Ranking.");
                someoneHasntIp = true;
            }

            if (killerIp is null)
            {
                await _serverContext.SendPrivateMessage(battleData.KilledId, $"Você não está participando do Ranking até que relogue sua conta.");
                someoneHasntIp = true;
            }

            //Verifica se o sistema não tem registro de IP do personagem que morreu
            if (killedIp.Equals("0"))
            {
                await _serverContext.SendPrivateMessage(battleData.KilledId, "Relogue sua conta para participar do Ranking e ganhar pontos.");
                someoneHasZeroIP = true;
            }

            //Verifica se o sistema não tem registro de IP do personagem que matou
            if (killerIp.Equals("0"))
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, "Relogue sua conta para participar do Ranking e ganhar pontos.");
                someoneHasZeroIP = true;
            }

            //Verifica se o personagem que matou tem o mesmo IP que o personagem que morreu
            if (killedIp.Equals(killerIp))
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, "Matar personagens que estão na mesma rede de internet que você não contabiliza pontos.");

                _logWriter.Write($"{battleData.KillerRole.CharacterName}: Matar personagens que estão na mesma rede de internet que você não contabiliza pontos.");

                sameIp = true;
            }


            return someoneHasntIp | someoneHasZeroIP | sameIp;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return true;
    }
    /// <summary>
    /// Checa se a diferença de nível entre os dois envolvidos no PVP está dentro do intervalo definido, retornando true caso haja incoerência, false caso não haja.
    /// </summary>
    /// <param name="battleData"></param>
    /// <returns></returns>
    private async Task<bool> CheckLevelRange(Battle battleData)
    {
        try
        {
            if ((battleData.KillerRole.Level - _definitions.LevelDifference) >= battleData.KilledRole.Level && battleData.KilledRole.Level <= (battleData.KillerRole.Level + _definitions.LevelDifference))
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, $"Você não ganhará pontos por matar {battleData.KilledRole.CharacterName} porque a diferença de nível entre vocês não está no intervalo permitido para contabilizar pontos.");

                await _serverContext.SendPrivateMessage(battleData.KilledId, $"Você não perdeu pontos por ser morto por {battleData.KillerRole.CharacterName} porque a diferença de nível entre vocês não está no intervalo permitido para contabilizar pontos.");

                _logWriter.Write($"{battleData.KillerRole.CharacterName} tentou matar {battleData.KilledRole.CharacterName}, que é {battleData.KillerRole.Level - battleData.KilledRole.Level} níveis menor");

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return true;
    }

    /// <summary>
    /// Retorna true caso haja restrição, false caso não haja restrição
    /// </summary>
    /// <param name="battleData"></param>
    /// <returns></returns>
    private async Task<bool> CheckBan(Battle battleData)
    {
        try
        {
            var isBanned = await _bannedContext.PlayerCurrentlyBanned(battleData.KillerId);

            if (isBanned)
            {
                await _serverContext.SendPrivateMessage(battleData.KillerId, $"Você não ganhará pontos nem kills por um tempo determinado por ter sido punido por FreeKill.");

                await _serverContext.SendPrivateMessage(battleData.KilledId, $"Você não perdeu pontos nem kills por ter sido morto por {battleData?.KillerRole?.CharacterName} devido a uma restrição do mesmo por FreeKill.");

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return true;
    }
    private async Task<Battle> EnsureRoleCreation(int killerId, int killedId, DateTime when)
    {
        try
        {
            Role killedChar = (await _roleContext.GetRoleFromId(killedId)) ?? await _roleContext.AddByID(killedId);
            Role killerChar = (await _roleContext.GetRoleFromId(killerId)) ?? await _roleContext.AddByID(killerId);

            DateTime date = when;

            if (killedChar is null)
            {
                _logWriter.Write($"EnsureRoleCreation: erro ao criar Role ID {killedId}");
                return default;
            }

            if (killerChar is null)
            {
                _logWriter.Write($"EnsureRoleCreation: erro ao criar Role ID {killerId}");
                return default;
            }

            return new Battle
            {
                KilledId = killedChar.RoleId,
                KillerId = killerChar.RoleId,
                KilledRole = killedChar,
                KillerRole = killerChar,
                Date = date
            };
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private async Task<Battle> GatherPvPData(string log)
    {
        if (log.Contains("die:roleid") && !log.Contains("attacker=-"))
        {
            DateTime when = Convert.ToDateTime(System.Text.RegularExpressions.Regex.Match(log, @"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})").Value);
            int _killerId = int.Parse(System.Text.RegularExpressions.Regex.Match(log, @"attacker=([0-9]*)").Value.Replace("attacker=", ""));
            int _killedId = int.Parse(System.Text.RegularExpressions.Regex.Match(log, @"roleid=([0-9]*)").Value.Replace("roleid=", ""));

            Battle battleData = await EnsureRoleCreation(_killerId, _killedId, when);

            return battleData;
        }

        return default;
    }
    private async Task<string> BuildMessage(string killer, string dead, int kills)
    {
        try
        {
            string message = _definitions.Channel is BroadcastChannel.System ?
            _definitions.Messages.ElementAt(
            new Random().Next(_definitions.Messages.Count)).
            Replace("$killer", $"&{killer}&").
            Replace("$dead", $"&{dead}&") + (_definitions.ShowKDA ? $". Kills: {kills}. KDA: {(await _roleContext.GetKDA(killer)).ToString("0.00")}" : default)
            :
            _definitions.Messages.ElementAt(
            new Random().Next(_definitions.Messages.Count)).
            Replace("$killer", $"{killer}").
            Replace("$dead", $"{dead}") + (_definitions.ShowKDA ? $". Kills: {kills}. KDA: {(await _roleContext.GetKDA(killer)).ToString("0.00")}" : default);

            return message;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private long UpdateLastFileSize(long fileSize)
    {
        long difference = fileSize - lastSize;
        lastSize = fileSize;

        return difference;
    }
    public long GetFileSize(string fileName)
    {
        return new FileInfo(fileName).Length;
    }
}