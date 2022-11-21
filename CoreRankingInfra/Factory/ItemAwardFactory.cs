namespace CoreRankingInfra.Factory;

public class ItemAwardFactory
{
    private List<ItemAward> ItemsAward = new List<ItemAward>();
    public ItemAwardFactory()
    {
        JObject jsonNodes = (JObject)JsonConvert.DeserializeObject(File.ReadAllText("./Configurations/ItensAward.json"));

        foreach (var item in jsonNodes)
        {
            ItemAward itemAward = new ItemAward();

            itemAward.Id = int.Parse(item.Key);
            itemAward.Name = item.Value["NOME"].ToObject<string>();
            itemAward.Cost = item.Value["CUSTO"].ToObject<int>();
            itemAward.Stack = item.Value["STACK"].ToObject<int>();
            itemAward.Octet = item.Value["OCTET"].ToObject<string>();
            itemAward.Proctype = item.Value["PROCTYPE"].ToObject<string>();
            itemAward.Mask = item.Value["MASK"].ToObject<string>();

            ItemsAward.Add(itemAward);
        }
    }

    public List<ItemAward> Get()
    {
        return ItemsAward;
    }
}