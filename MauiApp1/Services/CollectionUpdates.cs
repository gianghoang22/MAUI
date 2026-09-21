using System.Collections.ObjectModel;

namespace MauiApp1.Services;

public static class CollectionUpdates
{
    public static void Apply<TItem>(ObservableCollection<TItem> target, IEnumerable<TItem> source) where TItem : notnull
    {
        var desired = source.ToList();
        var retained = desired.ToHashSet();
        for (var index = target.Count - 1; index >= 0; index--)
            if (!retained.Contains(target[index])) target.RemoveAt(index);

        for (var index = 0; index < desired.Count; index++)
        {
            if (index < target.Count && EqualityComparer<TItem>.Default.Equals(target[index], desired[index])) continue;
            var previousIndex = target.IndexOf(desired[index]);
            if (previousIndex >= 0) target.Move(previousIndex, index);
            else target.Insert(index, desired[index]);
        }
    }
}
