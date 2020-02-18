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
            Helper.Logger logger = Helper.Logger.Instance;                                    // logging
            logger.Log("START NEW REQUEST", Helper.Logger.eLogLevels.DEBUG);

            string path = request.Path.ToString();                                              // requested resource
            logger.Log("Request path: " + path, Helper.Logger.eLogLevels.INFO);

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

            if (route.Destination.RequiresAuthentication)
            {
                logger.Log("Route asks authentication.", Helper.Logger.eLogLevels.WARNING);

                string token = request.Headers["Authorization"];
                byte loginService = Convert.ToByte(request.Headers["LoginService"]);

                if (loginService == 0) return Helper.ErrorResponse.Create(request, (int)HttpStatusCode.NotFound, "LoginService not specified.");

                AuthenticationService = new Routing.Validation(AuthenticationRoutes, loginService);
                HttpResponseMessage authResponse = await AuthenticationService.SendRequest(token);                

                if (!authResponse.IsSuccessStatusCode) return await Helper.ErrorResponse.Construct(authResponse, loginService);
            }

            Routing.Destination destination = new Routing.Destination(route);

            HttpResponseMessage response = null;
            try
            {
                response = await destination.SendRequest(request);
            }catch (Exception ex){
                logger.Log("SEND REQUEST ERROR." + ex.Message, Helper.Logger.eLogLevels.ERROR);
                return Helper.ErrorResponse.Create(request, (int)HttpStatusCode.InternalServerError, "Server error: " + ex.Message);
            }

            logger.Log("REQUEST HANDLED.", Helper.Logger.eLogLevels.DEBUG);
            logger.Dispose();

            return response;
        }

    }
}
