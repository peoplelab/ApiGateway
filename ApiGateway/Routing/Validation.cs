using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiGateway.Routing
{
    public class Validation
    {
        private EntityRoutes.AuthenticationRoute _authRoute;
        private static HttpClient client = new HttpClient();

        public Validation(List<EntityRoutes.AuthenticationRoute> routes, byte loginService)
        {
            foreach (EntityRoutes.AuthenticationRoute route in routes){
                if (route.LoginService == loginService) { this._authRoute = route; break; }
            }
            
        }

        /// <summary>
        /// Send a request without the content body (only with token header).
        /// Only for authorization purposes.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRequest(string token)
        {
            HttpResponseMessage response;

            if (this._authRoute == null)
            {
                response = new HttpResponseMessage
                {
                    //StatusCode = HttpStatusCode.NotFound,

                    Content = new StringContent("pppp", Encoding.UTF8, "text/xml"),

                    StatusCode = System.Net.HttpStatusCode.NotFound

                };
                return response;
            }


            string requestContent = "";

            using (var newRequest = new HttpRequestMessage(new HttpMethod("GET"), this._authRoute.Uri))
            {
                newRequest.Headers.Add("Authorization", token);

                newRequest.Content = new StringContent(requestContent);
                response = await client.SendAsync(newRequest);
            }

            return response;
        }

    }
}
