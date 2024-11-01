﻿#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Aspire.Hosting.ApplicationModel;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public sealed class LakeFSResource(string name) : ContainerResource(name), IResourceWithConnectionString, IResourceWithWaitSupport
{
    // Constants used to refer to well known-endpoint names, this is specific
    // for each resource type. lakeFS exposes an HTTP endpoint
    // endpoint.
    internal const string HttpEndpointName = "http";

    // An EndpointReference is a core .NET Aspire type used for keeping
    // track of endpoint details in expressions. Simple literal values cannot
    // be used because endpoints are not known until containers are launched.
    private EndpointReference? _httpReference;

    public EndpointReference Endpoint => _httpReference ??= new(this, HttpEndpointName);

    // Required property on IResourceWithConnectionString. Represents a connection
    // string that applications can use to access the lakeFS server.
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"http://{Endpoint.Property(EndpointProperty.Host)}:{Endpoint.Property(EndpointProperty.Port)}"
    );
}