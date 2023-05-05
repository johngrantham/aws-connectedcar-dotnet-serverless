using System.Security.Claims;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ConnectedCar.Core.Shared.Data.Enums;
using ConnectedCar.Core.Shared.AuthPolicy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectedCar.Lambda
{
    public abstract class BaseRequestFunctions : BaseFunctions
    {
        protected const string HeaderXVin = "X-Vin";
        protected const string HeaderXPin = "X-Pin";

        protected const string PathDealerId = "dealerId";
        protected const string PathUsername = "username";
        protected const string PathServiceDateHour = "serviceDateHour";
        protected const string PathAppointmentId = "appointmentId";
        protected const string PathVin = "vin";
        protected const string PathTimestamp = "timestamp";
  
        protected const string QueryStateCode = "stateCode";
        protected const string QueryStartDate = "startDate";
        protected const string QueryEndDate = "endDate";
        protected const string QueryLastname = "lastname";
        protected const string QueryPartialVin = "partialVin";        
        protected readonly Dictionary<string, string> ContentResponseHeaders = new Dictionary<string, string> { { "Content-Type", "application/json" } };

        protected readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml
        };

        protected BaseRequestFunctions()
        {
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        protected BaseRequestFunctions(ServiceProvider serviceProvider) : base(serviceProvider)
        {
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        protected async Task<APIGatewayProxyResponse> Process(Func<Task<APIGatewayProxyResponse>> func, ILambdaContext context)
        {
            try
            {
                return await func.Invoke();
            }
            catch (Exception e)
            {
                context.Logger.Log(e.Message);
                context.Logger.Log(e.StackTrace);

                if (e.InnerException != null)
                {
                    context.Logger.Log(e.InnerException.Message);
                    context.Logger.Log(e.InnerException.StackTrace);
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        protected async Task<AuthPolicy> Process(Func<Task<AuthPolicy>> func, ILambdaContext context)
        {
            try
            {
                return await func.Invoke();
            }
            catch (Exception e)
            {
                context.Logger.Log(e.Message);
                context.Logger.Log(e.StackTrace);

                if (e.InnerException != null)
                {
                    context.Logger.Log(e.InnerException.Message);
                    context.Logger.Log(e.InnerException.StackTrace);
                }
            }

            return null;
        }

        protected string GetHeaderValue(APIGatewayProxyRequest req, string headerName)
        {
            if (req.Headers == null || !req.Headers.ContainsKey(headerName))
                throw new InvalidOperationException("Missing header value: " + headerName);

            return req.Headers[headerName];
        }

        protected string GetQueryParameter(APIGatewayProxyRequest req, string parameterName)
        {
            if (req.QueryStringParameters == null || !req.QueryStringParameters.ContainsKey(parameterName))
                throw new InvalidOperationException("Query parameter missing: " + parameterName);

            return req.QueryStringParameters[parameterName];
        }

        protected string GetPathParameter(APIGatewayProxyRequest req, string parameterName)
        {
            if (req.PathParameters == null || !req.PathParameters.ContainsKey(parameterName))
                throw new InvalidOperationException("Path parameter missing: " + parameterName);

            return req.PathParameters[parameterName];
        }

        protected T DeserializeItem<T>(APIGatewayProxyRequest req)
        {
            return JsonConvert.DeserializeObject<T>(req.Body, jsonSerializerSettings);
        }

        protected string SerializeItem(Object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented, jsonSerializerSettings);
        }

        protected StateCodeEnum GetStateCode(APIGatewayProxyRequest req)
        {
            StateCodeEnum stateCode = StateCodeEnum.Unknown;

            if (req.QueryStringParameters == null || !req.QueryStringParameters.ContainsKey(QueryStateCode))
                throw new InvalidOperationException("Query parameter missing: " + QueryStateCode);

            try
            {
                string code = req.QueryStringParameters[QueryStateCode];
                stateCode = (StateCodeEnum)Enum.Parse(typeof(StateCodeEnum), code);
            }
            catch (Exception) 
            {
                throw new InvalidOperationException("Query parameter value invalid: " + QueryStateCode);
            }

            return stateCode;
        }

        protected string GetCognitoUsername(APIGatewayProxyRequest req, ILambdaContext context)
        {
            var claims = (JObject)req.RequestContext.Authorizer["claims"];

            if (claims.ContainsKey("username"))
            {
                return claims.GetValue("username").ToString();
            }

            return null;
        }
    }
}
