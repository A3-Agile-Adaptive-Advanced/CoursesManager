namespace CoursesManager.MVVM.Messages
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
