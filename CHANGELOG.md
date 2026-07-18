# Changelog

All notable changes to this project are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added
- Initial release: an EF Core-style API over Amazon DynamoDB.
  - `BaseDynamoDbContext`, `IDynamoDbSet<T>`, LINQ predicate translation, change
    tracking, transactional `SaveChangesAsync`.
  - Global Secondary Index support (declare with EF `HasIndex`, query via `IndexName`).
  - Optimistic concurrency via ETag conditional writes (`DynamoDbConcurrencyException`).
  - Transactional outbox (`AddDynamoDbOutbox`) and DynamoDB-backed distributed lock
    (`AddDynamoDbDistributedLock`).
  - Table auto-creation with GSIs and TTL, field-level encryption via `ICryptoProvider`.
  - `EntityFrameworkCore.DynamoDb.Abstractions` foundation package.
- 44 unit tests covering LINQ translation, entity conversion, change tracking,
  pagination, transactions, GSI promotion, ETag concurrency, the distributed lock and
  the transactional outbox.
