namespace CoreRanking.Model.Table;

public class Role : ICloneable
{
    [Required]
    [ForeignKey("Account")]
    public int AccountId { get; set; }
    public Account Account { get; set; }
    [Key]
    public int RoleId { get; set; }
    public string CharacterName { get; set; }
    public string CharacterClass { get; set; }
    public string CharacterGender { get; set; }
    public int Kill { get; set; }
    public int Death { get; set; }
    public string Elo { get; set; }
    public int Level { get; set; }
    public byte RebornCount { get; set; }
    public DateTime LevelDate { get; set; }
    public int Points { get; set; }
    public int Doublekill { get; set; }
    public int Triplekill { get; set; }
    public int Quadrakill { get; set; }
    public int Pentakill { get; set; }
    public double CollectPoint { get; set; }

    public Role(Role role)
    {
        this.AccountId = role.AccountId;
        this.Account = role.Account;
        this.RoleId = role.RoleId;
        this.CharacterName = role.CharacterName;
        this.CharacterClass = role.CharacterClass;
        this.CharacterGender = role.CharacterGender;
        this.Kill = role.Kill;
        this.Death = role.Death;
        this.Elo = role.Elo;
        this.Level = role.Level;
        this.RebornCount = role.RebornCount;
        this.LevelDate = role.LevelDate;
        this.Points = role.Points;
        this.Doublekill = role.Doublekill;
        this.Triplekill = role.Triplekill;
        this.Quadrakill = role.Quadrakill;
        this.Pentakill = role.Pentakill;
        this.CollectPoint = role.CollectPoint;
    }

    public Role()
    {

    }

    public object Clone()
    {
        return new Role(this);
    }
}