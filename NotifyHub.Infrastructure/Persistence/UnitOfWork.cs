using Dapper;
using NotifyHub.Application.Interfaces;
using NotifyHub.Application.Interfaces.Repositories;
using NotifyHub.Infrastructure.Persistence.Repositories;
using Npgsql;
using System.Data;

namespace NotifyHub.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DapperContext _context;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public INotificationRepository Notifications { get; private set; }
    public IUserRepository User { get; private set; }
    public ITenantRepository Tenant { get; private set; }

    public UnitOfWork(DapperContext context)
    {
        _context = context;

        // Each repository gets the same connection
        // so they share the same transaction
        _connection = _context.CreateConnection();
        _connection.Open();

        Notifications = new NotificationRepository(_connection);
        User = new UserRepository(_connection);
        Tenant = new TenantRepository(_connection);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = _connection!.BeginTransaction();
        await Task.CompletedTask;
    }

    public async Task CommitAsync()
    {
        _transaction?.Commit();
        await Task.CompletedTask;
    }

    public async Task RollbackAsync()
    {
        _transaction?.Rollback();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
    }
}