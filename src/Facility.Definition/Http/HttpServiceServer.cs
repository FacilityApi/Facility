namespace Facility.Definition.Http;

/// <summary>
/// Represents an HTTP server for a service, including its URL and optional description.
/// </summary>
public sealed class HttpServiceServer
{
	/// <summary>
	/// Gets the URL of the HTTP server.
	/// </summary>
	public string Url { get; }

	/// <summary>
	/// Gets an optional description of the HTTP server.
	/// </summary>
	public string? Description { get; }

	internal HttpServiceServer(string url, string? description = null)
	{
		Url = url ?? throw new ArgumentNullException(nameof(url));
		Description = description;
	}
}
