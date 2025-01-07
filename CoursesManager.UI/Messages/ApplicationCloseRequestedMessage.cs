using CoursesManager.MVVM.Messages;

namespace CoursesManager.UI.Messages;

public class ApplicationCloseRequestedMessage : BaseMessage<ApplicationCloseRequestedMessage>
{
    public override ApplicationCloseRequestedMessage Clone()
    {
        return new ApplicationCloseRequestedMessage
        {
            MessageId = MessageId,
            TimeStamp = new(TimeStamp.Ticks)
        };
    }
}