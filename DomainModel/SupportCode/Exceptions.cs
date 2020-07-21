using System;
using System.Runtime.Serialization;

namespace BES.SWMM.PAC
{
    public class PACSizingException: ApplicationException
    {
        #region ctor/Factories
        public PACSizingException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public PACSizingException(string sMessage)
            : base(sMessage) { }
        public PACSizingException() { }
        #endregion ctor/Factories

        #region Serializeable Code
        public PACSizingException(
            SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code
    }

    #region DataExceptions
    public class PACDataException : PACSizingException
    {
        #region ctor/Factories
        public PACDataException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public PACDataException(string sMessage)
            : base(sMessage) { }
        public PACDataException() { }
        #endregion ctor/Factories

        #region Serializeable Code
        public PACDataException(
            SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code
    }
    public class WrongFacilityException : PACDataException
    {
        #region Properties
        public Type ActualType { get; set; }
        public Type ExpectedType { get; set; }
        #endregion Properties

        #region ctor/Factories
        public WrongFacilityException(
            Type actual, Type expected,
            string sMessage, Exception innerException)
            : base(sMessage, innerException)
        {
            ActualType = actual;
            ExpectedType = expected;
        }

        public WrongFacilityException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public WrongFacilityException(string sMessage)
            : base(sMessage) { }
        public WrongFacilityException() { }
        #endregion ctor/Factories

        #region Serializeable Code
        public WrongFacilityException(
            SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }
        #endregion Serializeable Code
    }
    public class PACInputDataException: PACDataException
    {
        #region Properties
        public Type DataType { get; set; }
        public string MissingParameter { get; set; }
        #endregion Properties

        #region ctor/Factories
        public PACInputDataException(Type typ,
            string sMessage, string missParam,
            Exception innerException = null)
            : base(sMessage, innerException)
        { DataType = typ; MissingParameter = missParam; }
        public PACInputDataException(Type typ, 
            string sMessage, Exception innerException = null)
            : base(sMessage, innerException)
        { DataType = typ; }

        public PACInputDataException(
            string sMessage, string missParam = null,
            Exception innerException = null)
            : base(sMessage, innerException) 
        { MissingParameter = missParam; }
        public PACInputDataException(string sMessage)
            : base(sMessage) { }
        public PACInputDataException() { }
        #endregion ctor/Factories

        #region Serializeable Code
        public PACInputDataException(
            SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }
        #endregion Serializeable Code
    }
    public class ParameterOutOfRangeException : PACDataException
    {
        #region Properties
        public object ActualValue { get; set; }
        #endregion Properties

        #region ctor/Factories
        public ParameterOutOfRangeException(object val,
            string sMessage, Exception innerException = null)
            : base(sMessage, innerException)
        { ActualValue = val; }

        public ParameterOutOfRangeException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public ParameterOutOfRangeException(string sMessage)
            : base(sMessage) { }
        public ParameterOutOfRangeException() { }
        #endregion ctor/Factories

        #region Serializeable Code
        public ParameterOutOfRangeException(
            SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code
    }
    #endregion DataExceptions

    #region CalculationExceptions
    public class CalculationException : PACSizingException
    {
        #region ctor/Factories
        public CalculationException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException) { }
        public CalculationException(string sMessage)
            : base(sMessage) { }
        public CalculationException() { }
        #endregion ctor/Factories

        #region Serializeable Code
        public CalculationException(
            SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code
    }
    #endregion CalculationExceptions

    public class ValidationException : CalculationException
    {
        public DataControl Control { get; set; }

        #region ctor/Factories
        public ValidationException(DataControl ctrl,
            string sMessage, Exception innerException)
            : base(sMessage, innerException)
        { Control = ctrl; }
        private ValidationException(DataControl ctrl,
            string sMessage) : base(sMessage) { Control = ctrl; }
        private ValidationException(string sMessage,
            Exception innerException)
            : base(sMessage, innerException)
        { Control = DataControl.NA; }
        private ValidationException(string sMessage) : base(sMessage) 
        { Control = DataControl.NA; }
        private ValidationException() { Control = DataControl.NA; }
        // --------------------------------------------------------
        public static ValidationException Default => new ValidationException();
        public static ValidationException Make(string sMessage)
        { return new ValidationException(sMessage); }
        public static ValidationException Make(
            string sMessage, Exception innerException)
        { return new ValidationException(sMessage, innerException); }
        public static ValidationException Make(
            DataControl ctrl, string sMessage)
        { return new ValidationException(ctrl, sMessage); }
        #endregion ctor/Factories

        #region Serializeable Code
        public ValidationException(
            SerializationInfo info, StreamingContext context) :
            base(info, context) { }
        #endregion Serializeable Code
    }
}