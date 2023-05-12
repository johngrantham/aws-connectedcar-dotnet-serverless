using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ConnectedCar.Core.Shared.AuthPolicy;
using System.Threading.Tasks;

namespace ConnectedCar.Lambda
{
    public class VehicleAuthorizer : BaseRequestFunctions
    {
        public VehicleAuthorizer() {}

        public async Task<AuthPolicy> Authorize(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string vin = GetHeaderValue(request, HeaderXVin);
                string pin = GetHeaderValue(request, HeaderXPin);

                bool isAllowed = await GetVehicleService().ValidatePin(vin, pin);

                return AuthPolicyFactory.GetApiPolicy(
                    vin,
                    isAllowed);
                    
            }, context);
        }
    }
}
