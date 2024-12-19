using CoursesManager.UI.Enums;
using CoursesManager.UI.Messages;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.MVVM.Messages
{
    public class ToastNotificationMessage : BaseMessage<ToastNotificationMessage>
    {
        public bool SetVisibillity { get; set; }
        public string NotificationText { get; set; }
        public ToastType ToastType { get; set; }

        public ToastNotificationMessage(bool setVisibillity, string notificationText, ToastType toastType)
        {
            SetVisibillity = setVisibillity;
            NotificationText = notificationText;
            ToastType = toastType;
        }


        public override ToastNotificationMessage Clone()
        {
            return new ToastNotificationMessage(SetVisibillity, NotificationText, ToastType)
            {
                SetVisibillity = SetVisibillity,
                NotificationText = NotificationText,
                ToastType = ToastType,
                MessageId = MessageId,
                TimeStamp = new(TimeStamp.Ticks)
            };
        }
    }
}
