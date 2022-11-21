namespace CoreRankingInfra.Model;

public interface Multikill
{
    public double Time { get; set; }
    public int Points { get; set; }
    public List<string> Messages { get; set; }
}