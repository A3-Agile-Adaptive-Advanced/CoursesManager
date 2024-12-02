using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.MVVM.Exceptions
{
    [Serializable]
    public class CantBeOverwrittenException : System.Exception
    {
        public CantBeOverwrittenException()
            : base("The cache entry is marked as permanent and cannot be overwritten.")
        {
        }

        public CantBeOverwrittenException(string message)
            : base(message)
        {
        }

        public CantBeOverwrittenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected CantBeOverwrittenException(System.Runtime.Serialization.SerializationInfo info,
                                             System.Runtime.Serialization.StreamingContext context)
        {
        }
    }
}


