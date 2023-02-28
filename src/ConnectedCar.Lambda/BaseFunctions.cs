using System;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.DynamoDBv2;
using ConnectedCar.Core.Shared.Services;
using ConnectedCar.Core.Shared.Orchestrators;
using ConnectedCar.Core.Services;
using ConnectedCar.Core.Services.Context;
using ConnectedCar.Core.Services.Translator;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ConnectedCar.Lambda
{
    public abstract class BaseFunctions
    {
        private readonly ServiceProvider serviceProvider;

        protected BaseFunctions()
        {
            AWSSDKHandler.RegisterXRay<IAmazonDynamoDB>();

            var services = new ServiceCollection();

            services.AddSingleton<IServiceContext, CloudServiceContext>();
            services.AddSingleton<ITranslator, Translator>();
            services.AddSingleton<IAppointmentService, AppointmentService>();
            services.AddSingleton<ICustomerService, CustomerService>();
            services.AddSingleton<IDealerService, DealerService>();
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<IRegistrationService, RegistrationService>();
            services.AddSingleton<ITimeslotService, TimeslotService>();
            services.AddSingleton<IVehicleService, VehicleService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IAdminOrchestrator, AdminOrchestrator>();
            services.AddSingleton<ICustomerOrchestrator, CustomerOrchestrator>();

            serviceProvider = services.BuildServiceProvider();
        }

        protected BaseFunctions(ServiceProvider serviceProvider) 
        {
            if (serviceProvider == null)
                throw new InvalidOperationException();

            this.serviceProvider = serviceProvider;
        }

        protected IAppointmentService GetAppointmentService()
        {
            return serviceProvider.GetRequiredService<IAppointmentService>();
        }

        protected ICustomerService GetCustomerService()
        {
            return serviceProvider.GetRequiredService<ICustomerService>();
        }

        protected IDealerService GetDealerService()
        {
            return serviceProvider.GetRequiredService<IDealerService>();
        }

        protected IEventService GetEventService()
        {
            return serviceProvider.GetRequiredService<IEventService>();
        }

        protected IRegistrationService GetRegistrationService()
        {
            return serviceProvider.GetRequiredService<IRegistrationService>();
        }

        protected ITimeslotService GetTimeslotService()
        {
            return serviceProvider.GetRequiredService<ITimeslotService>();
        }

        protected IVehicleService GetVehicleService()
        {
            return serviceProvider.GetRequiredService<IVehicleService>();
        }

        protected IUserService GetUserService()
        {
            return serviceProvider.GetRequiredService<IUserService>();
        }

        protected IAdminOrchestrator GetAdminOrchestrator()
        {
            return serviceProvider.GetRequiredService<IAdminOrchestrator>();
        }

        protected ICustomerOrchestrator GetCustomerOrchestrator()
        {
            return serviceProvider.GetRequiredService<ICustomerOrchestrator>();
        }
    }
}
