namespace CoreRankingInfra.Factory;

public class ClassPointConfigFactory
{
    private readonly List<ClassPointConfig> ClassesConfig;

    public ClassPointConfigFactory()
    {
        ClassesConfig = new List<ClassPointConfig>();

        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/PointsConfiguration.json"));

        string[] classes = new string[] { "WR", "MG", "WB", "WF", "EA", "EP", "MC", "PSY", "SK", "MS", "RT", "TM" };

        foreach (var classe in classes)
        {
            ClassPointConfig newClassPointConfig = new();

            newClassPointConfig.Ocuppation = classe;
            newClassPointConfig.onKill = jsonNodes[classe]["onKill"].ToObject<int>();
            newClassPointConfig.onDeath = jsonNodes[classe]["onDeath"].ToObject<int>();

            ClassesConfig.Add(newClassPointConfig);
        }
    }

    public List<ClassPointConfig> Get()
    {
        return ClassesConfig;
    }
}