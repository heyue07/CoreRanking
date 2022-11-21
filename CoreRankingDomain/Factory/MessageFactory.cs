namespace CoreRankingDomain.Factory;

public record MessageFactory
{
    private readonly IServerRepository _serverContext;
    public MessageFactory(IServerRepository serverContext)
    {
        this._serverContext = serverContext;
    }

    private async Task<Message> GetMessage(string log)
    {
        if (log.Contains("src=") && !log.Contains("src=-1") && !log.Contains("whisper"))
        {
            if (int.TryParse(System.Text.RegularExpressions.Regex.Match(log, @"chl=([0-9]*)").Value.Replace("chl=", ""), out int channel))
            {
                //Caso a mensagem tenha sido enviada no global
                //obtém o canal principal configurado em .JSON
                //para então usar esse canal pra enviar a mensagem globalmente
                if (_serverContext.GetChannelsAllowed().Contains((BroadcastChannel)channel))
                {
                    channel = channel is 1 ? (int)_serverContext.GetMainChannel() : 14;
                }

                //Se conseguir dar parse em RoleID e o canal de envio da mensagem estiver contido dentro da lista de canais permitidos
                if (int.TryParse(System.Text.RegularExpressions.Regex.Match(log, @"src=([0-9]*)").Value.Replace("src=", ""), out int roleId))
                {
                    string text = Encoding.Unicode.GetString(Convert.FromBase64String(System.Text.RegularExpressions.Regex.Match(log, @"msg=([\s\S]*)").Value.Replace("msg=", "")));

                    Message newMessage = new Message(
                        (BroadcastChannel)channel,
                        roleId,
                        await _serverContext.GetRoleNameByID(roleId),
                        text);

                    return newMessage;
                }
            }
        }

        return default;
    }

    public async Task<List<Message>> GetMessages(List<string> logs)
    {
        List<Message> messages = new List<Message>();

        foreach (var log in logs)
        {
            var message = await GetMessage(log);

            if (message is null) continue;

            messages.Add(message);
        }

        return messages;
    }
}