﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// Il codice sorgente è stato generato automaticamente da Microsoft.VSDesigner, versione 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace ElaboraOperazioniMassive.Servizi.ICONAMAIL {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3752.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="InvioSoap", Namespace="http://tempuri.org/")]
    public partial class Invio : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private LoginHeader loginHeaderValueField;
        
        private System.Threading.SendOrPostCallback InvioEmailOperationCompleted;
        
        private System.Threading.SendOrPostCallback InvioEmail_01OperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Invio() {
            this.Url = global::ElaboraOperazioniMassive.Servizi.Properties.Settings.Default.ElaboraOperazioniMassive_Servizi_ICONAMAIL_Invio;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public LoginHeader LoginHeaderValue {
            get {
                return this.loginHeaderValueField;
            }
            set {
                this.loginHeaderValueField = value;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event InvioEmailCompletedEventHandler InvioEmailCompleted;
        
        /// <remarks/>
        public event InvioEmail_01CompletedEventHandler InvioEmail_01Completed;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("LoginHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/InvioEmail", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public InvioMail_OUTPUT InvioEmail(InvioMail_INPUT input) {
            object[] results = this.Invoke("InvioEmail", new object[] {
                        input});
            return ((InvioMail_OUTPUT)(results[0]));
        }
        
        /// <remarks/>
        public void InvioEmailAsync(InvioMail_INPUT input) {
            this.InvioEmailAsync(input, null);
        }
        
        /// <remarks/>
        public void InvioEmailAsync(InvioMail_INPUT input, object userState) {
            if ((this.InvioEmailOperationCompleted == null)) {
                this.InvioEmailOperationCompleted = new System.Threading.SendOrPostCallback(this.OnInvioEmailOperationCompleted);
            }
            this.InvokeAsync("InvioEmail", new object[] {
                        input}, this.InvioEmailOperationCompleted, userState);
        }
        
        private void OnInvioEmailOperationCompleted(object arg) {
            if ((this.InvioEmailCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.InvioEmailCompleted(this, new InvioEmailCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("LoginHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/InvioEmail_01", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public InvioMail_OUTPUT InvioEmail_01(InvioMail_INPUT_01 input) {
            object[] results = this.Invoke("InvioEmail_01", new object[] {
                        input});
            return ((InvioMail_OUTPUT)(results[0]));
        }
        
        /// <remarks/>
        public void InvioEmail_01Async(InvioMail_INPUT_01 input) {
            this.InvioEmail_01Async(input, null);
        }
        
        /// <remarks/>
        public void InvioEmail_01Async(InvioMail_INPUT_01 input, object userState) {
            if ((this.InvioEmail_01OperationCompleted == null)) {
                this.InvioEmail_01OperationCompleted = new System.Threading.SendOrPostCallback(this.OnInvioEmail_01OperationCompleted);
            }
            this.InvokeAsync("InvioEmail_01", new object[] {
                        input}, this.InvioEmail_01OperationCompleted, userState);
        }
        
        private void OnInvioEmail_01OperationCompleted(object arg) {
            if ((this.InvioEmail_01Completed != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.InvioEmail_01Completed(this, new InvioEmail_01CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/", IsNullable=false)]
    public partial class LoginHeader : System.Web.Services.Protocols.SoapHeader {
        
        private string userNameField;
        
        private string passwordField;
        
        private System.Xml.XmlAttribute[] anyAttrField;
        
        /// <remarks/>
        public string UserName {
            get {
                return this.userNameField;
            }
            set {
                this.userNameField = value;
            }
        }
        
        /// <remarks/>
        public string Password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr {
            get {
                return this.anyAttrField;
            }
            set {
                this.anyAttrField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class Destinatari_01 {
        
        private tipoDestinatari_01 tipoField;
        
        private string destinatarioField;
        
        /// <remarks/>
        public tipoDestinatari_01 tipo {
            get {
                return this.tipoField;
            }
            set {
                this.tipoField = value;
            }
        }
        
        /// <remarks/>
        public string destinatario {
            get {
                return this.destinatarioField;
            }
            set {
                this.destinatarioField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public enum tipoDestinatari_01 {
        
        /// <remarks/>
        to,
        
        /// <remarks/>
        cc,
        
        /// <remarks/>
        bcc,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class InvioMail_INPUT_01 {
        
        private string keygestField;
        
        private string mittenteField;
        
        private string subjectField;
        
        private string bodyField;
        
        private string sedeField;
        
        private string codiceApplicazioneField;
        
        private bool allegatiPresentiField;
        
        private bool checkEsitoInvioField;
        
        private bool pECField;
        
        private Destinatari_01[] destinatariField;
        
        private Allegati[] allegatiField;
        
        /// <remarks/>
        public string keygest {
            get {
                return this.keygestField;
            }
            set {
                this.keygestField = value;
            }
        }
        
        /// <remarks/>
        public string mittente {
            get {
                return this.mittenteField;
            }
            set {
                this.mittenteField = value;
            }
        }
        
        /// <remarks/>
        public string subject {
            get {
                return this.subjectField;
            }
            set {
                this.subjectField = value;
            }
        }
        
        /// <remarks/>
        public string body {
            get {
                return this.bodyField;
            }
            set {
                this.bodyField = value;
            }
        }
        
        /// <remarks/>
        public string sede {
            get {
                return this.sedeField;
            }
            set {
                this.sedeField = value;
            }
        }
        
        /// <remarks/>
        public string codiceApplicazione {
            get {
                return this.codiceApplicazioneField;
            }
            set {
                this.codiceApplicazioneField = value;
            }
        }
        
        /// <remarks/>
        public bool allegatiPresenti {
            get {
                return this.allegatiPresentiField;
            }
            set {
                this.allegatiPresentiField = value;
            }
        }
        
        /// <remarks/>
        public bool checkEsitoInvio {
            get {
                return this.checkEsitoInvioField;
            }
            set {
                this.checkEsitoInvioField = value;
            }
        }
        
        /// <remarks/>
        public bool PEC {
            get {
                return this.pECField;
            }
            set {
                this.pECField = value;
            }
        }
        
        /// <remarks/>
        public Destinatari_01[] destinatari {
            get {
                return this.destinatariField;
            }
            set {
                this.destinatariField = value;
            }
        }
        
        /// <remarks/>
        public Allegati[] allegati {
            get {
                return this.allegatiField;
            }
            set {
                this.allegatiField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class Allegati {
        
        private string nomeField;
        
        private byte[] allegatoField;
        
        /// <remarks/>
        public string nome {
            get {
                return this.nomeField;
            }
            set {
                this.nomeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] allegato {
            get {
                return this.allegatoField;
            }
            set {
                this.allegatoField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class InvioMail_OUTPUT {
        
        private ResultType cD_RCField;
        
        private string errorDescriptionField;
        
        private string protocolloField;
        
        private System.DateTime dataInserimentoField;
        
        /// <remarks/>
        public ResultType CD_RC {
            get {
                return this.cD_RCField;
            }
            set {
                this.cD_RCField = value;
            }
        }
        
        /// <remarks/>
        public string errorDescription {
            get {
                return this.errorDescriptionField;
            }
            set {
                this.errorDescriptionField = value;
            }
        }
        
        /// <remarks/>
        public string protocollo {
            get {
                return this.protocolloField;
            }
            set {
                this.protocolloField = value;
            }
        }
        
        /// <remarks/>
        public System.DateTime dataInserimento {
            get {
                return this.dataInserimentoField;
            }
            set {
                this.dataInserimentoField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public enum ResultType {
        
        /// <remarks/>
        IM,
        
        /// <remarks/>
        ER,
        
        /// <remarks/>
        PK,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class Destinatari {
        
        private tipoDestinatari tipoField;
        
        private string destinatarioField;
        
        /// <remarks/>
        public tipoDestinatari tipo {
            get {
                return this.tipoField;
            }
            set {
                this.tipoField = value;
            }
        }
        
        /// <remarks/>
        public string destinatario {
            get {
                return this.destinatarioField;
            }
            set {
                this.destinatarioField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public enum tipoDestinatari {
        
        /// <remarks/>
        to,
        
        /// <remarks/>
        cc,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3752.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class InvioMail_INPUT {
        
        private string keygestField;
        
        private string mittenteField;
        
        private string subjectField;
        
        private string bodyField;
        
        private string sedeField;
        
        private string codiceApplicazioneField;
        
        private bool allegatiPresentiField;
        
        private bool checkEsitoInvioField;
        
        private bool pECField;
        
        private Destinatari[] destinatariField;
        
        private Allegati[] allegatiField;
        
        /// <remarks/>
        public string keygest {
            get {
                return this.keygestField;
            }
            set {
                this.keygestField = value;
            }
        }
        
        /// <remarks/>
        public string mittente {
            get {
                return this.mittenteField;
            }
            set {
                this.mittenteField = value;
            }
        }
        
        /// <remarks/>
        public string subject {
            get {
                return this.subjectField;
            }
            set {
                this.subjectField = value;
            }
        }
        
        /// <remarks/>
        public string body {
            get {
                return this.bodyField;
            }
            set {
                this.bodyField = value;
            }
        }
        
        /// <remarks/>
        public string sede {
            get {
                return this.sedeField;
            }
            set {
                this.sedeField = value;
            }
        }
        
        /// <remarks/>
        public string codiceApplicazione {
            get {
                return this.codiceApplicazioneField;
            }
            set {
                this.codiceApplicazioneField = value;
            }
        }
        
        /// <remarks/>
        public bool allegatiPresenti {
            get {
                return this.allegatiPresentiField;
            }
            set {
                this.allegatiPresentiField = value;
            }
        }
        
        /// <remarks/>
        public bool checkEsitoInvio {
            get {
                return this.checkEsitoInvioField;
            }
            set {
                this.checkEsitoInvioField = value;
            }
        }
        
        /// <remarks/>
        public bool PEC {
            get {
                return this.pECField;
            }
            set {
                this.pECField = value;
            }
        }
        
        /// <remarks/>
        public Destinatari[] destinatari {
            get {
                return this.destinatariField;
            }
            set {
                this.destinatariField = value;
            }
        }
        
        /// <remarks/>
        public Allegati[] allegati {
            get {
                return this.allegatiField;
            }
            set {
                this.allegatiField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3752.0")]
    public delegate void InvioEmailCompletedEventHandler(object sender, InvioEmailCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3752.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class InvioEmailCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal InvioEmailCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public InvioMail_OUTPUT Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((InvioMail_OUTPUT)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3752.0")]
    public delegate void InvioEmail_01CompletedEventHandler(object sender, InvioEmail_01CompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.3752.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class InvioEmail_01CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal InvioEmail_01CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public InvioMail_OUTPUT Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((InvioMail_OUTPUT)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591