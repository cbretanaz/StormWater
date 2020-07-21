using System;
using System.Security.Permissions;
using System.Runtime.Serialization;
using CoP.Enterprise;

[assembly:SecurityPermissionAttribute(SecurityAction.RequestMinimum, UnmanagedCode=true)]
namespace CoP
{
	#region Exceptions
    [Serializable]
    public class CoPDataException : CoPException
    {
        public CoPDataException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CoPDataException(string sMessage)
            : base(sMessage) { }
        public CoPDataException() { }

        #region Serializeable Code
        public CoPDataException(SerializationInfo info, 
           StreamingContext context):base(info, context) { }
        #endregion Serializeable Code

    }

    [Serializable]
    public class CoPDataConfigurationException : CoPDataException
    {
        public CoPDataConfigurationException(string sMessage,
            Exception innerException) 
            : base(sMessage, innerException) { }
        public CoPDataConfigurationException(string sMessage)
            : base(sMessage) { }
        public CoPDataConfigurationException() { }

        #region Serializeable Code
        public CoPDataConfigurationException(SerializationInfo info,
           StreamingContext context)
            : base(info, context) { }
        #endregion Serializeable Code

    }
    [Serializable]
    public class CoPNoPropertyLiensDataException : CoPMissingDataException
    {
        private string propId;

        public CoPNoPropertyLiensDataException(string sMessage,
            string propertyId, Exception innerException):
             base(sMessage, innerException) { propId = propertyId; }

        public CoPNoPropertyLiensDataException(string sMessage, string propertyId):
             base(sMessage) { propId = propertyId; }

        public CoPNoPropertyLiensDataException(string propertyId) { propId = propertyId; }

        #region Serializeable Code
        public CoPNoPropertyLiensDataException(SerializationInfo info, StreamingContext context):
             base(info, context) { }
        #endregion Serializeable Code

    }
    #endregion Exceptions
}
