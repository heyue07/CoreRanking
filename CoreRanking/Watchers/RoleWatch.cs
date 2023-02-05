namespace CoreRanking.Watchers;

public class RoleWatch : BackgroundService
{
    private long lastSize;

    private readonly string path;

    private readonly ILogger<RoleWatch> _logger;

    private readonly RankingDefinitions _definitions;

    private readonly IServerRepository _serverContext;

    private readonly IRoleRepository _roleContext;

    private readonly IAccountRepository _accountContext;

    private readonly IBattleRepository _battleContext;

    private readonly LogWriter _logWriter;

    public RoleWatch(ILogger<RoleWatch> logger, LogWriter logWriter, IServerRepository serverContext, IRoleRepository roleContext,
        IAccountRepository accountContext, IBattleRepository battleContext, RankingDefinitions definitions)
    {
        this._logger = logger;

        this._logWriter = logWriter;

        this._definitions = definitions;

        this._serverContext = serverContext;

        this._roleContext = roleContext;

        this._accountContext = accountContext;

        this._battleContext = battleContext;

        PWGlobal.UsedPwVersion = _serverContext.GetPwVersion();

        this.path = Path.Combine(_serverContext.GetLogsPath(), ELogFile.Formatlog.GetDescription());

        lastSize = LoadLastSize("./Configurations/Internal/lastlog.size");
    }

