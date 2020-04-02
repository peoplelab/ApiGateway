using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ApiGateway.LoginParsers
{
	/// <summary>
	/// Json login parser.
	/// Login service is in json format. 
	/// So it's necessary to convert messages from xml to json and viceversa.
	/// </summary>
	public abstract class Xml2Json_Parser : ILoginParser
	{
		// fields
		protected string _requestID = "";         // request id

		// consts
		protected const string XSD_LOGINPATH = "login.xsd";


		// properties 
		public string RequestID
		{
			get
			{
				return this._requestID;
			}
		}


		#region Abstract Methods 

		/// <summary>
		/// Set body of request converting (source) xml format to (destination) json format.
		/// </summary>
		/// <param name="xml_raw">original request body in xml format.</param>
		/// <returns>request body in json format</returns>
		protected abstract StringBuilder setRequestBody(string xml_raw);
		/// <summary>
		/// Set body of response converting (source) json format in (destination) xml format.
		/// </summary>
		/// <param name="json_raw">response body in json format</param>
		/// <param name="requestID">request id</param>
		/// <returns>response body in xml format.</returns>
		protected abstract StringBuilder setResponseBody(string json_raw, string requestID);
		/// <summary>
		/// Set body of "error" response converting (source) json format in (destination) xml format.
		/// </summary>
		/// <param name="json_raw">response body in json format</param>
		/// <param name="httpcode">http response code</param>
		/// <param name="requestID">request id</param>
		/// <returns>response body in xml format.</returns>
		protected abstract StringBuilder setResponseBodyError(string json_raw, int httpcode, string requestID);

		#endregion

		#region Public Methods 

		/// <summary>
		/// Set body request from xml to json.
		/// </summary>
		/// <param name="xml">request in xml format</param>
		/// <param name="error_message">optional error message (only if an error occurs)</param>
		/// <returns>request in json format</returns>
		public string SetRequest(string xml, out string error_message)
		{
			error_message = "";
			if (xml.Length == 0) return "";

			// xml validation
			RawDataRequest oRawData = null;
			try
			{
				oRawData = Helper.JsonLoader.LoadFromString<RawDataRequest>(xml);
			}
			catch
			{
				// an error occurs, rawdata not parsable to json
				return "";
			}

			System.Text.StringBuilder sbJson = new StringBuilder();
			string xml_data = oRawData.data;

			if (!this.validaXML(xml_data, out error_message)) return "";

			// conversion xml to json

			sbJson = this.setRequestBody(xml_data);

			return sbJson.ToString();
		}
		/// <summary>
		/// Wrap response from json to xml.
		/// </summary>
		/// <param name="responseMessage">response in json format</param>
		/// <param name="requestID">original request id</param>
		/// <returns>response message in xml format</returns>
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
		/// <summary>
		/// Wrap error response from json to xml.
		/// </summary>
		/// <param name="responseMessage">response in json format</param>
		/// <param name="requestID">original request id</param>
		/// <returns>response in xml format</returns>
		public async Task<HttpResponseMessage> WrapResponseError(HttpResponseMessage responseMessage, string requestID)
		{

			string json_raw = await responseMessage.Content.ReadAsStringAsync();

			string soap_envelope = this.addSoapEnvelope(this.setResponseBodyError(json_raw, (int)responseMessage.StatusCode, requestID));

			responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml");

			responseMessage.StatusCode = System.Net.HttpStatusCode.OK;

			return responseMessage;
		}
		/// <summary>
		/// Create a response error based on an error message.
		/// </summary>
		/// <param name="error_message">error message in a string format</param>
		/// <param name="requestID">original request id.</param>
		/// <returns></returns>
		public HttpResponseMessage CreateResponseError(string error_message, string requestID)
		{
			HttpResponseMessage responseMessage = new HttpResponseMessage();

			string soap_envelope = this.addSoapEnvelope(this.setResponseBodyError(error_message, requestID));

			responseMessage.Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml");

			responseMessage.StatusCode = System.Net.HttpStatusCode.OK;

			return responseMessage;
		}

		#endregion

		#region Private Methods 

		private string addSoapEnvelope(StringBuilder sbxmlOut)
		{
			StringBuilder sb1 = new StringBuilder();

			RawDataResponse rawdata = new RawDataResponse();
			rawdata.d = sbxmlOut.ToString();
			string json = Helper.JsonLoader.Serialize<RawDataResponse>(rawdata);

			return json;
		}
		/// <summary>
		/// Set error response based on an error message.
		/// </summary>
		/// <param name="error_response">error response in a string format.</param>
		/// <param name="requestID">request original id</param>
		/// <returns></returns>
		private StringBuilder setResponseBodyError(string error_response, string requestID)
		{
			
			ApiGateway.Helper.Xml.Login.Response response = new Helper.Xml.Login.Response();
			response.ID = requestID;
			Helper.Xml.Login.ResponseResult result = new Helper.Xml.Login.ResponseResult();
			result.Codice = (int)System.Net.HttpStatusCode.BadRequest;
			result.Descrizione = error_response;
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
		/// <param name="error_message">optional error message (only if an error occurs)</param>
		/// <returns>risultato validazione</returns>
		protected bool validaXML(string strxmlIN, out string error_message)
		{
			XmlDocument xmlIN = new XmlDocument();
			error_message = "";
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
					error_message = "XML Request is not well-formed.";
				}

				if (result)
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
						error_message = validMsg.ToString();
					}
				}
			}
			else
			{
				result = false;
				error_message = "XML Request is null.";
			}

			return result;
		}

		#endregion
	}
}
