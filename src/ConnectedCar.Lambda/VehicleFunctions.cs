using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ConnectedCar.Core.Shared.Data.Entities;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectedCar.Lambda
{
    public class VehicleFunctions : BaseRequestFunctions 
    {
        public VehicleFunctions() {}

        public VehicleFunctions(ServiceProvider serviceProvider) : base(serviceProvider) {}
        
        public async Task<APIGatewayProxyResponse> CreateEvent(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string vin = GetHeaderValue(request, HeaderXVin);
                Event evnt = DeserializeItem<Event>(request);
                evnt.Vin = vin;
                await GetEventService().CreateEvent(evnt);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string,string> {{ "Location", $"/vehicle/events/{evnt.Timestamp}" }}
                };

            }, context);
        }
        
        public async Task<APIGatewayProxyResponse> GetEvents(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string vin = GetHeaderValue(request, HeaderXVin);
                List<Event> events = await GetEventService().GetEvents(vin);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(events),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetEvent(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string vin = GetHeaderValue(request, HeaderXVin);
                string timestamp = GetPathParameter(request, PathTimestamp);

                Event evnt = await GetEventService().GetEvent(vin, long.Parse(timestamp));

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(evnt),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }
    }
}
