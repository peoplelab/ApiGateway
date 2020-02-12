
namespace ApiGateway.EntityRoutes
{
    public class Route
    {
        public string Endpoint { get; set; }
        public DestinationRoute Destination { get; set; }
        public byte LoginService { get; set; }
    }

    public class DestinationRoute
    {
        public string Uri { get; set; }
        public string Method { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string Format { get; set; }
    }
}
