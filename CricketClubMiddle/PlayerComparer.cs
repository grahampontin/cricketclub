using System.Collections.Generic;
using CricketClubMiddle;

public class PlayerComparer : IEqualityComparer<Player>
{
    #region IEqualityComparer<Player> Members

    public bool Equals(Player x, Player y)
    {
        if (x.Id == y.Id) return true;
        return false;
    }

    public int GetHashCode(Player obj)
    {
        return obj.Id;
    }

    #endregion
}