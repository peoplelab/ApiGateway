using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApiGateway.LoginParsers
{
    public interface ILoginParser{
        string MakeRequest(string xml);
        Task<HttpResponseMessage> WrapResponse(HttpResponseMessage responseMessage);
        Task<HttpResponseMessage> WrapResponseError(HttpResponseMessage responseMessage);
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

        private string _requestID = "";         // request id

		public string MakeRequest(string xml)
		{
            System.Text.StringBuilder sbJson = new System.Text.StringBuilder();

            sbJson = this.xml2Json(xml);
            sbJson.Append("&grant_type=password");

            return sbJson.ToString();
        }
        public async Task<HttpResponseMessage> WrapResponse(HttpResponseMessage responseMessage)
        {
            // if there is no "Http ok" code, response must be wrapped in an error response
            if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK) return await this.WrapResponseError(responseMessage);


            string json_raw = await responseMessage.Content.ReadAsStringAsync();

            string soap_envelope = this.addSoapEnvelope(this.setResponseBody(json_raw));

            responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml");


            return responseMessage;
        }
        public async Task<HttpResponseMessage> WrapResponseError(HttpResponseMessage responseMessage)
        {
                      
            string json_raw = await responseMessage.Content.ReadAsStringAsync();

            string soap_envelope = this.addSoapEnvelope(this.setResponseBodyError(json_raw, (int) responseMessage.StatusCode));

            responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml");

            responseMessage.StatusCode = System.Net.HttpStatusCode.OK;

            return responseMessage;
        }


        private System.Text.StringBuilder xml2Json(string xml)
        {
            // in: data=<?xml version="1.0" encoding="utf-8" ?><Request ID="1"><Data><Username>Alberto</Username><Password>123456</Password></Data></Request>
            // out: username=Alberto&password=123456&grant_type=password

            if (xml.Length == 0) return new StringBuilder();

            System.Text.StringBuilder sbJson = new StringBuilder();
            string xml_raw = xml.Substring(xml.IndexOf("<"));
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
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb1.Append("<string xmlns=\"http://yeap.local/public/ws\">");
            sb1.Append(sbxmlOut.Replace("<", "&lt;").Replace(">", "&gt;").ToString());
            sb1.Append("</string>");

            return sb1.ToString();
        }
        private StringBuilder setResponseBody(string json_raw)
        {
            JsonResponse deserializedJson = JsonConvert.DeserializeObject<JsonResponse>(json_raw);

            ApiGateway.Helper.Xml.Login.Response response = new Helper.Xml.Login.Response();
            response.ID = this._requestID;
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
        private StringBuilder setResponseBodyError(string json_raw, int httpcode)
        {
            JsonErrorResponse deserializedJson = JsonConvert.DeserializeObject<JsonErrorResponse>(json_raw);

            ApiGateway.Helper.Xml.Login.Response response = new Helper.Xml.Login.Response();
            response.ID = this._requestID;
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
    }


}
