using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApiGateway.LoginParsers
{
    public interface ILoginParser{
        string RequestID { get; }
        string MakeRequest(string xml);
        Task<HttpResponseMessage> WrapResponse(HttpResponseMessage responseMessage, string requestID);
        Task<HttpResponseMessage> WrapResponseError(HttpResponseMessage responseMessage, string requestID);
    }
    public class RawDataRequest{
        public string data{ get; set; }
    }
    public class RawDataResponse
    {
        public string d { get; set; }
    }

    /// <summary>
    /// Login parser for TokenAuthenticationWebAPI Authentication.
    /// (test token project)
    /// </summary>
    public class TestToken : ILoginParser
	{
        public class JsonResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }

        }
        public class JsonErrorResponse
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }
        
        public string RequestID
        {
            get
            {
                return this._requestID;
            }
        }

        private string _requestID = "";         // request id

        private const int NOERROR = 0;
        private const int XMLNULLO = 20;
        private const int XML_NONVALIDO = 21;
        private string XSD_LOGINPATH = "login.xsd";


        public string MakeRequest(string xml)
		{
            System.Text.StringBuilder sbJson = new System.Text.StringBuilder();

            sbJson = this.xml2Json(xml);
            sbJson.Append("&grant_type=password");

            return sbJson.ToString();
        }
        public async Task<HttpResponseMessage> WrapResponse(HttpResponseMessage responseMessage, string requestID)
        {
            // if there is no "Http ok" code, response must be wrapped in an error response
            if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK) return await this.WrapResponseError(responseMessage, requestID);


            string json_raw = await responseMessage.Content.ReadAsStringAsync();

           string soap_envelope = this.addSoapEnvelope(this.setResponseBody(json_raw, requestID));

            //responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml");
            responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "application/json");


            return responseMessage;
        }
        public async Task<HttpResponseMessage> WrapResponseError(HttpResponseMessage responseMessage, string requestID)
        {
                      
            string json_raw = await responseMessage.Content.ReadAsStringAsync();

            string soap_envelope = this.addSoapEnvelope(this.setResponseBodyError(json_raw, (int) responseMessage.StatusCode, requestID));

            responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml");

            responseMessage.StatusCode = System.Net.HttpStatusCode.OK;

            return responseMessage;
        }


        private System.Text.StringBuilder xml2Json(string rawdata)
        {
            // in: {data : "<?xml version="1.0" encoding="utf-8" ?><Request ID="1"><Data><Username>Alberto</Username><Password>123456</Password></Data></Request>"}
            // out: username=Alberto&password=123456&grant_type=password

            if (rawdata.Length == 0) return new StringBuilder();

            RawDataRequest oRawData = null;
            try
            {
                oRawData = Helper.JsonLoader.LoadFromString<RawDataRequest>(rawdata);
            }
            catch
            {   
                // an error occurs, rawdata not parsable to json
                return new StringBuilder();
            }

            System.Text.StringBuilder sbJson = new StringBuilder();
            string xml_raw = oRawData.data;

            if (!this.validaXML(xml_raw)) return new StringBuilder();

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(xml_raw);
            bool first = true;

            System.Xml.XmlNode request_node = xmlDoc.SelectSingleNode("/Request");
            if (request_node != null)
            {
                this._requestID = request_node.Attributes["ID"].InnerText;
            }

            System.Xml.XmlNode data_node = xmlDoc.SelectSingleNode("/Request/Data");
            if (data_node != null)
            {
                foreach (System.Xml.XmlNode node in data_node.ChildNodes)
                {
                    if (!first)
                    {
                        sbJson.Append("&");
                    }
                    first = false;
                    sbJson.Append(node.Name).Append("=").Append(node.InnerText);
                }
            }

            return sbJson;
        }
        private string addSoapEnvelope(StringBuilder sbxmlOut)
        {
            //StringBuilder sb1 = new StringBuilder();

            //RawDataResponse rawdata = new RawDataResponse();
            //rawdata.d = sbxmlOut.ToString();
            //string json = Helper.JsonLoader.Serialize<RawDataResponse>(rawdata);   

            @string rawdata = new @string();
            rawdata.Value = sbxmlOut.ToString();
            System.Text.StringBuilder sbxml = new System.Text.StringBuilder();
            XmlSerializer serOUT = new XmlSerializer(typeof(@string));
            StringWriter rdr_out = new StringWriter(sbxml);
            serOUT.Serialize(rdr_out, rawdata);

            return sbxml.ToString();

            //return json;
        }
        private StringBuilder setResponseBody(string json_raw, string requestID)
        {
            JsonResponse deserializedJson = JsonConvert.DeserializeObject<JsonResponse>(json_raw);

            ApiGateway.Helper.Xml.Login.Response response = new Helper.Xml.Login.Response();
            response.ID = requestID;
            Helper.Xml.Login.ResponseResult result = new Helper.Xml.Login.ResponseResult();
            result.Codice = 0;
            result.Descrizione = "xxxxxxxxxxx"; // andrà tolto
            ApiGateway.Helper.Xml.Login.ResponseData data = new Helper.Xml.Login.ResponseData();

            data.AccessToken = deserializedJson.access_token;
            data.Expires = deserializedJson.expires_in;
            data.TokenType = deserializedJson.token_type;
            response.Result = result;
            response.Data = data;

            System.Text.StringBuilder sbxmlOut = new System.Text.StringBuilder();
            XmlSerializer serOUT = new XmlSerializer(typeof(ApiGateway.Helper.Xml.Login.Response));
            StringWriter rdr_out = new StringWriter(sbxmlOut);
            serOUT.Serialize(rdr_out, response);

            return sbxmlOut;
        }
        private StringBuilder setResponseBodyError(string json_raw, int httpcode, string requestID)
        {
            JsonErrorResponse deserializedJson = JsonConvert.DeserializeObject<JsonErrorResponse>(json_raw);

            ApiGateway.Helper.Xml.Login.Response response = new Helper.Xml.Login.Response();
            response.ID = requestID;
            Helper.Xml.Login.ResponseResult result = new Helper.Xml.Login.ResponseResult();
            result.Codice = httpcode;
            result.Descrizione = deserializedJson.error + ((deserializedJson.error_description?.Length > 0) ? " - " : "") + deserializedJson.error_description;
            ApiGateway.Helper.Xml.Login.ResponseData data = null;

            response.Result = result;
            response.Data = data;

            System.Text.StringBuilder sbxmlOut = new System.Text.StringBuilder();
            XmlSerializer serOUT = new XmlSerializer(typeof(ApiGateway.Helper.Xml.Login.Response));
            StringWriter rdr_out = new StringWriter(sbxmlOut);
            serOUT.Serialize(rdr_out, response);

            return sbxmlOut;
        }

        /// <summary>
        /// Validazione XML con schema.
        /// </summary>
        /// <param name="strxmlIN">stringa corrispondente al file xml</param>
        /// <param name="schemapath">percorso dello schema xsd</param>
        /// <returns>risultato validazione</returns>
        protected bool validaXML(string strxmlIN)
        {
            XmlDocument xmlIN = new XmlDocument();
            bool result = true;

            if (strxmlIN.Length > 0)
            {
                // leggo l'xml...

                try
                {
                    xmlIN.LoadXml(strxmlIN);
                }
                catch
                {
                    result = false;
                }

                if (xmlIN != null)
                {
                    // lo valido con lo schema xsd ...
                    // string schemafile = HttpContext.Current.ApplicationInstance.Server.MapPath(schemapath);
                    string schemafile = XSD_LOGINPATH;
                    XmlTextReader schemaReader = new XmlTextReader(schemafile);
                    System.Text.StringBuilder validMsg = new StringBuilder();
                    XmlSchema schema = XmlSchema.Read(schemaReader, (sender, args) =>
                    {
                        if (args.Severity == XmlSeverityType.Error)
                            validMsg.Append("ERROR:").Append(
                                args.Message).Append("\n");
                    });
                    xmlIN.Schemas.Add(schema);
                    xmlIN.Validate((sender, args) =>
                    {
                        if (args.Severity == XmlSeverityType.Error)
                            validMsg.Append("ERROR:").Append(args.Message).Append("\n");
                    });

                    if (validMsg.Length > 0)
                    {
                        // errori di validazione
                        result = false;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

    }


	[System.SerializableAttribute()]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
	public partial class @string
	{

        [System.Xml.Serialization.XmlTextAttribute]
        public string Value { get; set; }
    }



}
