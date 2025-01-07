namespace CoursesManager.MVVM.Messages.DefaultMessages
{
    public class MouseButtonReleasedMessage : BaseMessage<MouseButtonReleasedMessage>
    {
        public override MouseButtonReleasedMessage Clone()
        {
            return new MouseButtonReleasedMessage()
            {
                MessageId = MessageId,
                TimeStamp = new(TimeStamp.Ticks)
            };
        }
    }
}
