using ApiGateway.EntityRoutes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ApiGateway
{
    public class Router
    {

        public List<EntityRoutes.Route> Routes { get; set; }
        public List<EntityRoutes.AuthenticationRoute> AuthenticationRoutes { get; set; }
        public Routing.Validation AuthenticationService { get; set; }


        public Router(string routeConfigFilePath)
        {
            dynamic router = Helper.JsonLoader.LoadFromFile<dynamic>(routeConfigFilePath);

            Routes = Helper.JsonLoader.Deserialize<List<EntityRoutes.Route>>(Convert.ToString(router.routes));
            AuthenticationRoutes = Helper.JsonLoader.Deserialize<List<EntityRoutes.AuthenticationRoute>>(Convert.ToString(router.authenticationServices));

        }

        public async Task<HttpResponseMessage> RouteRequest(HttpRequest request)
        {
            // create logger 
            Helper.Logger logger = Helper.Logger.Instance;                                    
            logger.Log("START NEW REQUEST", Helper.Logger.eLogLevels.DEBUG);

            // gets requested resource
            string path = request.Path.ToString();                                              
            logger.Log("Request path: " + path, Helper.Logger.eLogLevels.INFO);

            // finds destination Route
            EntityRoutes.Route route;
            try
            {
                route = Routes.First(r => r.Endpoint.Equals(path));
            }
            catch (Exception ex)
            {
                logger.Log("ERROR PARSING ROUTES.JSON. " + ex.Message, Helper.Logger.eLogLevels.ERROR);
                return Helper.ErrorResponse.Create(request, (int)HttpStatusCode.NotFound, "The path " + path + " could not be found.");
            }

            // checks if authentication is needed
            if (route.Destination.RequiresAuthentication)
            {
                logger.Log("Route asks authentication.", Helper.Logger.eLogLevels.WARNING);

                // Authorization is Bearer token
                string token = request.Headers["Authorization"];
                // LoginService is a number identifying login service (1 for TestToken) 
                byte loginService = Convert.ToByte(request.Headers["LoginService"]);

                // if no loginservice was specified, an error will be sent
                if (loginService == 0) return Helper.ErrorResponse.Create(request, (int)HttpStatusCode.NotFound, "LoginService not specified.");

                // creates the authentication Route (based on loginservice)
                AuthenticationService = new Routing.Validation(AuthenticationRoutes, loginService);
                // gets response from Authentication Route
                HttpResponseMessage authResponse = await AuthenticationService.SendRequest(token);                

                // if Authentication is not ok then an error will be sent, otherwise process continues with original request.
                if (!authResponse.IsSuccessStatusCode) return await Helper.ErrorResponse.Construct(request, authResponse, loginService);
            }

            // creating destination
            Routing.Destination destination = new Routing.Destination(route);

            // sends request to destination
            HttpResponseMessage response = null;
            try
            {
                response = await destination.SendRequest(request);
            }
            catch (Exception ex)
            {
                logger.Log("SEND REQUEST ERROR." + ex.Message, Helper.Logger.eLogLevels.ERROR);
                return Helper.ErrorResponse.Create(request, (int)HttpStatusCode.InternalServerError, "Server error: " + ex.Message);
            }

            // closes logger
            logger.Log("REQUEST HANDLED.", Helper.Logger.eLogLevels.DEBUG);
            logger.Dispose();

            return response;
        }

    }
}
