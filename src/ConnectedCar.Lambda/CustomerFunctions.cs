using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ConnectedCar.Core.Shared.Data.Enums;
using ConnectedCar.Core.Shared.Data.Entities;
using ConnectedCar.Core.Shared.Data.Updates;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectedCar.Lambda
{
    public class CustomerFunctions : BaseRequestFunctions
    {
        public CustomerFunctions() {}

        public CustomerFunctions(ServiceProvider serviceProvider) : base(serviceProvider) {}

        public async Task<APIGatewayProxyResponse> UpdateCustomer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                CustomerPatch patch = DeserializeItem<CustomerPatch>(request);
                patch.Username = username;
                await GetCustomerService().UpdateCustomer(patch);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetCustomer(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
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

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };

            }, context);
        }

        /******************************************************************************************************************/

        public async Task<APIGatewayProxyResponse> CreateAppointment(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                Appointment appointment = DeserializeItem<Appointment>(request);

                string appointmentId = await GetCustomerOrchestrator().CreateAppointment(username, appointment);

                if (appointmentId != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.Created,
                        Headers = new Dictionary<string,string> {{ "Location", $"/customer/appointments/{appointmentId}" }}
                    };
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetAppointment(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                string appointmentId = GetPathParameter(request, PathAppointmentId);

                Appointment appointment = await GetAppointmentService().GetAppointment(appointmentId);

                if (appointment != null && appointment.RegistrationKey.Username.Equals(username))
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(appointment),
                        Headers = ContentResponseHeaders
                    };
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> DeleteAppointment(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                string appointmentId = GetPathParameter(request, PathAppointmentId);

                Appointment appointment = await GetAppointmentService().GetAppointment(appointmentId);

                if (appointment != null && appointment.RegistrationKey.Username.Equals(username))
                {
                    await GetAppointmentService().DeleteAppointment(appointmentId);

                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK
                    };
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };

            }, context);
        }

        /******************************************************************************************************************/

        public async Task<APIGatewayProxyResponse> GetRegistrations(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                List<Registration> registrations = await GetRegistrationService().GetCustomerRegistrations(username);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(registrations),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetAppointments(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                string vin = GetPathParameter(request, PathVin);
                List<Appointment> appointments = await GetAppointmentService().GetRegistrationAppointments(username, vin);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(appointments),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        /******************************************************************************************************************/

        public async Task<APIGatewayProxyResponse> GetVehicle(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                string vin = GetPathParameter(request, PathVin);
                Vehicle vehicle = await GetCustomerOrchestrator().GetVehicle(username, vin);

                if (vehicle != null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Body = SerializeItem(vehicle),
                        Headers = ContentResponseHeaders
                    };
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound
                };

            }, context);
        }

        public async Task<APIGatewayProxyResponse> GetEvents(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                string username = GetCognitoUsername(request, context);
                string vin = GetPathParameter(request, PathVin);

                List<Event> events = await GetCustomerOrchestrator().GetEvents(username, vin);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(events),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }

        /******************************************************************************************************************/

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

        public async Task<APIGatewayProxyResponse> GetTimeslots(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await Process(async () =>
            {
                const string DateFormat = "yyyy-MM-dd";

                string dealerId = GetPathParameter(request, PathDealerId);

                string startDate = DateTime.Now.ToString(DateFormat);
                string endDate = DateTime.Now.AddDays(30).ToString(DateFormat);

                List<Timeslot> timeslots = await GetTimeslotService().GetTimeslots(dealerId, startDate, endDate);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = SerializeItem(timeslots),
                    Headers = ContentResponseHeaders
                };

            }, context);
        }
    }
}
