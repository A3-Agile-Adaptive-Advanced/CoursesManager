using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;

namespace CoursesManager.UI.Repositories.Base;

public abstract class BaseRepository<T>
{
    public void OverwriteItemInPlace(T target, T source)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        if (source == null) throw new ArgumentNullException(nameof(source));

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }
    }

    protected static ObservableCollection<T> SetupCache(string cacheKey)
    {
        var col = new ObservableCollection<T>();
        GlobalCache.Instance.Put(cacheKey, col, true);
        return col;
    }
}