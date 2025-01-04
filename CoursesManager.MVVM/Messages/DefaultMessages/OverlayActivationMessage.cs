namespace CoursesManager.MVVM.Messages.DefaultMessages
{
    public class OverlayActivationMessage : BaseMessage<OverlayActivationMessage>
    {
        public bool IsVisible { get; set; }

        public OverlayActivationMessage(bool isVisible)
        {
            IsVisible = isVisible;
        }


        public override OverlayActivationMessage Clone()
        {
            return new OverlayActivationMessage(IsVisible)
            {
                IsVisible = IsVisible,
                MessageId = MessageId,
                TimeStamp = new(TimeStamp.Ticks)
            };
        }
    }
}
