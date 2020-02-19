using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApiGateway.Routing
{
    public class Destination
    {
        private EntityRoutes.Route _configRoute;
        private static HttpClient client = new HttpClient();
        private LoginParsers.ILoginParser _loginParser = null;



        public Destination(EntityRoutes.Route route)
        {
            this._configRoute = route;

            switch (this._configRoute.LoginService){
                case 1: //  test token 
                    this._loginParser = new LoginParsers.TestToken();
                    break;
            }
        }

        private Destination()
        {
        }




        public async Task<HttpResponseMessage> SendRequest(HttpRequest request)
        {
            HttpResponseMessage response;
            string requestContent = "";

            // setting request content
            Stream receiveStream = request.Body;
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            requestContent = readStream.ReadToEnd();


            // if request is a "login" request, it must be converted in a "destination" specific format because login service is not handled by us
            if (this._configRoute.LoginService > 0)
            {
                requestContent = this._loginParser.MakeRequest(requestContent);
            }
            

            using (var newRequest = new HttpRequestMessage(new HttpMethod(this._configRoute.Destination.Method), this._configRoute.Destination.Uri))
            {
                if (request.Headers["Authorization"] != (object)null)
                    newRequest.Headers.Add("Authorization", request.Headers["Authorization"].ToString());

                newRequest.Content = new StringContent(requestContent, Encoding.UTF8, request.ContentType);
                response = await client.SendAsync(newRequest);
            }

            if (this._configRoute.LoginService > 0)
                response = await this._loginParser.WrapResponse(response, this._loginParser.RequestID);

            return response;
        }

    }
}
