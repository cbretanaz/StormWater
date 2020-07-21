using System;
using System.Runtime.Serialization;

namespace CoP.Enterprise
{
	#region Exceptions  
    /// <summary>
    /// General City of Portland Enterprise level Exception
    /// Base class for all City of Portland specific exceptions
    /// </summary>
    [Serializable]
	public class CoPException: ApplicationException
	{
		public CoPException(string sMessage, 
            Exception innerException)
			: base(sMessage,innerException) {}
		public CoPException(string sMessage)
			: base(sMessage) {}
		public CoPException() {}

        #region Serializeable Code
        public CoPException(
           SerializationInfo info, StreamingContext context): 
           base(info, context) { }
        #endregion Serializeable Code
	}
    /// <summary>
    /// Exception generated during attempts to Logon 
    /// to City Of Portland rose domain
    /// </summary>
    [Serializable]
	public class LogonException: CoPException
	{
		public LogonException(string sMessage, 
            Exception innerException)
			: base(sMessage,innerException) {}
		public LogonException(string sMessage)
			: base(sMessage) {}
		public LogonException() {}

        #region Serializeable Code
        public LogonException(
           SerializationInfo info, StreamingContext context): 
           base(info, context) { }
        #endregion Serializeable Code
    
    }
    /// <summary>
    /// General City of Portland Enterprise Security system related level Exception
    /// </summary>
    [Serializable]
    public class SecurityException : CoPException
    {
        public SecurityException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public SecurityException(string sMessage)
            : base(sMessage) { }
        public SecurityException() { }

        #region Serializeable Code
        public SecurityException(
           SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code

    }
    /// <summary>
    /// General City of Portland Enterprise Security system 
    /// Permission-related or access-Control-List (ACL) Exception
    /// </summary>
    [Serializable]
    public class AccessDeniedException : SecurityException
    {
        public AccessDeniedException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public AccessDeniedException(string sMessage)
            : base(sMessage) { }
        public AccessDeniedException() { }

        #region Serializeable Code
        public AccessDeniedException(
           SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code

    }
    /// <summary>
    /// General Operating system Input output exception
    /// </summary>
    [Serializable]
    public class CoPIOException : CoPException
    {
        public CoPIOException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPIOException(string sMessage)
            : base(sMessage) { }
        public CoPIOException() { }

        #region Serializeable Code
        public CoPIOException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }
    /// <summary>
    /// City of Portland Configuration system Exception
    /// </summary>
    [Serializable]
    public class CoPConfigurationException : CoPException
    {
        public CoPConfigurationException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPConfigurationException(string sMessage)
            : base(sMessage) { }
        public CoPConfigurationException() { }

        #region Serializeable Code
        public CoPConfigurationException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }
    /// <summary>
    /// Exception caused by planned outage of WCF Service
    /// </summary>
    [Serializable]
    public class PlannedWcfOutageException : CoPException
    {
        public PlannedWcfOutageException(string sMessage, Exception innerException) :
            base(sMessage, innerException)
        { }
        public PlannedWcfOutageException(string sMessage) : base(sMessage) { }
        public PlannedWcfOutageException() { }
        public PlannedWcfOutageException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }
    }
    /// <summary>
    /// Attempt to access read/write service method during period
    ///  when service has been configured to be read-only
    /// </summary>
    [Serializable]
    public class ReadOnlyException : CoPException
    {
        public ReadOnlyException(string sMessage, Exception innerException) :
            base(sMessage, innerException) { }
        public ReadOnlyException(string sMessage) : base(sMessage) { }
        public ReadOnlyException() { }
        public ReadOnlyException(SerializationInfo info, StreamingContext context) :
            base(info, context) { }
    }

    /// <summary>
    /// Thrown when evaluating list of HourInterval objects and not all intervals are represented
    /// </summary>
    [Serializable]
    public class CoPMissingScheduleItemException : CoPException
    {
        public CoPMissingScheduleItemException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPMissingScheduleItemException(string sMessage)
            : base(sMessage) { }
        public CoPMissingScheduleItemException() { }

        #region Serializeable Code
        public CoPMissingScheduleItemException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CoPMissingDataException : CoPException
    {
        public CoPMissingDataException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPMissingDataException(string sMessage)
            : base(sMessage) { }
        public CoPMissingDataException() { }

        #region Serializeable Code
        public CoPMissingDataException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }
    [Serializable]
    public class CoPMissingFileException : CoPException
    {
        public CoPMissingFileException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPMissingFileException(string sMessage)
            : base(sMessage) { }
        public CoPMissingFileException() { }

        #region Serializeable Code
        public CoPMissingFileException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }

    [Serializable]
    public class CoPInvalidDataException : CoPException
    {
        public CoPInvalidDataException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPInvalidDataException(string sMessage)
            : base(sMessage) { }
        public CoPInvalidDataException() { }

        #region Serializeable Code
        public CoPInvalidDataException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }

    [Serializable]
    public class CoPInvalidOperationException : CoPException
    {
        public CoPInvalidOperationException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPInvalidOperationException(string sMessage)
            : base(sMessage) { }
        public CoPInvalidOperationException() { }

        #region Serializeable Code
        public CoPInvalidOperationException(SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }
    [Serializable]
    public class CoPTaskFailureException : CoPException
    {
        public (string subject, string body) Email { get; set; }
        public CoPTaskFailureException(string sMessage,
            string subject, string body,
            Exception innerException)
            : base(sMessage, innerException)
        { Email = (subject, body); }
        public CoPTaskFailureException(string sMessage,
            (string subject, string body) email,
            Exception innerException)
            : base(sMessage, innerException)
        {Email = email;}
        public CoPTaskFailureException(string sMessage)
            : base(sMessage) { }
        public CoPTaskFailureException() { }

        #region Serializeable Code
        public CoPTaskFailureException(SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code
    }
    #endregion Exceptions
}
