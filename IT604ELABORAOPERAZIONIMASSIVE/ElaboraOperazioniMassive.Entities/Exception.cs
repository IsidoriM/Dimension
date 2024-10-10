using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ElaboraOperazioniMassive.Entities
{
    //class Exception
    //{
    /// <summary>
    /// Represents errors that occur during application execution.
    /// </summary>
    [Serializable]
    public class ElaboraOperazioniException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinProvisioningException"/> class.
        /// </summary>
        public ElaboraOperazioniException()
            : base()
        {
            this.ErrorCode = 0;
            this.EventCode = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinProvisioningException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ElaboraOperazioniException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinProvisioningException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ElaboraOperazioniException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinProvisioningException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">A positive numeric value that identifies the error.</param>
        /// <param name="eventCode">A positive numeric value that identifies the event that is the cause of the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ElaboraOperazioniException(string message, int errorCode, int eventCode, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
            this.EventCode = eventCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinProvisioningException" /> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public ElaboraOperazioniException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.ErrorCode = info.GetInt32("ErrorCode");
                this.EventCode = info.GetInt32("EventCode");
            }
        }

        /// <summary>
        /// Gets or Sets a positive numeric value that identifies the error.
        /// </summary>
        /// <value>
        /// A positive numeric value that identifies the error.
        /// </value>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets a positive numeric value that identifies the event that is the cause of the current exception.
        /// </summary>
        /// <value>
        /// A positive numeric value that identifies the event that is the cause of the current exception.
        /// </value>
        public int EventCode { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("ErrorCode", this.ErrorCode);
                info.AddValue("EventCode", this.EventCode);
            }
        }
    }
}
