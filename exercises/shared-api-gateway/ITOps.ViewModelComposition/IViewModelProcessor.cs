using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace ITOps.ViewModelComposition
{
    public interface IViewModelProcessor : IRouteInterceptor
    {
        Task Process(dynamic viewModel, RouteData routeData, IQueryCollection query);
    }
}
