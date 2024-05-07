public enum ESortMethod
{
    None,
    Ascending,
    Descending
}

public enum EDisplayType
{
    None,
    Seconds,
    Milliseconds,
    Numeric
}

public class AppLeaderboardInfo
{
    public AppLeaderboardInfo()
    {
        ID = System.Guid.NewGuid().ToString();
    }
    public string ID { get; set; }
    public string ProjectID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ESortMethod SortMethod { get; set; } = ESortMethod.None;
    public EDisplayType DisplayType { get; set; } = EDisplayType.None;
}

public class AppLeaderboard
{
    public string PlayerID { get; set; } = string.Empty;
    public string Score { get; set; } = string.Empty;
}