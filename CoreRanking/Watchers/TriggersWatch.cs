namespace CoreRanking.Watchers;

public class TriggersWatch : BackgroundService
{
    private readonly ILogger<TriggersWatch> _logger;

    private readonly IServerRepository _serverContext;

    private readonly IRoleRepository _roleContext;

    private readonly RankingDefinitions _definitions;

    private readonly PveConfiguration pveDefinitions;

    private readonly MessageFactory _mFactory;

    private readonly LogWriter _logWriter;

    private readonly string path;

    private DateTime lastTopRank;

    private DateTime lastTopLevel;

    private long lastSize;

    public TriggersWatch(ILogger<TriggersWatch> logger, LogWriter logWriter, IServiceProvider services, PveConfiguration pveDefinitions)
    {
        try
        {
            lastTopRank = new DateTime(1990, 1, 1);

            lastTopLevel = new DateTime(1990, 1, 1);

            this.pveDefinitions = pveDefinitions;

            this._logger = logger;

            this._logWriter = logWriter;

            this._mFactory = (MessageFactory)services.GetService(typeof(MessageFactory));

            this._roleContext = (IRoleRepository)services.GetService(typeof(IRoleRepository));

            this._serverContext = (IServerRepository)services.GetService(typeof(IServerRepository));

            this._definitions = (RankingDefinitions)services.GetService(typeof(RankingDefinitions));

            this.path = Path.Combine(_serverContext.GetLogsPath(), ELogFile.Chat.GetDescription());

            lastSize = GetFileSize(path);

            PWGlobal.UsedPwVersion = _serverContext.GetPwVersion();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_definitions.isTriggerAllowed)
        {
            _logger.LogInformation("MÓDULO DE TRIGGERS INICIADO");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    long fileSize = GetFileSize(path);

                    if (fileSize > lastSize)
                    {
                        List<Message> messages = await ReadTail(path, UpdateLastFileSize(fileSize));

                        foreach (var message in messages)
                        {
                            await CommandForward(message);
                        }
                    }

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logWriter.Write(ex.ToString());
                }
            }
        }
        else
        {
            await StopAsync(new CancellationToken(true));
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
    private async Task<List<Message>> ReadTail(string filename, long offset)
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

            List<string> logs = Encoding.Default.GetString(bytes).Split(new string[] { "\n" }[0]).Where(x => !string.IsNullOrEmpty(x.Trim())).ToList();

            List<Message> decodedMessages = await _mFactory.GetMessages(logs);

            return decodedMessages.Where(x => x != default | x == null).ToList();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private async Task CommandForward(Message message)
    {
        try
        {
            if (message is null)
            {
                _logWriter.Write("Message model returned null");
                return;
            }

            //Redireciona à função respectiva do trigger acionado via chat global
            Task forwardCommand = message.Text.Trim() switch
            {
                string curMessage when curMessage.Contains(ETrigger.Ponto.GetDescription()) & !_definitions.isOnlyPveAllowed => _GetPoints(message),
                string curMessage when curMessage.Contains(ETrigger.Kill.GetDescription()) & !_definitions.isOnlyPveAllowed => _GetKill(message),
                string curMessage when curMessage.Contains(ETrigger.Kda.GetDescription()) & !_definitions.isOnlyPveAllowed => _GetKDA(message),
                string curMessage when curMessage.Contains(ETrigger.Atividade.GetDescription()) & !_definitions.isOnlyPveAllowed => _DisplayActivity(message),
                string curMessage when curMessage.Contains(ETrigger.TopRankLevel.GetDescription()) => _GetTopLevel(message),
                string curMessage when curMessage.Contains(ETrigger.TopRankPVE.GetDescription()) => _GetTopPVE(message),
                string curMessage when curMessage.Contains(ETrigger.TopRankKDA.GetDescription()) & !_definitions.isOnlyPveAllowed => _GetTopKDA(message),
                string curMessage when curMessage.Contains(ETrigger.TopRank.GetDescription()) & !_definitions.isOnlyPveAllowed => _GetTopPvP(message),
                string curMessage when curMessage.Contains(ETrigger.Reward.GetDescription()) & !_definitions.isOnlyPveAllowed => _DeliverReward(message),
                string curMessage when curMessage.Contains(ETrigger.Itens.GetDescription()) & !_definitions.isOnlyPveAllowed => _SendItemsAvailable(message),
                string curMessage when curMessage.Contains(ETrigger.Help.GetDescription()) => _SendHelpMessages(message),
                string curMessage when curMessage.Contains(ETrigger.Participar.GetDescription()) => _InsertOnRanking(message),
                string curMessage when curMessage.Contains(ETrigger.Transferir.GetDescription()) & !_definitions.isOnlyPveAllowed => _BuildTransferObject(message),
                string curMessage when curMessage.Contains(ETrigger.ServerVersion.GetDescription()) => _GetVersion(message),
                string curMessage when curMessage.Contains(ETrigger.FixRole.GetDescription()) => _FixRole(message),
                string curMessage when curMessage.Contains(ETrigger.Coleta.GetDescription()) => _Coleta(message),
                _ => Task.Run(() => Task.CompletedTask)
            };

            if (forwardCommand != null)
                await forwardCommand;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

    }

    private async Task _Coleta(Message message)
    {
        try
        {
            //Trata o comando "!coleta [player]" que o jogador digitou no global
            string roleName = message.Text.Replace(ETrigger.Coleta.GetDescription(), default).Trim();

            //Verifica se o jogador digitou o comando "!coleta" puro ou direcionado a um jogador específico
            roleName = string.IsNullOrEmpty(roleName) ? message.RoleName : roleName;

            //Busca roleName na database do Ranking
            Role roleToDisplay = await _roleContext.GetRoleFromNameAsNoTracking(roleName);

            Task currentTask = roleToDisplay != null ?
                _serverContext.SendMessage(message.Channel, $"{roleToDisplay.CharacterName} possui {roleToDisplay.CollectPoint} pontos de coleta.")
                :
                _serverContext.SendPrivateMessage(message.RoleID, $"O personagem {roleName} não existe no ranking.");

            await currentTask;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    private async Task _FixRole(Message message)
    {
        try
        {
            //!fixrole id:1024

            if (!(await _serverContext.GetGmID()).Contains(await _serverContext.GetUserIDByRoleID(message.RoleID))) return;

            int roleId = int.Parse(Regex.Match(message.Text.Replace("id: ", "id:"), @"id:\w+").Value.Replace("id:", string.Empty));

            await _roleContext.RemoveByID(roleId);

            await _roleContext.SaveChangesAsync();

            await _roleContext.AddByID(roleId);

            await _serverContext.SendPrivateMessage(message.RoleID, $"Personagem ID {roleId} foi re-inserido no ranking.");
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    private async Task _GetVersion(Message message)
    {
        try
        {
            await _serverContext.SendPrivateMessage(message.RoleID, CoreRankingInfra.Utils.RankingVersion.VersionDescription);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }

    private async Task _GetPoints(Message message)
    {
        try
        {
            //Trata o comando "!ponto [player]" que o jogador digitou no global
            string roleName = message.Text.Replace(ETrigger.Ponto.GetDescription(), default).Trim();

            //Verifica se o jogador digitou o comando "!ponto" puro ou direcionado a um jogador específico
            roleName = string.IsNullOrEmpty(roleName) ? message.RoleName : roleName;

            //Busca roleName na database do Ranking
            Role roleToDisplay = await _roleContext.GetRoleFromNameAsNoTracking(roleName);

            //Se o jogador da requisição existe na database, retorna a quantidade de pontos, se não, informa que não existe ou erro na requisição
            if (roleToDisplay != null)
            {
                await _serverContext.SendMessage(message.Channel, $"{roleToDisplay.CharacterName} possui {await _GetPoints(roleName)} pontos.", roleToDisplay.RoleId);

                return;
            }

            var currentTask = roleName.Equals("s") ?
                _serverContext.SendPrivateMessage(message.RoleID, $"Talvez você tenha digitado o comando errado. Tente !ponto, sem 's'")
                :
                _serverContext.SendPrivateMessage(message.RoleID, $"O personagem {roleName} não existe no ranking.");

            await currentTask;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _GetKill(Message message)
    {
        try
        {
            string roleName = message.Text.Replace(ETrigger.Kill.GetDescription(), default).Trim();

            roleName = string.IsNullOrEmpty(roleName) ? message.RoleName : roleName;

            Role roleToDisplay = await _roleContext.GetRoleFromNameAsNoTracking(roleName);

            Task currentTask = roleToDisplay != null ?
                _serverContext.SendMessage(message.Channel, $"{roleToDisplay.CharacterName} possui {roleToDisplay.Kill} kills.", roleToDisplay.RoleId)
                :
                _serverContext.SendPrivateMessage(message.RoleID, $"O personagem {roleName} não existe no ranking.");

            await currentTask;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

    }
    private async Task _GetKDA(Message message)
    {
        try
        {
            string roleName = message.Text.Replace(ETrigger.Kda.GetDescription(), default).Trim();

            roleName = string.IsNullOrEmpty(roleName) ? message.RoleName : roleName;

            Role roleToDisplay = await _roleContext.GetRoleFromNameAsNoTracking(roleName);

            Task currentTask = roleToDisplay != null ?
                _serverContext.SendMessage(message.Channel, $"{roleToDisplay.CharacterName} possui {(await _roleContext.GetKDA(roleName)).ToString("0.00")} KDA.", roleToDisplay.RoleId)
                :
                _serverContext.SendPrivateMessage(message.RoleID, $"O personagem {roleName} não existe no ranking.");

            await currentTask;

        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

    }
    private async Task _DisplayActivity(Message message)
    {
        try
        {
            string roleName = message.Text.Replace(ETrigger.Atividade.GetDescription(), default).Trim();

            roleName = string.IsNullOrEmpty(roleName) ? message.RoleName : roleName;

            var values = await _roleContext.GetInteractions(roleName);

            string textMessage = $"{roleName} possui {values.Key} participações no PVP. {values.Value} Kills; {values.Key - values.Value} Mortes.";

            await _serverContext.SendMessage(message.Channel, textMessage, message.RoleID);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _GetTopLevel(Message message)
    {
        try
        {
            int requesterRoleId = message.RoleID;

            string classe = message.Text.Replace(ETrigger.TopRankLevel.GetDescription(), default).Trim();

            double cooldown = DateTime.Now.Subtract(lastTopLevel).TotalSeconds;

            if (!(lastTopLevel.Year.Equals(1990) || cooldown > 10))
            {
                await _serverContext.SendPrivateMessage(requesterRoleId, $"O pedido está em tempo de espera. Tente novamente em {((cooldown - 10) * -1).ToString("0")} segundos.");
                return;
            }

            if (classe.Length > 0 && string.IsNullOrEmpty(classe.ConvertClassToGameStructure()))
            {
                await _serverContext.SendPrivateMessage(requesterRoleId, $"A classe {classe} não existe.");
                return;
            }

            List<Role> roles = await _roleContext.GetTopLevelRank(classe);

            if (roles?.Count <= 0)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "Ainda não há jogadores suficientes para compor o ranking especificado.");
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var player in roles?.Select((value, i) => (value, i)))
            {
                sb.Clear();

                sb.Append($"{player.i + 1}º lugar: {player.value.CharacterName}. ");

                if (_definitions.hasReborn)
                    sb.Append($"Reborn: {player.value.RebornCount}. ");

                sb.Append($"Nível {player.value.Level}");

                await _serverContext.SendMessage(message.Channel, sb.ToString(), message.RoleID);
            }

            this.lastTopLevel = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _GetTopPvP(Message message)
    {
        try
        {
            int requesterRoleId = message.RoleID;

            double cooldown = DateTime.Now.Subtract(lastTopRank).TotalSeconds;

            if (lastTopRank.Year.Equals(1990) || cooldown > 10)
            {
                string classe = message.Text.Replace(ETrigger.TopRank.GetDescription(), default).Trim();

                if (classe.Length > 0 && string.IsNullOrEmpty(classe.ConvertClassToGameStructure()))
                {
                    await _serverContext.SendPrivateMessage(requesterRoleId, $"A classe {classe} não existe.");
                    return;
                }

                List<Role> roles = await _GetPodium(classe);

                if (roles?.Count <= 0)
                {
                    await _serverContext.SendPrivateMessage(message.RoleID, "Ainda não há jogadores suficientes para compor o ranking especificado.");
                    return;
                }

                foreach (var player in roles.Select((value, i) => (value, i)))
                {
                    await _serverContext.SendMessage(message.Channel, $"{player.i + 1}º lugar: {player.value.CharacterName}. Kills {player.value.Kill}", message.RoleID);
                }

                lastTopRank = DateTime.Now;
            }
            else
            {
                await _serverContext.SendPrivateMessage(requesterRoleId, $"O pedido está em tempo de espera. Tente novamente em {((cooldown - 10) * -1).ToString("0")} segundos.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _GetTopKDA(Message message)
    {
        try
        {
            int requesterRoleId = message.RoleID;

            double cooldown = DateTime.Now.Subtract(lastTopRank).TotalSeconds;

            if (message.Channel != BroadcastChannel.World) return;

            if (lastTopRank.Year.Equals(1990) || cooldown > 10)
            {
                string classe = message.Text.Replace(ETrigger.TopRankKDA.GetDescription(), default).Trim();

                List<Role> topPlayers = await _roleContext.GetTopRankingByKDA(classe);

                if (topPlayers?.Count > 0)
                {
                    foreach (var player in topPlayers.Select((value, i) => (value, i)))
                    {
                        await _serverContext.SendMessage(message.Channel, $"{player.i + 1}º lugar: {player.value.CharacterName}. KDA: {await _roleContext.GetKDA(player.value.CharacterName)}", message.RoleID);
                    }

                    lastTopRank = DateTime.Now;

                    return;
                }

                await _serverContext.SendPrivateMessage(requesterRoleId, "Ainda não há jogadores suficientes para compor o ranking especificado.");
            }
            else
            {
                await _serverContext.SendPrivateMessage(requesterRoleId, $"O pedido está em tempo de espera. Tente novamente em {((cooldown - 10) * -1).ToString("0")} segundos.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _GetTopPVE(Message message)
    {
        try
        {
            if (!pveDefinitions.isActive) return;

            int requesterRoleId = message.RoleID;

            double cooldown = DateTime.Now.Subtract(lastTopRank).TotalSeconds;

            if (lastTopRank.Year.Equals(1990) || cooldown > 10)
            {
                string classe = message.Text.Replace(ETrigger.TopRankPVE.GetDescription(), default).Trim();

                List<Role> topPlayers = await _roleContext.GetTopRankingByCollectPoint(classe);

                if ((bool)(topPlayers?.Count > 0 & topPlayers?.Any(x => x.CollectPoint > 0)))
                {
                    foreach (var player in topPlayers.Select((value, i) => (value, i)))
                    {
                        await _serverContext.SendMessage(message.Channel, $"{player.i + 1}º lugar: {player.value.CharacterName}. Ponto de Coleta: {player.value.CollectPoint.ToString("0.00")}", message.RoleID);
                    }

                    lastTopRank = DateTime.Now;

                    return;
                }

                await _serverContext.SendPrivateMessage(requesterRoleId, "Ainda não há jogadores suficientes para compor o ranking especificado.");
            }
            else
            {
                await _serverContext.SendPrivateMessage(requesterRoleId, $"O pedido está em tempo de espera. Tente novamente em {((cooldown - 10) * -1).ToString("0")} segundos.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _SendHelpMessages(Message message)
    {
        try
        {
            if (!_definitions.isOnlyPveAllowed)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !ponto serve para mostrar quantos pontos algum char tem. Exemplo: !ponto Player");

                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !kill serve para mostrar quantas kills algum char tem. Exemplo: !kill Player");

                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !atividade serve para mostrar quantas participações no pvp algum char tem. Exemplo: !atividade Player");

                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !kda mostra sua relação de kill sobre morte. Quanto mais alto, melhor. Exemplo: !kda Player");

                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !toprank serve para mostrar quantas kills os primeiros do ranking tem. É possível filtrar por classe e level. Exemplos: !toprank ou !toprank wr");
            }

            //Verifica se há itens elegíveis para recompensa para enviar mensagem informando da funcionalidade
            if (_definitions.ItemsReward.Count >= 1)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !reward serve para resgatar seus pontos por algum item.");

                await _serverContext.SendPrivateMessage(message.RoleID, "O comando !itens serve para mostrar todos os itens elegíveis para trocar por pontos.");
            }

            //Verifica se a transferência está disponível para enviar mensagem informando da funcionalidade
            if (_definitions.isTrasferenceAllowed)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, $"O comando transferir serve para transferir seus pontos para algum personagem. Exemplo: !transferir Player 10.");
            }

            await _serverContext.SendPrivateMessage(message.RoleID, "O comando !coleta serve para mostrar quantos pontos de coleta PVE um personagem tem. Exemplo: !coleta Player");

            await _serverContext.SendPrivateMessage(message.RoleID, "O comando !toprank serve para mostrar os jogadores em ranking. É possível filtrar por PVE e Level. Exemplos: '!toprank level' ou '!toprank pve'");
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _DeliverReward(Message message)
    {
        try
        {
            //Retira o trigger !reward da mensagem escrita, restando o nome do item e a quantidade, esta se houver
            string sentence = message.Text.Trim().Replace(ETrigger.Reward.GetDescription(), default).Trim();

            //Captura a quantidade de item, se houver número
            int amount = sentence.Any(char.IsDigit) ? int.Parse(System.Text.RegularExpressions.Regex.Match(sentence, @"\d+").Value) : 1;

            //Retorna o item especifico filtrado a quantidade
            sentence = sentence.Replace(amount.ToString(), default).Trim();

            //Verifica se há recompensas elegíveis
            if (_definitions.ItemsReward.Count <= 0)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "Não há itens disponíveis para recompensa.");

                return;
            }

            //Verifica se o jogador que acionou o trigger está cadastrado no ranking
            Role currentUser = await _roleContext.GetRoleFromId(message.RoleID);
            if (currentUser is null)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "Você não está cadastrado(a) no ranking. Relogue sua conta para participar.");

                return;
            }

            //Verifica a nomenclatura da escrita do item a ser resgatado, tratando espaços vazios e casing
            if (!_definitions.ItemsReward.Select(x => x.Name.ToLower()).Contains(sentence.Trim().ToLower()))
            {
                //Verifica se o jogador digitou algo, ou se digitou algo errado para montar uma mensagem de feedback
                string displayMessage = sentence.Trim().Length <= 1 ? "É necessário especificar o nome do item a ser recebido" : @$"O item ""{sentence}"" não está elegível para recompensa.";

                await _serverContext.SendPrivateMessage(message.RoleID, displayMessage);

                //Envia os itens disponíveis para recompensa
                await _SendItemsAvailable(message);

                return;
            }

            //Verifica a quantidade de itens resgatados para evitar overflow
            if (amount > 99999 || amount <= 0)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, $"A quantidade resgatada precisa estar contida no intervalo entre 1 e 99.999");

                return;
            }

            //Seleciona o item escolhido pelo jogador
            ItemAward itemChoosed = _definitions.ItemsReward.Where(x => x.Name.ToLower().Contains(sentence.ToLower().Trim())).FirstOrDefault();

            //Verifica se o custo do item multiplicado pela quantia desejada supera os pontos que o jogador possui
            if (itemChoosed.Cost * amount > currentUser.Points)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, @$"Você não tem pontos suficientes para resgatar ""{sentence}"". Necessita de {itemChoosed.Cost * amount} ponto(s).");

                return;
            }

            //Gera o registro do item ingame e entrega no correio
            await _serverContext.DeliverReward(itemChoosed, amount, message.RoleID, null, null);

            currentUser.Points -= itemChoosed.Cost * amount;

            await _serverContext.SendPrivateMessage(message.RoleID, $"Sua recompensa foi entregue. Em sua Caixa de Correios deve haver {amount}x {itemChoosed.Name}({itemChoosed.Cost * amount} pontos). Te restam {currentUser.Points} pontos.");

            await _roleContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _SendItemsAvailable(Message message)
    {
        try
        {
            foreach (var item in _definitions.ItemsReward)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, $"Item: {item.Name}. Custo: {item.Cost} ponto(s).");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task<int> _GetPoints(string roleName)
    {
        try
        {
            int points = 0;

            Role requestRole = await _roleContext.GetRoleFromNameAsNoTracking(roleName);

            if (requestRole != null)
            {
                points = requestRole.Points;
            }

            return points;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private async Task<List<Role>> _GetPodium(string classe)
    {
        try
        {
            string convertedClass = classe.ConvertClassToGameStructure();

            //Se a classe for nula, pega o ranking geral. Se não for nula, pega o ranking de classe específica
            List<Role> topPlayers = await _roleContext.GetTopRank(convertedClass);

            return topPlayers?.Where(x => x.Kill > 0).ToList();
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private async Task _InsertOnRanking(Message message)
    {
        try
        {
            if (!await _roleContext.ExistAnyRecord(message.RoleID))
            {
                Role response = await _roleContext.AddByID(message.RoleID);

                if (response is null)
                {
                    await _serverContext.SendPrivateMessage(message.RoleID, "Houve um erro no seu cadastro. Entre em contato com a administração.");
                    return;
                }

                await _serverContext.SendPrivateMessage(response.RoleId, "Você foi inserido no CoreRanking. Digite !help para instruções de comandos.");
            }
            else
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "Você já está participando do ranking. Digite !help para receber a lista de comandos disponíveis.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task _BuildTransferObject(Message message)
    {
        try
        {
            //Verifica se está permitido transferências no servidor.
            if (!_definitions.isTrasferenceAllowed)
            {
                await _serverContext.SendPrivateMessage(message.RoleID, "O módulo de transferências não está ativado no servidor.");

                return;
            }

            //Inicializa nova instância da sentença de request, substituindo o trigger !transferir
            string sentence = new string(message.Text);

            sentence = sentence.Replace(ETrigger.Transferir.GetDescription(), default).Trim();

            Transference transference = new Transference() { idFrom = message.RoleID };

            //Verifica se há digito na requisição de transferência. Caso não haja, será transferido 1 ponto
            if (sentence.Any(char.IsDigit) && sentence.Length > 0)
            {
                //Extrai a quantidade de pontos especificado na request e substitui na sentença de request, restando apenas o nome na string
                string pointString = Regex.Match(sentence, @" \d+").Value.Trim();

                sentence = Regex.Replace(sentence, @" \d+", "");

                //Tratamento de quantidade para evitar overflow
                transference.points = pointString.Length > 7 ? 9_999_999 : int.Parse(pointString);
            }
            else
            {
                //Caso não haja número especificado na transferência, é considerado 1 ponto
                transference.points = 1;
            }

            //Restando apenas o nome na string, busco no repo do server o Id do personagem pelo nome.
            transference.idTo = await _serverContext.GetRoleIdByName(sentence);

            await TransferPoints(transference);
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private async Task TransferPoints(Transference transference)
    {
        try
        {
            //Verifica se o personagem que tenta transferir existe na database
            Role roleFrom = await _roleContext.GetRoleFromId(transference.idFrom);
            if (roleFrom is null)
            {
                await _serverContext.SendPrivateMessage(transference.idFrom, "Você não está cadastrado(a) no ranking. Relogue sua conta para participar, ou digite !participar.");
                return;
            }

            //Verifica se o personagem que tenta receber existe na database
            Role roleTo = await _roleContext.GetRoleFromId(transference.idTo);
            if (roleTo is null)
            {
                await _serverContext.SendPrivateMessage(transference.idTo, "Você não está cadastrado(a) no ranking. Relogue sua conta para participar, ou digite !participar.");
                return;
            }

            //Verifica se o valor transferido é negativo
            if (transference.points <= 0)
            {
                await _serverContext.SendPrivateMessage(roleFrom.RoleId, "Não é possível realizar transferência de valores negativos ou nulos.");
                _logWriter.Write($"{roleFrom.CharacterName}({roleFrom.RoleId}) tentou realizar transferência de valor negativo ou nulo. Valor: {transference.points}");
                return;
            }

            //Verifica se o destino da transferência é igual ao de origem
            if (roleFrom.RoleId.Equals(roleTo.RoleId))
            {
                await _serverContext.SendPrivateMessage(roleFrom.RoleId, "Não é possível realizar transferência para si mesmo.");
                return;
            }

            //Verifica se há pontos o suficiente para realizar a transferência
            if (roleFrom.Points < transference.points)
            {
                await _serverContext.SendPrivateMessage(roleFrom.RoleId, $"Você não tem pontos suficientes para realizar a transferência. Sua pontuação: {roleFrom.Points}. Pontuação necessária: {transference.points}");
                _logWriter.Write($"O personagem {roleFrom.CharacterName} tentou enviar {transference.points} a {roleTo.CharacterName}, mas não teve pontos suficientes.");
                return;
            }

            roleTo.Points += transference.points;

            roleFrom.Points -= transference.points;

            await _roleContext.SaveChangesAsync();

            await _serverContext.SendPrivateMessage(roleTo.RoleId, $"{roleFrom.CharacterName} te enviou {transference.points} pontos. Totalizam-te {roleTo.Points} pontos.");

            await _serverContext.SendPrivateMessage(roleFrom.RoleId, $"Você enviou {transference.points} ponto(s) ao(à) jogador(a) {roleTo.CharacterName}. Totalizam-te {roleFrom.Points} pontos.");

            _logWriter.Write($"O personagem {roleFrom.CharacterName} enviou a {roleTo.CharacterName} {transference.points} pontos. \n{roleTo.CharacterName}:{roleTo.Points}\n{roleFrom.CharacterName}:{roleFrom.Points}");
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
    private long GetFileSize(string fileName)
    {
        return new FileInfo(fileName).Length;
    }
}