namespace CoursesManager.MVVM.Messages;

/// <summary>
/// Interface to enable casting the subscriber wrapper to a type where we only know the recipient type and not the message type. Shortens the UbsubscribeMe method in the messagebroker.
/// </summary>
/// <typeparam name="TRecipient"></typeparam>
internal interface IRecipient<TRecipient>
{
    TRecipient Recipient { get; set; }
}