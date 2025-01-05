using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.MVVM.Messages
{
    public class StudentPaymentUpdatedMessage : BaseMessage<StudentPaymentUpdatedMessage>
    {
        public override StudentPaymentUpdatedMessage Clone()
        {
            return new StudentPaymentUpdatedMessage
            {
                MessageId = MessageId,
                TimeStamp = new(TimeStamp.Ticks)
            };
        }
    }
}