using AlzaProducts.Domain;

namespace AlzaProducts.Infrastructure.Products;

/// <summary>
/// Sample products used as mock data, as the initial database seed and in tests
/// </summary>
public static class SampleProducts
{
    /// <summary>Creates new instances on every call, so callers never share state</summary>
    public static IReadOnlyList<Product> Create() =>
    [
        CreateProduct("509f8af8-f889-4dbc-b462-861f775a1536", "Action Camera 4K", 7990m, "Waterproof action camera with image stabilization"),
        CreateProduct("b6feb34c-645b-46df-af66-c618f9db98de", "Bluetooth Speaker", 1890m, "Portable speaker with 20 hours of battery life"),
        CreateProduct("6a115977-09ce-495e-af2a-3d9f7d688ec1", "Desk Lamp LED", 899m),
        CreateProduct("1a597a13-4942-4858-aeff-02b1e2541022", "Drawing Tablet", 3490m, "Pen tablet with 8192 pressure levels"),
        CreateProduct("388948a7-6d38-4991-87dd-721ecbd10836", "E-book Reader", 3290m, "6-inch glare-free display"),
        CreateProduct("42dd4e14-9177-4775-a0e3-dffb1bce2c91", "Electric Toothbrush", 1490m),
        CreateProduct("be0e5470-d573-4aae-9d82-06a832b7d994", "External SSD 1 TB", 2790m, "USB-C portable drive with read speeds up to 1050 MB/s"),
        CreateProduct("cbebe37a-e249-41ca-9ef6-fdd20eec859a", "Fitness Tracker", 1290m, "Heart rate and sleep monitoring"),
        CreateProduct("2b136352-3810-4848-bd0d-4177323bd49a", "Gaming Chair", 5990m),
        CreateProduct("c54d2cdb-8529-4c78-9638-f5b238e3cc89", "Gaming Laptop 16", 42990m, "16-inch laptop with 32 GB RAM and a dedicated graphics card"),
        CreateProduct("4e35f2f2-b436-4dfe-8b41-2977c133f6e6", "Graphics Card 16 GB", 18990m, "Ray tracing graphics card for 1440p gaming"),
        CreateProduct("e941097e-f039-45b9-b58e-ab4756cd63cc", "HDMI Cable 2 m", 249.90m),
        CreateProduct("392639bc-59bd-47e5-a3d5-f74c9609cc7d", "Laser Printer", 3990m, "Monochrome printer with Wi-Fi and duplex printing"),
        CreateProduct("a0588386-2e00-41df-95f4-80cee7d37ba7", "Mechanical Keyboard", 2490m, "Hot-swappable switches and RGB backlight"),
        CreateProduct("5545ca22-dee2-4c95-9b9d-04bc295eda24", "Mesh Wi-Fi System", 6490m, "Three units covering up to 450 square meters"),
        CreateProduct("9e0c2603-4e2f-45cc-b199-bf47011dce5a", "Monitor 27 4K", 8990m, "27-inch IPS panel with USB-C power delivery"),
        CreateProduct("cbb5a222-93e8-443b-83a7-165c1b51ac36", "Noise Cancelling Headphones", 7490m, "Over-ear headphones with adaptive noise cancelling"),
        CreateProduct("757bc89f-0011-4dc5-adad-5ea049b07cc7", "Portable Power Bank", 989.90m),
        CreateProduct("0d31dc29-41ce-4a59-8e5c-a4fca565d144", "Robot Vacuum Cleaner", 9990m, "Lidar navigation and automatic emptying station"),
        CreateProduct("83fe51b9-61d0-45d1-bb03-f4339fa5b995", "Smart Watch", 6990m, "Always-on display, GPS and contactless payments"),
        CreateProduct("c8172ed6-dbcb-4811-8afb-932532c1262c", "Smartphone 256 GB", 19990m, "6.7-inch OLED display and triple camera"),
        CreateProduct("bef71233-5e38-455f-b5bd-5f46c58f6b44", "Tablet 11", 11990m),
        CreateProduct("a77fd1ec-994e-4da5-965f-1829f06b2afd", "USB-C Charger 65 W", 890m, "GaN charger for laptops and phones"),
        CreateProduct("5815469c-3017-4419-a295-e8e96de14447", "Webcam Full HD", 1590m),
        CreateProduct("3c1d6197-2ed0-44d5-b05a-b36ccd56cadc", "Wireless Mouse", 690m, "Silent clicks and 18-month battery life"),
    ];

    private static Product CreateProduct(string id, string name, decimal price, string? description = null) =>
        new(Guid.Parse(id), name, new Uri($"https://cdn.example.com/products/{ToSlug(name)}.jpg"), price, description);

    private static string ToSlug(string name) =>
        string.Join('-', name.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
