using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGateway.LoginParsers
{
    /// <summary>
    /// Interface for Login Parsers.
    /// </summary>
    public interface ILoginParser
    {
        /// <summary>
        /// Request identifier.
        /// </summary>
        string RequestID { get; }
        /// <summary>
        /// Request builder (from xml to ...)
        /// </summary>
        /// <param name="xml">xml IN as a string</param>
        /// <param name="error_message">optional error message (only if an error occurs)</param>
        /// <returns>formatted request string</returns>
        string SetRequest(string xml, out string error_message);
        /// <summary>
        /// OK Response builder (from original format to xml "classic" format)
        /// </summary>
        /// <param name="responseMessage">original response</param>
        /// <param name="requestID">request identifier</param>
        /// <returns>xml formatted response</returns>
        Task<HttpResponseMessage> WrapResponse(HttpResponseMessage responseMessage, string requestID);
        /// <summary>
        /// Error Response builder (from original format to xml "classic" format)
        /// </summary>
        /// <param name="responseMessage">original response</param>
        /// <param name="requestID">request identifier</param>
        /// <returns>xml formatted response</returns>
        Task<HttpResponseMessage> WrapResponseError(HttpResponseMessage responseMessage, string requestID);
        /// <summary>
        /// Create an error response based on error_message.
        /// </summary>
        /// <param name="error_message">error message in string format.</param>
        /// <param name="requestID">request original id</param>
        /// <returns></returns>
        HttpResponseMessage CreateResponseError(string error_message, string requestID);

    }

    /// <summary>
    /// Serializator for Login Request.
    /// </summary>
    public class RawDataRequest
    {
        public string data { get; set; }
    }
    /// <summary>
    /// Serializator for Login Response.
    /// </summary>
    public class RawDataResponse
    {
        public string d { get; set; }
    }

}
