namespace Nhs.Appointments.Core.Extensions;

public static class ListExtensions
{
    public static List<T> RemoveFirstOccurrence<T>(this List<T> list, T candidate)
    {
        var index = list.IndexOf(candidate);
        if (index == -1)
        {
            return list;
        }

        var newList = new List<T>(list);
        newList.RemoveAt(index);
        return newList;
    }
}
