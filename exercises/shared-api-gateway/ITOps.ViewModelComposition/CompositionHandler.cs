using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITOps.ViewModelComposition
{
    public class CompositionHandler
    {
        public static async Task<(dynamic ViewModel, int StatusCode)> HandleGetRequest(HttpContext context)
        {
            var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
            var viewModel = new DynamicViewModel(context.GetRouteData(), context.Request.Query, loggerFactory);
            var routeData = context.GetRouteData();

            // matching interceptors could be cached by URL
            var interceptors = context.RequestServices.GetServices<IRouteInterceptor>()
                .Where(interceptor => interceptor.Matches(context.GetRouteData(), HttpMethods.Get))
                .ToList();

            try
            {
                foreach (var subscriber in interceptors.OfType<ISubscribeToCompositionEvents>())
                {
                    subscriber.Subscribe(viewModel);
                }

                var pendingTasks = new List<Task>();

                foreach (var processor in interceptors.OfType<IViewModelProcessor>())
                {
                    pendingTasks.Add(processor.Process(viewModel, routeData, context.Request.Query).WithLogging(() => loggerFactory.CreateLogger(processor.GetType())));
                }

                if (!pendingTasks.Any())
                {
                    return (null, StatusCodes.Status404NotFound);
                }
                else
                {
                    await Task.WhenAll(pendingTasks);

                    return (viewModel, StatusCodes.Status200OK);
                }
            }
            finally
            {
                viewModel.ClearSubscriptions();
            }
        }

        public static async Task<int> HandlePostRequest(HttpContext context)
        {
            var loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
            var viewModel = new DynamicViewModel(context.GetRouteData(), context.Request.Query, loggerFactory);
            var routeData = context.GetRouteData();

            var processors = context.RequestServices.GetServices<IViewModelProcessor>()
                .Where(interceptor => interceptor.Matches(routeData, HttpMethods.Post))
                .ToList();

            if (!processors.Any())
            {
                return StatusCodes.Status404NotFound;
            }

            var processingTasks = processors.Select(processor =>
                processor.Process(viewModel, routeData, context.Request.Query)
                    .WithLogging(() => loggerFactory.CreateLogger(processor.GetType())));

            await Task.WhenAll(processingTasks);

            return StatusCodes.Status202Accepted;
        }
    }
}
