namespace CoreRankingInfra.Utils;

public record RankingVersion
{
    private static string VersionPID = "2.0.2";

    public static string VersionDescription { get { return $"CURRENT CORERANKING VERSION: {VersionPID}"; } }
}
