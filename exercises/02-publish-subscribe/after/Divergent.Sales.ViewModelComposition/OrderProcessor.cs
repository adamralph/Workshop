using System;
using System.Net.Http;
using System.Threading.Tasks;
using ITOps.ViewModelComposition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Divergent.Sales.ViewModelComposition
{
    public class OrderProcessor : IViewModelProcessor
    {
        public bool Matches(RouteData routeData, string httpMethod) =>
            HttpMethods.IsPost(httpMethod)
                && string.Equals((string)routeData.Values["controller"], "orders/createOrder", StringComparison.OrdinalIgnoreCase);

        public async Task Process(dynamic viewModel, RouteData routeData, IQueryCollection query)
        {
            // Hardcoded for simplicity. In a production app, a config object could be injected.
            var url = $"http://localhost:20185/api/orders";

            // ultra hacky, just for demo purposes
            var content =
$@"{{
    customerId: {viewModel.customerId},
    products: [{{
        productId: {viewModel.products[0].productId}
    }}]
}}";

            var response = await new HttpClient().PostAsync(url, new StringContent(content));
        }
    }
}
