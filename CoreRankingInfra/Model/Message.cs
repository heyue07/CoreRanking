namespace CoreRankingInfra.Model;

public record Message
{
    public BroadcastChannel Channel { get; private init; }
    public int RoleID { get; private init; }
    public string RoleName { get; private init; }
    public string Text { get; private init; }

    public Message(BroadcastChannel Channel, int RoleID, string RoleName, string Text)
    {
        this.Channel = Channel;
        this.RoleID = RoleID;
        this.RoleName = RoleName;
        this.Text = Text;
    }
}