    private async Task FillRankingTable()
    {
        try
        {
            var accounts = await GameDatabase.GetAllAccountsId();

            if (accounts is null) return;

            _logger.LogCritical($"CRIANDO REGISTRO PARA {accounts.Count} CONTAS");

            foreach (var accountId in accounts)
            {
                try
                {
                    if (await _accountContext.Add(new() { Id = accountId }) != default)
                        await _accountContext.SaveChangesAsync();

                    await _roleContext.AddByAccountID(accountId);
                }
                catch (Exception ex)
                {
                    _logWriter.Write($"Erro ao criar registro do personagem {accountId}\n" + ex.ToString());
                }
            }

            await _accountContext.SaveChangesAsync();

            _logger.LogCritical($"{await _roleContext.SaveChangesAsync()} PERSONAGENS INSERIDOS COM SUCESSO");
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    private async Task UpdateRoleLevel()
    {
        try
        {
            _logger.LogCritical("PROCEDIMENTO DE ATUALIZAÇÃO DE NÍVEL DE PERSONAGENS INICIADO. PARA DESATIVAR, DEFINA false EM RankingDefinitions.json");

            var roles = await _roleContext.GetAll();

            if (roles is null) return;

            _logger.LogCritical($"{roles.Count} PERSONAGENS PARA PROCESSAR");

            foreach (var role in roles)
            {
                var serverRole = await _serverContext.GetRoleByID(role.RoleId);

                if (serverRole is null)
                {
                    _logger.LogCritical($"O PERSONAGEM {role.CharacterName} NÃO EXISTE NA DATABASE DO GAME. PROVAVELMENTE FOI EXCLUÍDO.");
                    continue;
                }

                if (role.Level < serverRole.GRoleStatus.Level)
                {
                    _logger.LogCritical($"O PERSONAGEM {role.CharacterName} FOI ATUALIZADO DO NÍVEL {role.Level} PARA O NÍVEL {serverRole.GRoleStatus.Level}");
                    role.Level = serverRole.GRoleStatus.Level;
                }
            }

            var recordsUpdated = await _roleContext.SaveChangesAsync();

            _logger.LogCritical($"{recordsUpdated} PERSONAGENS ATUALIZADOS");
        }
        catch (Exception e)
        {
            LogWriter.StaticWrite(e.ToString());
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MÓDULO DE PERSONAGENS INICIADO");

        //if (!await _roleContext.ExistAnyRecord()) await FillRankingTable(); //Irá inserir todos os personagens no ranking caso não haja ninguém na tabela Role

        if (_definitions.UpdateLevelProcedure) await UpdateRoleLevel(); //Procedimento para atualizar nível de todos os personagens caso esteja configurado

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                long fileSize = await GetFileSize(path);

                if (fileSize > lastSize)
                {
                    await UploadRoleEvent(await ReadTail(path, UpdateLastFileSize(fileSize)));
                }

                this.lastSize = lastSize > fileSize ? fileSize : lastSize;

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                _logWriter.Write(ex.ToString());
            }
        }
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
    private async Task<List<string>> ReadTail(string filename, long offset)
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

            List<string> logs = Encoding.Default
                .GetString(bytes)
                .Split(new string[] { "\n" }[0])
                .Where(x => !x.Contains('\r') & !string.IsNullOrEmpty(x))
                .ToList();

            return logs;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private async Task UploadRoleEvent(List<string> logs)
    {
        try
        {
            foreach (var log in logs)
            {
                //Retorna o ID do personagem na variável roleId, caso haja na log
                bool parsed = int.TryParse(Regex.Match(log, @"roleid=([0-9]*)").Value.Replace("roleid=", ""), out int roleId);

                Task forwarding = log switch
                {
                    string currentLog when currentLog.Contains("login:account") => AccountLogin(log),
                    string currentLog when currentLog.Contains("deleterole") && parsed => DeleteRole(roleId),
                    string currentLog when currentLog.Contains("createrole-success") && parsed => CreateRole(roleId),
                    string currentLog when currentLog.Contains("dbplayerrename:") => RenameRole(roleId),
                    string currentLog when currentLog.Contains("formatlog:upgrade:roleid") && parsed => LevelUpRole(roleId, int.Parse(System.Text.RegularExpressions.Regex.Match(log, @"level=([0-9]*)").Value.Replace("level=", ""))),
                    string currentLog when currentLog.Contains($"taskid={_definitions.QuestIdResetKDA}:type=1:msg=DeliverByAwardData: success = 1") => ResetKDA(log),
                    string currentLog when currentLog.Contains($"player reincarnation:") => PlayerReincarnation(log),
                    _ => Task.Run(() => { })
                };

                await forwarding;
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task PlayerReincarnation(string log)
    {
        try
        {
            int roleId = int.Parse(Regex.Match(log, @"roleid=([0-9]*)").Value.Replace("roleid=", ""));
            int timesReincarnated = int.Parse(Regex.Match(log, @"times=([0-9]*)").Value.Replace("times=", "").Trim());

            await _roleContext.IncrementRebornCount(roleId);
            await _roleContext.SaveChangesAsync();

            _logWriter.Write($"Jogador {roleId} rebornou a {timesReincarnated}ª vez");
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    public async Task AccountLogin(string log)
    {
        try
        {
            //Sentença do log retirando os pontos do IP
            string _log = log.Replace(".", default);

            string ip = Regex.Match(_log, @"peer=([0-9]*)").Value.Replace("peer=", "").Trim();
            string login = Regex.Match(_log, @"account=[a-zA-Z0-9]*").Value.Replace("account=", "").Trim();
            int userId = int.Parse(Regex.Match(_log, @"userid=([0-9]*)").Value.Replace("userid=", ""));

            Account account = await _accountContext.GetByID(userId);

            //Se a conta não existir, cria o registro e também inclui no ranking todos os personagens na conta. Se não existir, atualiza o IP e login da conta
            if (account is null)
            {
                account = new Account
                {
                    Id = userId,
                    Login = login,
                    Ip = ip
                };

                await _accountContext.Add(account);

                await _accountContext.SaveChangesAsync();

                await _roleContext.AddByAccountID(account.Id);

                await _roleContext.SaveChangesAsync();

                _logWriter.Write($"Conta {account.Id} criada. IP: {account.Ip} | Login: {account.Login}");
            }
            else
            {
                account.Ip = ip;

                account.Login = login;

                await _accountContext.SaveChangesAsync();

                _logWriter.Write($"A conta {login}({userId}) se conectou ao servidor com o IP {ip}");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task DeleteRole(int roleId)
    {
        try
        {
            Role roleToRemove = await _roleContext.GetRoleFromId(roleId);

            if (roleToRemove != null)
            {
                var playerBattles = await _battleContext.GetPlayerBattles(roleId);

                await _battleContext.RemoveRange(playerBattles);

                await _battleContext.SaveChangesAsync();

                await _roleContext.RemoveByModel(roleToRemove);

                await _roleContext.SaveChangesAsync();

                _logWriter.Write($"O personagem {roleToRemove.CharacterName} foi removido do Ranking, junto com os registros do seu PvP.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    public async Task<Role> CreateRole(int roleId)
    {
        try
        {
            await _accountContext.AddByRoleID(roleId);

            await _accountContext.SaveChangesAsync();

            var roleAdded = await _roleContext.AddByID(roleId);

            if (roleAdded is not null)
            {
                _logWriter.Write($"Role ID {roleId} foi criado e inserido no ranking.");
            }

            return roleAdded;
        }
        catch (Exception ex)
        {
            _logWriter.Write($"Role ID ERROR: {roleId} => \n" + ex.ToString());
            return default;
        }
    }
    private async Task RenameRole(int roleId)
    {
        try
        {
            Role role = await _roleContext.GetRoleFromId(roleId);

            if (role != null)
            {
                string newName = (await _serverContext.GetRoleByID(roleId))?.GRoleBase?.Name;

                role.CharacterName = newName;

                await _roleContext.SaveChangesAsync();
            }
            else
            {
                _logWriter.Write($"O personagem atual não existe no Ranking.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task LevelUpRole(int roleId, int curLevel)
    {
        try
        {
            Role role = await _roleContext.GetRoleFromId(roleId);

            if (role != null)
            {
                int oldLevel = role.Level;

                role.Level = role.Level >= curLevel ? role.Level : curLevel;

                role.LevelDate = DateTime.Now;

                await _roleContext.SaveChangesAsync();

                _logWriter.Write($"Registro de UP: {curLevel}. Level do personagem: {oldLevel}.\n\t\t{(curLevel > oldLevel ? $"O personagem {role.CharacterName} upou para o nível {role.Level}." : $"Nível não alterado.")}");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task ResetKDA(string log)
    {
        try
        {
            int questId = int.Parse(System.Text.RegularExpressions.Regex.Match(log, @"taskid=([0-9]*)").Value.Replace("taskid=", "").Trim());

            if (_definitions.QuestIdResetKDA.Equals(questId))
            {
                if (int.TryParse(System.Text.RegularExpressions.Regex.Match(log, @"roleid=([0-9]*)").Value.Replace("roleid=", ""), out int roleId))
                {
                    await _roleContext.ResetKDA(roleId);

                    await _roleContext.SaveChangesAsync();

                    await _serverContext.SendPrivateMessage(roleId, "Seu KDA foi zerado com sucesso. Digite !kda para checar.");
                }
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private long UpdateLastFileSize(long fileSize)
    {
        long difference = fileSize - lastSize;
        lastSize = fileSize;

        return difference;
    }
    private async Task<long> GetFileSize(string fileName)
    {
        try
        {
            var fileSize = new System.IO.FileInfo(fileName).Length;

            if (fileSize > lastSize)
                using (StreamWriter sw = new StreamWriter("./Configurations/Internal/lastlog.size"))
                    await sw.WriteAsync(fileSize.ToString());

            return fileSize;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private long LoadLastSize(string fileName)
    {
        try
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                long.TryParse(sr.ReadLine().Trim(), out long result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
}