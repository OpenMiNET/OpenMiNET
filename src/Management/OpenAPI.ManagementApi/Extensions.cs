using Nancy;
using Nancy.Responses;
using Nancy.Responses.Negotiation;

namespace OpenAPI.ManagementApi
{
    public static class Extensions
    {
        private static ISerializer JsonSerializer { get; set; } = new NewtonJson();
        
        /// <summary>
        /// Serializes the <paramref name="model" /> to JSON and returns it to the
        /// agent, optionally using the <paramref name="statusCode" />.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="formatter">The formatter.</param>
        /// <param name="model">The model to serialize.</param>
        /// <param name="statusCode">The HTTP status code. Defaults to <see cref="F:Nancy.HttpStatusCode.OK" />.</param>
        public static Response AsJson<TModel>(
            this IResponseFormatter formatter,
            TModel model,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            JsonResponse<TModel> jsonResponse = new JsonResponse<TModel>(model, JsonSerializer, formatter.Environment);
            jsonResponse.StatusCode = statusCode;
            return (Response) jsonResponse;
        }
    }
}