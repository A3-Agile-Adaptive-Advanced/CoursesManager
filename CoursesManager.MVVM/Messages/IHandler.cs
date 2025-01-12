namespace CoursesManager.MVVM.Messages;

/// <summary>
/// Interface to enable casting the subscriber wrapper to a type where we only know the messagetype and not the recipienttype. Shortens the Unsubscribe method in the messagebroker.
/// Else we would need to also pass the recipient type as type argument even though it would not be used for anything in that method.
/// </summary>
/// <typeparam name="TMessageType"></typeparam>
internal interface IHandler<TMessageType>
{
    Action<TMessageType> Handler { get; set; }
}