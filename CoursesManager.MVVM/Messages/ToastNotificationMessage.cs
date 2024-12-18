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

        public ToastNotificationMessage(bool setVisibillity, string notificationText)
        {
            SetVisibillity = setVisibillity;
            NotificationText = notificationText;
        }


        public override ToastNotificationMessage Clone()
        {
            return new ToastNotificationMessage(SetVisibillity, NotificationText)
            {
                SetVisibillity = SetVisibillity,
                NotificationText = NotificationText,
                MessageId = MessageId,
                TimeStamp = new(TimeStamp.Ticks)
            };
        }
    }
}
