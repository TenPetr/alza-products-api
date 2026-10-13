using AlzaProducts.Domain;

namespace AlzaProducts.Tests.Domain;

public class ProductTests
{
    private static readonly Guid ValidId = new("0199d4a2-5b7e-7c41-9f3a-2e8b6d1c4a70");
    private static readonly Uri ValidImgUri = new("https://cdn.example.com/products/1.jpg");

    [Fact]
    public void Constructor_WithValidValues_SetsAllProperties()
    {
        var product = new Product(ValidId, "Gaming Laptop", ValidImgUri, 1299.90m, "Fast and light.");

        Assert.Equal(ValidId, product.Id);
        Assert.Equal("Gaming Laptop", product.Name);
        Assert.Equal(ValidImgUri, product.ImgUri);
        Assert.Equal(1299.90m, product.Price);
        Assert.Equal("Fast and light.", product.Description);
    }

    [Fact]
    public void Constructor_WithoutDescription_LeavesDescriptionEmpty()
    {
        var product = CreateProduct(description: null);

        Assert.Null(product.Description);
    }

    [Fact]
    public void Constructor_TrimsNameAndDescription()
    {
        var product = CreateProduct(name: "  Laptop  ", description: "  Fast.  ");

        Assert.Equal("Laptop", product.Name);
        Assert.Equal("Fast.", product.Description);
    }

    [Fact]
    public void Constructor_WithEmptyId_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => CreateProduct(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingName_Throws(string? name)
    {
        var exception = Assert.ThrowsAny<ArgumentException>(() => CreateProduct(name: name!));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNameAtMaxLength_Succeeds()
    {
        var name = new string('a', Product.NameMaxLength);

        var product = CreateProduct(name: name);

        Assert.Equal(name, product.Name);
    }

    [Fact]
    public void Constructor_WithTooLongName_Throws()
    {
        var name = new string('a', Product.NameMaxLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateProduct(name: name));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullImgUri_Throws()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new Product(ValidId, "Laptop", null!, 10m));

        Assert.Equal("imgUri", exception.ParamName);
    }

    [Theory]
    [InlineData("images/laptop.jpg")]
    [InlineData("ftp://cdn.example.com/laptop.jpg")]
    [InlineData("file:///images/laptop.jpg")]
    public void Constructor_WithNonHttpImgUri_Throws(string uri)
    {
        var imgUri = new Uri(uri, UriKind.RelativeOrAbsolute);

        var exception = Assert.Throws<ArgumentException>(() => CreateProduct(imgUri: imgUri));

        Assert.Equal("imgUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithTooLongImgUri_Throws()
    {
        var imgUri = new Uri("https://cdn.example.com/" + new string('a', Product.ImgUriMaxLength));

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateProduct(imgUri: imgUri));

        Assert.Equal("imgUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativePrice_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateProduct(price: -0.01m));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithZeroPrice_Succeeds()
    {
        var product = CreateProduct(price: 0m);

        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void Constructor_WithTooLongDescription_Throws()
    {
        var description = new string('a', Product.DescriptionMaxLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateProduct(description: description));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void UpdateDescription_WithText_ReplacesDescription()
    {
        var product = CreateProduct(description: "Original");

        product.UpdateDescription("Updated");

        Assert.Equal("Updated", product.Description);
    }

    [Fact]
    public void UpdateDescription_TrimsText()
    {
        var product = CreateProduct();

        product.UpdateDescription("  Updated  ");

        Assert.Equal("Updated", product.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDescription_WithNullOrWhitespace_ClearsDescription(string? description)
    {
        var product = CreateProduct(description: "Original");

        product.UpdateDescription(description);

        Assert.Null(product.Description);
    }

    [Fact]
    public void UpdateDescription_WithTextAtMaxLength_Succeeds()
    {
        var product = CreateProduct();
        var description = new string('a', Product.DescriptionMaxLength);

        product.UpdateDescription(description);

        Assert.Equal(description, product.Description);
    }

    [Fact]
    public void UpdateDescription_WithTooLongText_ThrowsAndKeepsPreviousDescription()
    {
        var product = CreateProduct(description: "Original");
        var description = new string('a', Product.DescriptionMaxLength + 1);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => product.UpdateDescription(description));

        Assert.Equal("description", exception.ParamName);
        Assert.Equal("Original", product.Description);
    }

    private static Product CreateProduct(
        Guid? id = null,
        string name = "Laptop",
        Uri? imgUri = null,
        decimal price = 999m,
        string? description = null) =>
        new(id ?? ValidId, name, imgUri ?? ValidImgUri, price, description);
}
