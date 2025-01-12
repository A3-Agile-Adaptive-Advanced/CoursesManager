namespace CoursesManager.MVVM.Messages;

/// <summary>
/// Covariant interface type with T as out since the only method defined returns T.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICloneable<out T>
{
    T Clone();
}