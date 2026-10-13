using AlzaProducts.Application.Paging;
using AlzaProducts.Application.Products;
using AlzaProducts.Domain;
using AlzaProducts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlzaProducts.Infrastructure.Products;

/// <summary>
/// Products stored in SQL Server, read and written through EF Core
/// </summary>
public sealed class SqlProductRepository(AlzaProductsDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await OrderedProducts().ToListAsync(cancellationToken);

    public async Task<PagedResult<Product>> GetPageAsync(PageRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var totalCount = await context.Products.CountAsync(cancellationToken);
        var items = await OrderedProducts()
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<Product>(items, request.Page, request.PageSize, totalCount);
    }

    // Tracked by EF Core, so UpdateAsync saves the changes made to the returned product
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Products.FindAsync([id], cancellationToken);

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        // A product from GetByIdAsync is tracked already; any other instance is attached as changed
        if (context.Entry(product).State == EntityState.Detached)
        {
            context.Products.Update(product);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // No row was updated, so the product does not exist (anymore)
            throw new InvalidOperationException($"Product {product.Id} does not exist", exception);
        }
    }

    // Lists are only read, so EF Core does not need to track them; sorted like the mock data source
    private IQueryable<Product> OrderedProducts() =>
        context.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id);
}
