namespace CoursesManager.MVVM.Messages;

/// <summary>
/// Wrapper for the handler of a message and the recipient so it will be easier to unsubscribe based on a recipient.
/// </summary>
/// <typeparam name="TMessageType"></typeparam>
/// <typeparam name="TRecipient"></typeparam>
/// <param name="handler"></param>
/// <param name="recipient"></param>
internal class Subscriber<TMessageType, TRecipient>(Action<TMessageType> handler, TRecipient recipient) : IHandler<TMessageType>, IRecipient<TRecipient>
{
    public Action<TMessageType> Handler { get; set; } = handler;

    public TRecipient Recipient { get; set; } = recipient;
}