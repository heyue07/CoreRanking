namespace CoreRanking.Watchers;

public class PvEWatch : BackgroundService
{
    private long lastSize;

    private readonly string path;

    private readonly PveConfiguration _pveDefinitions;

    private readonly ILogger<PvEWatch> _logger;

    private readonly IServerRepository _serverContext;

    private readonly IRoleRepository _roleContext;

    private readonly ICollectRepository _collectContext;

    private readonly IHuntRepository _huntContext;

    private readonly LogWriter _logWriter;

    private readonly List<int> mineralIds;

    private readonly List<int> herbsIds;

    private readonly List<int> huntAvailableIds;

    public PvEWatch(ILogger<PvEWatch> logger, LogWriter logWriter, IServerRepository serverContext, IRoleRepository roleContext,
        ICollectRepository collectContext, IHuntRepository huntContext, PveConfiguration pveDefinitions)
    {
        this._logger = logger;

        this._logWriter = logWriter;

        this._pveDefinitions = pveDefinitions;

        this._serverContext = serverContext;

        this._roleContext = roleContext;

        this._collectContext = collectContext;

        this._huntContext = huntContext;

        PWGlobal.UsedPwVersion = _serverContext.GetPwVersion();

        this.path = Path.Combine(_serverContext.GetLogsPath(), ELogFile.Log.GetDescription());

        lastSize = GetFileSize(path);

        this.herbsIds = Enumerable.Range(1820, 55).ToList();
        this.mineralIds = Enumerable.Range(795, 14).ToList();
        this.mineralIds.AddRange(new int[] { 815, 816, 817, 818, 819 });
        this.huntAvailableIds = Enumerable.Range(8079, 103).ToList();
    }

