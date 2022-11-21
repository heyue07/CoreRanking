namespace CoreRankingInfra.Utils;

public static class Mapper
{
    public static Role ToRole(this GRoleData data)
    {
        Role role = new();

        role.AccountId = data.GRoleBase.UserId;
        role.CharacterGender = data.GRoleBase.Gender == 0 ? "Male" : "Female";
        role.Level = data.GRoleStatus.Level;
        role.AccountId = data.GRoleBase.UserId;
        role.RoleId = data.GRoleBase.Id;
        role.CharacterClass = data.GRoleBase.Class.ToString();
        role.CharacterName = data.GRoleBase.Name;
        role.RebornCount = GetRebornCount(data.GRoleStatus.ReincarnationData);

        return role;
    }

    public static byte GetRebornCount(string octet)
    {
        using BinaryReader binaryReader = new BinaryReader(new MemoryStream(StringToByteArray(octet)));

        binaryReader.ReadInt32();
        binaryReader.ReadByte();

        var rebornCount = binaryReader.ReadInt32();

        return (byte)rebornCount;
    }
    private static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }
    public static GRoleInventory ToGRoleInventory(this ItemAward itemAward)
    {
        return new GRoleInventory
        {

            Count = itemAward.Count,
        };
    }

    public static Battle Prepare(this Battle battle)
    {
        return battle with { KilledRole = null, KillerRole = null };
    }

    /// <summary>
    /// Traduz as iniciais de cada classe para o nome original da classe, utilizado na estrutura do jogo. Ex.: EP = Priest
    /// </summary>
    /// <param name="classInitials">Sigla que representa a classe</param>
    /// <returns></returns>
    public static string ConvertClassToGameStructure(this string classInitials)
    {
        return classInitials.ToLower() switch
        {
            "wr" => "Warrior",
            "mg" => "Mage",
            "psy" => "Shaman",
            "wf" => "Druid",
            "wb" => "Werewolf",
            "mc" => "Assassin",
            "ea" => "Archer",
            "ep" => "Priest",
            "sk" => "Guardian",
            "ms" => "Mystic",
            "tm" => "Reaper",
            "rt" => "Ghost",
            _ => ""
        };
    }

    /// <summary>
    /// Traduz o nome de cada classe utilizada na estrutura do jogo para o conhecido comumente.
    /// </summary>
    /// <param name="classFullName">Nome inteiro da classe</param>
    /// <returns></returns>
    public static string ConvertClassFromGameStructure(this string classFullName)
    {
        return classFullName.ToLower() switch
        {
            "warrior" => "WR",
            "mage" => "MG",
            "shaman" => "PSY",
            "druid" => "WF",
            "werewolf" => "WB",
            "assassin" => "MC",
            "archer" => "EA",
            "priest" => "EP",
            "guardian" => "SK",
            "mystic" => "MS",
            "reaper" => "TM",
            "ghost" => "RT",
            _ => ""
        };
    }
}