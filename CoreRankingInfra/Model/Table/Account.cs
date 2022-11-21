namespace CoreRanking.Model.Table;

public class Account : ICloneable
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Ip { get; set; }

    public Account(Account account)
    {
        this.Id = account.Id;
        this.Login = account.Login;
        this.Ip = account.Ip;
    }

    public Account()
    {

    }

    public object Clone()
    {
        return new Account(this);
    }
}