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
        CreateProduct("f7fd93ab-32a7-4884-e83d-08df296652de", "Action Camera 4K", 7990m, "Waterproof action camera with image stabilization"),
        CreateProduct("0f6b5c98-ded2-46e0-e83e-08df296652de", "Bluetooth Speaker", 1890m, "Portable speaker with 20 hours of battery life"),
        CreateProduct("dbb608e6-4a09-4ee3-e83f-08df296652de", "Desk Lamp LED", 899m),
        CreateProduct("aec8cdf3-b970-426a-e840-08df296652de", "Drawing Tablet", 3490m, "Pen tablet with 8192 pressure levels"),
        CreateProduct("0b6506ba-a631-458f-e841-08df296652de", "E-book Reader", 3290m, "6-inch glare-free display"),
        CreateProduct("634235e2-2697-49f5-e842-08df296652de", "Electric Toothbrush", 1490m),
        CreateProduct("87bf08b9-7210-4f23-e843-08df296652de", "External SSD 1 TB", 2790m, "USB-C portable drive with read speeds up to 1050 MB/s"),
        CreateProduct("500d4815-f9bb-43dc-e844-08df296652de", "Fitness Tracker", 1290m, "Heart rate and sleep monitoring"),
        CreateProduct("fbf3b1e1-3353-4096-e845-08df296652de", "Gaming Chair", 5990m),
        CreateProduct("5c644c28-1b4b-49b6-e846-08df296652de", "Gaming Laptop 16", 42990m, "16-inch laptop with 32 GB RAM and a dedicated graphics card"),
        CreateProduct("699497c0-da78-4358-e847-08df296652de", "Graphics Card 16 GB", 18990m, "Ray tracing graphics card for 1440p gaming"),
        CreateProduct("ea83b579-8666-42b7-e848-08df296652de", "HDMI Cable 2 m", 249.90m),
        CreateProduct("d9257959-618e-4037-e849-08df296652de", "Laser Printer", 3990m, "Monochrome printer with Wi-Fi and duplex printing"),
        CreateProduct("421001af-948d-4e59-e84a-08df296652de", "Mechanical Keyboard", 2490m, "Hot-swappable switches and RGB backlight"),
        CreateProduct("5bae264c-a5f1-448d-e84b-08df296652de", "Mesh Wi-Fi System", 6490m, "Three units covering up to 450 square meters"),
        CreateProduct("f6f5e672-78cd-4509-e84c-08df296652de", "Monitor 27 4K", 8990m, "27-inch IPS panel with USB-C power delivery"),
        CreateProduct("f6ee1131-cb6c-4d44-e84d-08df296652de", "Noise Cancelling Headphones", 7490m, "Over-ear headphones with adaptive noise cancelling"),
        CreateProduct("2254f009-96b2-4e0d-e84e-08df296652de", "Portable Power Bank", 989.90m),
        CreateProduct("63581eac-b46b-44ef-e84f-08df296652de", "Robot Vacuum Cleaner", 9990m, "Lidar navigation and automatic emptying station"),
        CreateProduct("3459be9f-652d-48d3-e850-08df296652de", "Smart Watch", 6990m, "Always-on display, GPS and contactless payments"),
        CreateProduct("6fb0b1f2-b063-407a-e851-08df296652de", "Smartphone 256 GB", 19990m, "6.7-inch OLED display and triple camera"),
        CreateProduct("ed5ca140-6d67-4269-e852-08df296652de", "Tablet 11", 11990m),
        CreateProduct("f76e7c2f-4809-4e79-e853-08df296652de", "USB-C Charger 65 W", 890m, "GaN charger for laptops and phones"),
        CreateProduct("5a6ffa01-4cc6-4ae5-e854-08df296652de", "Webcam Full HD", 1590m),
        CreateProduct("097dac04-7786-46e9-e855-08df296652de", "Wireless Mouse", 690m, "Silent clicks and 18-month battery life"),
    ];

    private static Product CreateProduct(string id, string name, decimal price, string? description = null) =>
        new(Guid.Parse(id), name, new Uri($"https://cdn.example.com/products/{ToSlug(name)}.jpg"), price, description);

    private static string ToSlug(string name) =>
        string.Join('-', name.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
