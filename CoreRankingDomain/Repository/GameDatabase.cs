namespace CoreRankingDomain.Repository;

public class GameDatabase
{
    private static string query = "SELECT ID FROM users;";

    public async static Task<List<int>> GetAllAccountsId()
    {
        try
        {
            List<int> response = new();

            var conn = ConnectionBuilder.GetGameDBString();

            using MySqlConnection mySqlConnection = new MySqlConnection(conn);

            await mySqlConnection.OpenAsync();

            MySqlCommand sqlcmd = new MySqlCommand(query, mySqlConnection);

            var reader = await sqlcmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                response.Add(reader.GetInt32(0));
            }

            return response;
        }
        catch (Exception ex)
        {
            LogWriter.StaticWrite($"ERRO NA OPERAÇÃO DE VARREDURA\n{ex}");
        }

        return default;
    }
}