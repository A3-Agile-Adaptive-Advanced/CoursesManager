using CoursesManager.UI.Enums;

namespace CoursesManager.MVVM.Messages
{
    public class ToastNotificationMessage : BaseMessage<ToastNotificationMessage>
    {
        public bool SetVisibillity { get; set; }
        public string NotificationText { get; set; }
        public ToastType ToastType { get; set; }
        public bool IsPersistent { get; set; }

        public ToastNotificationMessage(bool setVisibillity, string notificationText, ToastType toastType, bool isPersistent = false)
        {
            SetVisibillity = setVisibillity;
            NotificationText = notificationText;
            ToastType = toastType;
            IsPersistent = isPersistent;
        }


        public override ToastNotificationMessage Clone()
        {
            return new ToastNotificationMessage(SetVisibillity, NotificationText, ToastType, IsPersistent)
            {
                SetVisibillity = SetVisibillity,
                NotificationText = NotificationText,
                ToastType = ToastType,
                IsPersistent = IsPersistent,
                MessageId = MessageId,
                TimeStamp = new(TimeStamp.Ticks)
            };
        }
    }
}
