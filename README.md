# EntityFrameworkCore.DynamoDb

An **Entity Framework Core-style API over Amazon DynamoDB** for .NET — the familiar
`DbContext` / `DbSet<T>` / LINQ / change-tracking / `SaveChangesAsync` programming
model that AWS doesn't ship, inspired by how `Microsoft.EntityFrameworkCore.Cosmos`
maps EF Core onto Cosmos DB.

> **Not an official EF Core provider.** This is an EF-Core-*style* API implemented over
> the AWS SDK (`AWSSDK.DynamoDBv2`). It mirrors EF Core's shapes so the code feels
> familiar, but it does not plug into EF Core's provider/model pipeline. Entity
> *configuration* is expressed with EF's `ModelBuilder` (via a small adapter) so
> `HasPartitionKey`, `HasKey`, `HasIndex` and value converters read naturally.

## Why

DynamoDB's official .NET experience is the low-level document/object-persistence model.
If you like EF Core's ergonomics — a context with typed sets, LINQ predicates, a unit
of work, and transactional saves — this brings that to DynamoDB.

## Features

- **`BaseDynamoDbContext` + `IDynamoDbSet<T>`** — a context per service, one set per aggregate
- **LINQ predicate translation** — `==`, `!=`, `<`, `<=`, `>`, `>=`, `&&`, `||`,
  `StartsWith`, `Contains`, list-`Contains` (→ `IN`), `Between`, null checks
  (→ `attribute_exists`/`attribute_not_exists`), reserved-word aliasing, and automatic
  **Query-vs-Scan** selection
- **Global Secondary Indexes** — declare with EF `HasIndex(...)`; matching predicates run
  as index Queries instead of table Scans, and tables are created with their GSIs
- **Change tracking + Unit of Work** — `SaveChangesAsync` writes an atomic
  `TransactWriteItems`, with audit stamping and MediatR domain-event dispatch
- **Optimistic concurrency** — ETag stamps rotate on every write; conflicts throw
  `DynamoDbConcurrencyException`
- **Transactional outbox** — stage integration events in the same atomic save; a hosted
  dispatcher publishes them afterwards
- **Distributed lock** — `IDistributedLock` on DynamoDB conditional writes (leader
  election / run-once guards)
- **Table auto-creation + TTL**, field-level **encryption** via a pluggable
  `ICryptoProvider`, and **LocalStack**-aware client registration for local dev

## Install

```bash
dotnet add package EntityFrameworkCore.DynamoDb
```

(`EntityFrameworkCore.DynamoDb.Abstractions` — base entities, unit of work, events —
comes along as a dependency.)

## Quick start

```csharp
// 1. Entity — derive from DocEntity
public class Order : DocEntity, IAggregateRoot
{
    public override string PartitionKey { get; set; } = "order";
    public string CustomerEmail { get; set; } = "";
    public int Quantity { get; set; }
}

// 2. Configure it EF-style (adapter maps this onto DynamoDB)
public class OrderConfiguration : DocEntityConfiguration<Order>
{
    protected override string ContainerName => "orders";
    public override void Configure(EntityTypeBuilder<Order> b)
    {
        base.Configure(b);
        b.HasIndex(e => e.CustomerEmail);   // → a GSI; queries on CustomerEmail become index Queries
    }
}

// 3. Context — one IDynamoDbSet per aggregate
public class ShopContext : BaseDynamoDbContext
{
    public ShopContext(IServiceProvider sp, ICurrentUser u, IDateTime clock, IMediator m)
        : base(sp, u, clock, m) { }
    public IDynamoDbSet<Order> Orders => Set<Order>();
}

// 4. Register + create tables
builder.Services.AddPersistenceDynamoDb<ShopContext>(builder.Configuration, builder.Environment);
await app.UsePersistenceDynamoAsync<ShopContext>(new DynamoDbRepositoryOptions());

// 5. Use it
var order = await context.Orders.FirstOrDefaultAsync(o => o.CustomerEmail == "alice@example.com");
order.Quantity += 1;                 // change tracking picks this up
await context.SaveChangesAsync();    // atomic TransactWriteItems + ETag concurrency
```

### Optional building blocks

```csharp
builder.Services.AddDynamoDbOutbox();            // transactional outbox + dispatcher
builder.Services.AddDynamoDbDistributedLock();   // IDistributedLock
```

## Running against LocalStack

Point the AWS SDK at LocalStack via `AWS:ServiceURL` (`http://localhost:4566`) with dummy
credentials, or use the AWS SDK's native endpoint support. See the parent project's sample
for a full end-to-end setup verified against LocalStack.

## Packages

| Package | Contents |
|---|---|
| **EntityFrameworkCore.DynamoDb** | the context/sets/LINQ translation, change tracking, table provider, outbox, lock |
| **EntityFrameworkCore.DynamoDb.Abstractions** | `DocEntity`/`BaseEntity`, `IUnitOfWork`, domain/integration events, `IDistributedLock`, `ICryptoProvider` |

## Status & roadmap

Extracted from a larger AWS-common library and verified end to end against LocalStack.

- [x] GSI query support, ETag optimistic concurrency, transactional outbox, distributed lock
- [ ] Value-converter read path (`ConvertFromProvider`)
- [ ] TTL attribute mapping (`ttl`) wiring
- [ ] `OrderBy`/`Take`/projection support in the LINQ translator

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Bug reports and PRs welcome.

## License

[MIT](LICENSE)
