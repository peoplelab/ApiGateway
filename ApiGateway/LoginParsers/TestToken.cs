using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace ApiGateway.LoginParsers
{
    /// <summary>
    /// Login parser for TokenAuthenticationWebAPI Authentication.
    /// (test token project)
    /// </summary>
    public class TestToken : Xml2Json_Parser
	{
        /// <summary>
        /// Serializator for json reponse (ok).
        /// </summary>
        public class JsonResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }

        }
        /// <summary>
        /// Serializator for json response (error).
        /// </summary>
        public class JsonErrorResponse
        {
            public string error { get; set; }
            public string error_description { get; set; }
        }
        



        protected override StringBuilder setRequestBody(string xml)
        {
            // in: {data : "<?xml version="1.0" encoding="utf-8" ?><Request ID="1"><Data><Username>Alberto</Username><Password>123456</Password></Data></Request>"}
            // out: username=Alberto&password=123456&grant_type=password

            System.Text.StringBuilder sbJson = new StringBuilder();
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(xml);
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

            sbJson.Append("&grant_type=password");

            return sbJson;
        }
        protected override StringBuilder setResponseBody(string json_raw, string requestID)
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
        protected override StringBuilder setResponseBodyError(string json_raw, int httpcode, string requestID)
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

    }


}