    protected override async Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
    {
        if (_pveDefinitions.isActive)
        {
            _logger.LogInformation("MÓDULO DE PVE INICIADO");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    long fileSize = GetFileSize(path);

                    if (fileSize > lastSize)
                    {
                        List<PVEInfo> generalInfo = await ReadTail(path, UpdateLastFileSize(fileSize));

                        foreach (var info in generalInfo.Where(x => x != null))
                        {
                            await UploadPvEEvent(info);
                        }
                    }

                    await Task.Delay(2000);
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
    public override Task StartAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(System.Threading.CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
    private async Task<List<PVEInfo>> ReadTail(string filename, long offset)
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

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<string> logs = GB2312ToUtf8(bytes).Split(new string[] { "\n" }[0]).Where(x => !string.IsNullOrEmpty(x.Trim())).ToList();

            List<PVEInfo> pveInformations = new List<PVEInfo>();

            foreach (var log in logs)
            {
                var pveIteration = await DecodeMessage(log);

                if (pveIteration?.Collect is null & pveIteration?.Hunt is null)
                {
                    continue;
                }

                pveInformations.Add(pveIteration);
            }

            return pveInformations;
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }

        return default;
    }
    private async Task UploadPvEEvent(PVEInfo info)
    {
        try
        {
            if (info.Hunt is null & info.Collect is null) return;

            Role currentRole = await _roleContext.GetRoleFromId(info.RoleId);

            if (currentRole is null)
            {
                await _serverContext.SendPrivateMessage(currentRole.RoleId, "Relogue sua conta para ser inserido no ranking e participar do ranking PVE, ou digite !participar");
                _logWriter.Write($"O personagem {info.RoleId} coletou {info.Collect.Count}x {info.Collect.First().ItemId}, mas não está cadastrado no ranking.");

                return;
            }

            double oldCollectPoint = currentRole.CollectPoint;

            if (info.Hunt != null)
            {
                foreach (var item in info.Hunt)
                {
                    currentRole.CollectPoint += item.ItemId switch
                    {
                        var itemId when huntAvailableIds.Contains(itemId) => _pveDefinitions.huntPoint,
                        var itemId when mineralIds.Contains(itemId) => _pveDefinitions.mineralPoint,
                        var itemId when herbsIds.Contains(itemId) => _pveDefinitions.herbPoint,
                        _ => 0
                    };
                }

                await _huntContext.AddByModel(info.Hunt);

                await _huntContext.SaveChangesAsync();
            }

            if (info.Collect != null)
            {
                foreach (var item in info.Collect)
                {
                    await _collectContext.AddByModel(item);

                    currentRole.CollectPoint += item.ItemId switch
                    {
                        var itemId when huntAvailableIds.Contains(itemId) => _pveDefinitions.huntPoint,
                        var itemId when mineralIds.Contains(itemId) => _pveDefinitions.mineralPoint,
                        var itemId when herbsIds.Contains(itemId) => _pveDefinitions.herbPoint,
                        _ => 0
                    };
                }

                await _collectContext.SaveChangesAsync();
            }

            await _roleContext.SaveChangesAsync();

            if (currentRole.CollectPoint > oldCollectPoint)
            {
                await _serverContext.SendPrivateMessage(currentRole.RoleId, $"Você ganhou {(currentRole.CollectPoint - oldCollectPoint).ToString("0.00")} ponto(s) no ranking de PVE. Digite !coleta para verificar seus pontos e !toprank pve para verificar o ranking geral.");
            }
        }
        catch (Exception ex)
        {
            _logWriter.Write(ex.ToString());
        }
    }
    private string GB2312ToUtf8(byte[] gb2312bytes)
    {
        Encoding fromEncoding = Encoding.GetEncoding("GB2312");
        Encoding toEncoding = Encoding.UTF8;
        return EncodingConvert(gb2312bytes, fromEncoding, toEncoding);
    }
    private string EncodingConvert(byte[] fromBytes, Encoding fromEncoding, Encoding toEncoding)
    {
        byte[] toBytes = Encoding.Convert(fromEncoding, toEncoding, fromBytes);

        string toString = toEncoding.GetString(toBytes);
        return toString;
    }
    private async Task<PVEInfo> DecodeMessage(string encodedMessage)
    {
        try
        {
            PVEInfo newInfo = new PVEInfo();

            string message = Regex.Match(encodedMessage, @"info : ([\s\S]*)").Value.Replace("info : ", "");

            if (message.Contains("卖店"))
            {
                List<Hunt> hunts = new List<Hunt>();

                int roleId = int.Parse(Regex.Match(message, @"用户([0-9]*)").Value.Replace("用户", ""));
                int amount = int.Parse(Regex.Match(message, @"卖店([0-9]*)").Value.Replace("卖店", ""));
                int itemId = int.Parse(Regex.Match(message, @"个([0-9]*)").Value.Replace("个", ""));

                if (huntAvailableIds.Contains(itemId))
                {
                    for (int i = 0; i < amount; i++)
                    {
                        hunts.Add(new Hunt
                        {
                            ItemId = itemId,
                            RoleId = roleId,
                            Date = DateTime.Now
                        });
                    }
                }

                newInfo.Hunt = hunts;
                newInfo.RoleId = roleId;
            }
            else if (message.Contains("采集得到"))
            {
                List<Collect> coletas = new List<Collect>();

                int roleId = int.Parse(Regex.Match(message, @"用户([0-9]*)").Value.Replace("用户", default));
                int amount = int.Parse(Regex.Match(message, @"采集得到([0-9]*)").Value.Replace("采集得到", default));
                int itemId = int.Parse(Regex.Match(message, @"个([0-9]*)").Value.Replace("个", default));

                if (mineralIds.Contains(itemId) | herbsIds.Contains(itemId))
                {
                    for (int i = 0; i < amount; i++)
                    {
                        coletas.Add(new Collect
                        {
                            RoleId = roleId,
                            Amount = 1,
                            ItemId = itemId,
                            Date = DateTime.Now
                        });
                    }
                }

                newInfo.Collect = coletas;
                newInfo.RoleId = roleId;
            }

            return newInfo;
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

    private long GetFileSize(string fileName)
    {
        return new FileInfo(fileName).Length;
    }
}