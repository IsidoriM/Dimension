using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace ElaboraOperazioniMassive.Entities
{

    /// <summary>
    /// Represents errors that occur during application execution in the controller layer.
    /// </summary>
    [Serializable]
    public class ElaboraOperazioniControllerException : ElaboraOperazioniException
    {
        private Dictionary<string, object> parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        public ElaboraOperazioniControllerException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ElaboraOperazioniControllerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ElaboraOperazioniControllerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="errorCode">A positive numeric value that identifies the error.</param>
        /// <param name="eventCode">A positive numeric value that identifies the event that is the cause of the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ElaboraOperazioniControllerException(string message, int errorCode, int eventCode, Exception innerException)
            : base(message, errorCode, eventCode, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="query">The query that is the cause of the current exception.</param>
        /// <param name="errorCode">A positive numeric value that identifies the error.</param>
        /// <param name="eventCode">A positive numeric value that identifies the event that is the cause of the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ElaboraOperazioniControllerException(string message, string query, int errorCode, int eventCode, Exception innerException)
            : base(message, errorCode, eventCode, innerException)
        {
            this.Query = query;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="connectionString">The active connection string.</param>
        /// <param name="query">The query that is the cause of the current exception.</param>
        /// <param name="errorCode">A positive numeric value that identifies the error.</param>
        /// <param name="eventCode">A positive numeric value that identifies the event that is the cause of the current exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public ElaboraOperazioniControllerException(string message, string connectionString, string query, int errorCode, int eventCode, Exception innerException)
            : this(message, query, errorCode, eventCode, innerException)
        {
            SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder(connectionString);

            // this.ConnectionInfo = "[" + connBuilder.DataSource + " | " + connBuilder.InitialCatalog + "]";
            this.ConnectionInfo = "[" + connBuilder.DataSource + "]";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PElaboraDecessiControllerException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public ElaboraOperazioniControllerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                this.ConnectionInfo = info.GetString("ConnectionString");
                this.Query = info.GetString("Query");
            }
        }

        /// <summary>
        /// The active connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionInfo { get; set; }

        public Dictionary<string, object> Parameters
        {
            get
            {
                return this.parameters ?? new Dictionary<string, object> { { "SP", this.Query } };
            }

            set
            {
                this.parameters = value;
            }
        }

        /// <summary>
        /// The query that is the cause of the current exception.
        /// </summary>
        /// <value>
        /// The query string.
        /// </value>
        public string Query { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        ///   </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("ConnectionString", this.ConnectionInfo);
                info.AddValue("Query", this.Query);
            }
        }
    }
}

