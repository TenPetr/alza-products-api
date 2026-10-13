namespace AlzaProducts.Domain;

/// <summary>
/// A product offered by the e-shop. It guards its own rules, so an instance is always valid.
/// </summary>
public sealed class Product
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 4000;
    public const int ImgUriMaxLength = 2048;

    public Product(Guid id, string name, Uri imgUri, decimal price, string? description = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(imgUri);
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        var trimmedName = name.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmedName.Length, NameMaxLength, nameof(name));
        EnsureValidImgUri(imgUri);

        Id = id;
        Name = trimmedName;
        ImgUri = imgUri;
        Price = price;
        Description = NormalizeDescription(description);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public Uri ImgUri { get; private set; }

    public decimal Price { get; private set; }

    public string? Description { get; private set; }

    /// <summary>
    /// Replaces the description. Null or whitespace clears it; surrounding whitespace is trimmed.
    /// </summary>
    public void UpdateDescription(string? description) => Description = NormalizeDescription(description);

    private static string? NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, DescriptionMaxLength, nameof(description));
        return trimmed;
    }

    private static void EnsureValidImgUri(Uri imgUri)
    {
        if (!imgUri.IsAbsoluteUri || (imgUri.Scheme != Uri.UriSchemeHttp && imgUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Image URI must be an absolute http or https URI", nameof(imgUri));
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(imgUri.OriginalString.Length, ImgUriMaxLength, nameof(imgUri));
    }
}
