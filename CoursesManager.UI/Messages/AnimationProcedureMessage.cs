using CoursesManager.MVVM.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.UI.Messages
{
    public class AnimationProcedureMessage :BaseMessage<AnimationProcedureMessage>
    {
        public bool animationTrigger {  get; set; }
        public AnimationProcedureMessage(bool AnimationTrigger) 
        {
            animationTrigger = AnimationTrigger;
        }

        public override AnimationProcedureMessage Clone()
        {
            return new AnimationProcedureMessage(animationTrigger)
            {
                animationTrigger = animationTrigger,
                MessageId = MessageId,
                TimeStamp = TimeStamp
            };
             
        }
    }
}

