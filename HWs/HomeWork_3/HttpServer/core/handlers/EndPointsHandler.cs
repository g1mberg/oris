using System.Net;
using System.Reflection;
using HttpServer.core.handlers;

internal class EndpointsHandler : Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var parts = (request.Url?.AbsolutePath ?? "/").Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            Successor?.HandleRequest(context);
            return;
        }

        var endpointSlug = parts[0];             
        var routeTail    = string.Join('/', parts.Skip(1));

        var assembly = Assembly.GetExecutingAssembly();

        var endpointType = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
            .FirstOrDefault(t => IsMatch(t.Name, endpointSlug));

        if (endpointType == null)
        {
            Successor?.HandleRequest(context);
            return;
        }

        var httpAttrName = $"Http{request.HttpMethod}";
        var methods = endpointType.GetMethods()
            .Where(m => m.GetCustomAttributes(true)
                .Any(attr => string.Equals(attr.GetType().Name, httpAttrName, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (methods.Count == 0)
        {
            Successor?.HandleRequest(context);
            return;
        }

        var chosen = methods.FirstOrDefault(m =>
        {
            var attr = m.GetCustomAttributes(true)
                        .First(a => string.Equals(a.GetType().Name, httpAttrName, StringComparison.OrdinalIgnoreCase));

            var routeProp = attr.GetType().GetProperty("Route");
            var route = (routeProp?.GetValue(attr) as string)?.Trim('/') ?? string.Empty;

            return string.IsNullOrEmpty(route)
                ? string.IsNullOrEmpty(routeTail) 
                : string.Equals(route, routeTail, StringComparison.OrdinalIgnoreCase);
        }) ?? methods.First(); 

        var instance = Activator.CreateInstance(endpointType);

        object? result = null;
        try
        {
            var ps = chosen.GetParameters();
            if (ps.Length == 1 && ps[0].ParameterType == typeof(HttpListenerContext))
                result = chosen.Invoke(instance, new object[] { context });
            else
                result = chosen.Invoke(instance, null);

            if (result is Task t)
                t.GetAwaiter().GetResult();
        }
        catch
        {
            context.Response.StatusCode = 500;
        }
    }

    private static bool IsMatch(string typeName, string slug)
    {
        return typeName.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
               typeName.Equals($"{slug}Endpoint", StringComparison.OrdinalIgnoreCase);
    }
}
