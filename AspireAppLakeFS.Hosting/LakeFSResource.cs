namespace Aspire.Hosting.ApplicationModel;

public sealed class LakeFSResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    // Constants used to refer to well known-endpoint names, this is specific
    // for each resource type. lakeFS exposes an SMTP endpoint and a HTTP
    // endpoint.
    internal const string HttpEndpointName = "http";

    // An EndpointReference is a core .NET Aspire type used for keeping
    // track of endpoint details in expressions. Simple literal values cannot
    // be used because endpoints are not known until containers are launched.
    private EndpointReference? _httpReference;

    public EndpointReference Endpoint =>
        _httpReference ??= new(this, HttpEndpointName);

    // Required property on IResourceWithConnectionString. Represents a connection
    // string that applications can use to access the lakeFS server. In this case
    // the connection string is composed of the SmtpEndpoint endpoint reference.
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{Endpoint.Property(EndpointProperty.Host)}:{Endpoint.Property(EndpointProperty.Port)}"
        );
}