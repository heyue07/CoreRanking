namespace CoreRankingInfra.Model.Enum;

public enum ETrigger : byte
{
    [Description("!ponto")]
    Ponto,

    [Description("!kill")]
    Kill,

    [Description("!kda")]
    Kda,

    [Description("!atividade")]
    Atividade,

    [Description("!toprank pve")]
    TopRankPVE,

    [Description("!toprank level")]
    TopRankLevel,

    [Description("!toprank kda")]
    TopRankKDA,

    [Description("!toprank")]
    TopRank,

    [Description("!reward")]
    Reward,

    [Description("!itens")]
    Itens,

    [Description("!help")]
    Help,

    [Description("!participar")]
    Participar,

    [Description("!transferir")]
    Transferir,

    [Description("!getversion")]
    ServerVersion,

    [Description("!fixrole")]
    FixRole,

    [Description("!coleta")]
    Coleta
}