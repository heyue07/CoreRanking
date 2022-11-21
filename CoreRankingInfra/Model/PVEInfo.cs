namespace CoreRankingInfra.Model;

public class PVEInfo
{
    public int RoleId { get; set; }
    public List<Collect> Collect { get; set; }
    public List<Hunt> Hunt { get; set; }
}