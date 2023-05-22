using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ConnectedCar.Core.Shared.Data.Enums;
using ConnectedCar.Core.Shared.Data.Entities;
using ConnectedCar.Core.Shared.Data.Updates;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConnectedCar.Lambda
{
    public class AdminFunctions : BaseRequestFunctions
    {
        public AdminFunctions() {}

        public AdminFunctions(ServiceProvider serviceProvider) : base(serviceProvider) {}

        public async Task<APIGatewayProxyResponse> CreateDealer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                Dealer dealer = DeserializeItem<Dealer>(request);
                string dealerId = await GetDealerService().CreateDealer(dealer);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string,string> {{ "Location", $"/admin/dealers/{dealerId}" }}
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetDealers(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                StateCodeEnum stateCode = GetStateCode(request);
                List<Dealer> dealers = await GetDealerService().GetDealers(stateCode);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(dealers),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetDealer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string dealerId = GetPathParameter(request, PathDealerId);
                Dealer dealer = await GetDealerService().GetDealer(dealerId);

                if (dealer != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(dealer),
                        Headers = ContentResponseHeaders
                    };
                }
                else
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

            }, context);
        }

        /********************************************************************************************************/

        public async Task<APIGatewayProxyResponse> CreateTimeslot(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                context.Logger.Log(request.Body);

                Timeslot timeslot = DeserializeItem<Timeslot>(request);
                await GetTimeslotService().CreateTimeslot(timeslot);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string,string> {{ "Location", $"/admin/dealers/{timeslot.DealerId}/timeslots/{timeslot.ServiceDateHour}" }}
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetTimeslots(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string dealerId = GetPathParameter(request, PathDealerId);
                string startDate = GetQueryParameter(request, QueryStartDate);
                string endDate = GetQueryParameter(request, QueryEndDate);
                List<Timeslot> timeslots = await GetTimeslotService().GetTimeslots(dealerId, startDate, endDate);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(timeslots),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetTimeslot(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string dealerId = GetPathParameter(request, PathDealerId);
                string serviceDateHour = GetPathParameter(request, PathServiceDateHour);

                Timeslot timeslot = await GetTimeslotService().GetTimeslot(dealerId, serviceDateHour);

                if (timeslot != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(timeslot),
                        Headers = ContentResponseHeaders
                    };
                }
                else
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

            }, context);
        }

        /******************************************************************************************************************/

        public async Task<APIGatewayProxyResponse> CreateCustomer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                CustomerProvision provision = DeserializeItem<CustomerProvision>(request);

                await GetAdminOrchestrator().CreateCustomer(provision);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string,string> {{ "Location", $"/admin/customers/{provision.Username}" }}
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetCustomers(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string lastname = GetQueryParameter(request, QueryLastname);
                List<Customer> customers = await GetCustomerService().GetCustomers(lastname);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(customers),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetCustomer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetPathParameter(request, PathUsername);
                Customer customer = await GetCustomerService().GetCustomer(username);

                if (customer != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(customer),
                        Headers = ContentResponseHeaders
                    };
                }
                else
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

            }, context);
        }

        /******************************************************************************************************************/

        public async Task<APIGatewayProxyResponse> CreateRegistration(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                Registration registration = DeserializeItem<Registration>(request);
                await GetRegistrationService().CreateRegistration(registration);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string,string> {{ "Location", $"/admin/customers/{registration.Username}/registrations/{registration.Vin}" }}
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> UpdateRegistration(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                RegistrationPatch patch = DeserializeItem<RegistrationPatch>(request);
                await GetRegistrationService().UpdateRegistration(patch);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetCustomerRegistrations(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetPathParameter(request, PathUsername);
                List<Registration> registrations = await GetRegistrationService().GetCustomerRegistrations(username);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(registrations),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

       public async Task<APIGatewayProxyResponse> GetRegistration(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetPathParameter(request, PathUsername);
                string vin = GetPathParameter(request, PathVin);

                Registration registration = await GetRegistrationService().GetRegistration(username, vin);

                if (registration != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(registration),
                        Headers = ContentResponseHeaders
                    };
                }
                else
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

            }, context);
        }

        /******************************************************************************************************************/

        public async Task<APIGatewayProxyResponse> CreateVehicle(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                Vehicle vehicle = DeserializeItem<Vehicle>(request);
                await GetVehicleService().CreateVehicle(vehicle);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Headers = new Dictionary<string,string> {{ "Location", $"/admin/vehicles/{vehicle.Vin}" }}
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetVehicle(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string vin = GetPathParameter(request, PathVin);
                Vehicle vehicle = await GetVehicleService().GetVehicle(vin);

                if (vehicle != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(vehicle),
                        Headers = ContentResponseHeaders
                    };
                }
                else
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetVehicleRegistrations(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string vin = GetPathParameter(request, PathVin);
                List<Registration> registrations = await GetRegistrationService().GetVehicleRegistrations(vin);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(registrations),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }
    }
}
