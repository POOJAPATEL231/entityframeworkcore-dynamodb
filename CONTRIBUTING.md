# Contributing

Thanks for your interest!

## Getting started

1. Install the .NET 8 SDK.
2. `dotnet build` and `dotnet test` must both pass before and after your change.

## Guidelines

- **Tests required** — bug fixes need a regression test that fails without the fix;
  new features need happy-path plus principal edge-case coverage.
- **Layering** — `EntityFrameworkCore.DynamoDb.Abstractions` holds base entities,
  the unit of work, events and cross-cutting abstractions; the main package holds the
  DynamoDB implementation. Keep AWS-specific code out of the abstractions package.
- **Style** — follow `.editorconfig`; match surrounding conventions; add XML doc comments
  to new public types.
- **Scope** — this is an EF-*style* API over the AWS SDK, not an EF Core provider. Keep
  the public surface close to EF Core's shapes where it makes sense.

## Reporting issues

Open a GitHub issue with a repro, expected vs. actual behavior, and versions. For LINQ
translation bugs, include the expression and the generated filter/key-condition expression.
