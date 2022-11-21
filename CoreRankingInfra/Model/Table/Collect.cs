namespace CoreRanking.Model.Table;

public class Collect
{
    [Key]
    public int Id { get; set; }
    public int ItemId { get; set; }
    [Required]
    [ForeignKey("Role")]
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}