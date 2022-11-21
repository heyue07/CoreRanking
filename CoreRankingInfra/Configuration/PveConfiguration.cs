namespace CoreRankingInfra.Configuration;

public class PveConfiguration
{
    public bool isActive { get; }
    public double herbPoint { get; }
    public double mineralPoint { get; }
    public double huntPoint { get; }

    public PveConfiguration()
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/PvePoints.json"));

        isActive = jsonNodes["ATIVO"].ToObject<bool>();
        herbPoint = jsonNodes["PONTOS POR ERVAS COLETADAS"].ToObject<double>();
        mineralPoint = jsonNodes["PONTOS POR MATERIAIS COLETADOS"].ToObject<double>();
        huntPoint = jsonNodes["PONTOS POR DS COLETADOS"].ToObject<double>();
    }
}
