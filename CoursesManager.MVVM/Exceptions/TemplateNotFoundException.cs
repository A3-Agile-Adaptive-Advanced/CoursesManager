namespace CoursesManager.MVVM.Exceptions
{
    [Serializable]
    public class TemplateNotFoundException : System.Exception
    {
        public TemplateNotFoundException()
            : base("The cache entry is marked as permanent and cannot be overwritten.")
        {
        }

        public TemplateNotFoundException(string message)
            : base(message)
        {
        }

        public TemplateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        protected TemplateNotFoundException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        {
        }
    }
}