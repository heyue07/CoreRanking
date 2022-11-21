namespace CoreRankingInfra.Utils;

public class OctetConverter
{
    public static dynamic GetRebornInformations(byte[] octet)
    {
        using BinaryReader binaryReader = new BinaryReader(new MemoryStream(octet));

        binaryReader.ReadInt32();
        binaryReader.ReadByte();

        var teste = binaryReader.Read();

        return teste;
    }
}