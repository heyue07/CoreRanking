namespace CoreRanking.Model.Table;

public record Battle
{
    [Key]
    public int Id { get; set; }
    public DateTime Date { get; set; }
    [Required]
    [ForeignKey("KillerRole")]
    public int KillerId { get; set; }
    public Role KillerRole { get; set; }
    [Required]
    [ForeignKey("KilledRole")]
    public int KilledId { get; set; }
    public Role KilledRole { get; set; }
}