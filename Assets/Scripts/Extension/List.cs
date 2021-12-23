using System.Collections.Generic;

public static class List
{
    public static bool containsMultiple(this List<Direction> thisObj, List<Direction>matchDirList)
    {
        foreach (Direction dir in matchDirList)
        {
            if (!thisObj.Contains(dir))
            {
                return false;
            }
        }
        return true;
    }
}
