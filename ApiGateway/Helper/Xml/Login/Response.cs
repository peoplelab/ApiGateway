using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Helper.Xml.Login
{
    /// <summary>
    /// Base Response for services.
    /// </summary>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Response
    {
        protected ResponseResult resultField;
        protected ResponseData dataField;
        protected string idField;

        /// <summary>
        /// Result Node.
        /// </summary>
        public ResponseResult Result
        {
            get
            {
                return this.resultField;
            }
            set
            {
                this.resultField = value;
            }
        }
        /// <summary>
        /// Data Node
        /// </summary>
        public ResponseData Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
        /// <summary>
        ///  Response id attribute (copy from request id...)
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <summary>
    /// Response RESULT Node.
    /// </summary>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class ResponseResult
    {

        protected int _codice = 0;
        protected string _descrizione = "";

        public int Codice
        {
            get
            {
                return this._codice;
            }
            set
            {
                this._codice = value;
            }
        }
        public string Descrizione
        {
            get
            {
                return this._descrizione;
            }
            set
            {
                this._descrizione = value;
            }
        }
    }

    /// <summary>
    /// Response DATA node.
    /// </summary>
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class ResponseData
    {
        private string _accessToken = "";
        private string _code = "";
        private string _nome = "";
        private string _cognome = "";
        private string _ruolo = "";
        private string _customer = "";
        private int _expires = 0;

        public string AccessToken
        {
            get
            {
                return this._accessToken;
            }
            set
            {
                this._accessToken = value;
            }
        }
        public string Code
        {
            get
            {
                return this._code;
            }
            set
            {
                this._code = value;
            }
        }
        public string Nome
        {
            get
            {
                return this._nome;
            }
            set
            {
                this._nome = value;
            }
        }
        public string Cognome
        {
            get
            {
                return this._cognome;
            }
            set
            {
                this._cognome = value;
            }
        }
        public string Ruolo
        {
            get
            {
                return this._ruolo;
            }
            set
            {
                this._ruolo = value;
            }
        }
        public string Customer
        {
            get
            {
                return this._customer;
            }
            set
            {
                this._customer = value;
            }
        }
        public int Expires
        {
            get
            {
                return this._expires;
            }
            set
            {
                this._expires = value;
            }
        }

    }
}
