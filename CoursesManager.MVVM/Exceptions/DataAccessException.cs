namespace CoursesManager.MVVM.Exceptions
{
    [Serializable]
    public class DataAccessException : System.Exception
    {
        public DataAccessException()
            : base("Something went wrong in the DataAccess")
        {
        }

        public DataAccessException(string message)
            : base(message)
        {
        }

        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected DataAccessException(System.Runtime.Serialization.SerializationInfo info,
                                             System.Runtime.Serialization.StreamingContext context)
        {
        }
    }
}


