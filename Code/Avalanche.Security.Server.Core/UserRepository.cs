using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Avalanche.Security.Server.Core.Entities;
using Avalanche.Security.Server.Core.Exceptions;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Avalanche.Shared.Infrastructure.Security.Hashing;
using FluentValidation;
using Ism.Common.Core.Aspects;
using Ism.Storage.Core.Infrastructure.Extensions;
using Ism.Storage.Core.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;
using static Ism.Utility.Core.Preconditions;

namespace Avalanche.Security.Server.Core
{
    // Design conventions for UsersRepository
    // 1. Public Methods accept Model types from Avalanche.Security.Server.Core.Models
    // 2. If needed, public methods use automapper to convert public Model types to EF Entity Types and call a private method which takes the Entity type
    // 3. Public Methods provide precondition checks of all inputs
    // 4. Public Methods provide top level exception logging (not handling) with useful log messages
    // 5. Methods which need to update the database declare a Func by closing over a "Writer" method and pass it to the IDatabaseWriter for execution
    // 6. Writer methods are private & static and one of their parameters must be a DataManagementContext

    public class UserRepository : IUserRepository, IDisposable
    {
        public const int MinSearchTermLength = 2;
        private const string EmptySearchExpression = "<None>";

        private readonly SecurityDbContext _context;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);
        private readonly ILogger<UserRepository> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<NewUserModel> _newUserValidator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IValidator<UpdateUserPasswordModel> _updateUserPasswordValidator;
        private readonly IValidator<UpdateUserModel> _updateUserValidator;
        private readonly IDatabaseWriter<SecurityDbContext> _writer;
        private bool _disposedValue;

        public UserRepository(
            ILogger<UserRepository> logger,
            IMapper mapper,
            SecurityDbContext context,
            IDatabaseWriter<SecurityDbContext> writer,
            IValidator<NewUserModel> newUserValidator,
            IValidator<UpdateUserModel> updateUserValidator,
            IValidator<UpdateUserPasswordModel> updateUserPasswordValidator,
            IPasswordHasher passwordHasher
        )
        {
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _context = ThrowIfNullOrReturn(nameof(context), context);
            _writer = ThrowIfNullOrReturn(nameof(writer), writer);
            _newUserValidator = ThrowIfNullOrReturn(nameof(newUserValidator), newUserValidator);
            _updateUserValidator = ThrowIfNullOrReturn(nameof(updateUserValidator), updateUserValidator);
            _updateUserPasswordValidator = ThrowIfNullOrReturn(nameof(updateUserPasswordValidator), updateUserPasswordValidator);
            _passwordHasher = ThrowIfNullOrReturn(nameof(passwordHasher), passwordHasher);
        }

        [AspectLogger]
        public async Task<UserModel> AddUser(NewUserModel user)
        {
            ThrowIfNull(nameof(user), user);
            _newUserValidator.ValidateAndThrow(user);

            try
            {
                var entity = new UserEntity
                {
                    UserName = user.UserName,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    PasswordHash = _passwordHasher.HashPassword(user.Password)
                };

                var added = await AddUserEntity(entity).ConfigureAwait(false);

                return _mapper.Map<UserModel>(added);
            }
            catch (DbUpdateException ex) when (ex.InnerException.Message.Contains("UNIQUE Constraint Failed", StringComparison.OrdinalIgnoreCase))
            {
                throw new DuplicateEntityException(typeof(UserEntity), "<Id Not Set>", nameof(UserEntity.UserName), user.UserName, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddUser)} failed for {nameof(NewUserModel)} with {nameof(NewUserModel.UserName)} = '{user.UserName}'{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task<int> DeleteUser(int userId)
        {
            ThrowIfNullOrDefault(nameof(userId), userId);

            try
            {
                Task<int> writerFunction(SecurityDbContext context) =>
                    DeleteUserWriter(userId, context);

                return await _writer.Write(writerFunction).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteUser)} failed for {nameof(UserEntity)} with {nameof(UserEntity.Id)} = '{userId}'{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task<UserModel?> GetUser(string userName)
        {
            var entity = await GetUserEntity(userName).ConfigureAwait(false);

            if (entity != null)
            {
                return _mapper.Map<UserModel>(entity);
            }

            _logger.LogWarning($"{nameof(GetUser)} failed to retrieve a {nameof(UserModel)} with {nameof(UserModel.UserName)} = '{userName}'");
            return null;
        }

        [AspectLogger]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            var result = await _context.Users
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return _mapper.Map<IList<UserEntity>, IList<UserModel>>(result);
        }

        [AspectLogger]
        public async Task<IEnumerable<UserModel>> SearchUsers(string keyword)
        {
            var baseFtsQuery = _context.UsersFts;
            IQueryable<UserEntity> userQuery;
            var searchExpression = (keyword?.Length ?? 0) > 2 ? FormatAsMatchExpression(keyword) : EmptySearchExpression;

            _logger.LogDebug($"Search Expression = {searchExpression}");

            try
            {
                if (searchExpression != EmptySearchExpression)
                {
                    // Define query for FTS keyword search
                    userQuery = baseFtsQuery
                        .Where(x => x.Match == searchExpression)
                        .Select(x => x.User);
                }
                else
                {
                    // Otherwise just query Users directly
                    userQuery = _context.Users;
                }

                var entities = await userQuery
                    .OrderBy(GetSortingColumns(), false)
                    .ToListAsync()
                    .ConfigureAwait(false);

                var results = _mapper.Map<IList<UserEntity>, IList<UserModel>>(entities);

                _logger.LogDebug($"Found {results.Count} result(s) for Search Expression = {searchExpression}");

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SearchUsers)} failed for search expression {searchExpression} with Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task UpdateUser(UpdateUserModel update)
        {
            ThrowIfNull(nameof(update), update);
            _updateUserValidator.ValidateAndThrow(update);

            try
            {
                var userToUpdate = await GetUserEntityAndThrowIfNull(update.UserName).ConfigureAwait(false);
                userToUpdate.UserName = update.UserName;
                userToUpdate.FirstName = update.FirstName;
                userToUpdate.LastName = update.LastName;

                _ = await UpdateUserEntity(userToUpdate).ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (ex.InnerException.Message.Contains("UNIQUE Constraint Failed", StringComparison.OrdinalIgnoreCase))
            {
                throw new DuplicateEntityException(typeof(UserEntity), update.Id.ToString(CultureInfo.InvariantCulture), nameof(UserEntity.UserName), update.UserName, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateUser)} failed for {nameof(update)} with {nameof(update.UserName)} = '{update.UserName}'{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task UpdateUserPassword(UpdateUserPasswordModel passwordUpdate)
        {
            ThrowIfNull(nameof(passwordUpdate), passwordUpdate);
            _updateUserPasswordValidator.ValidateAndThrow(passwordUpdate);

            try
            {
                var userToUpdate = await GetUserEntityAndThrowIfNull(passwordUpdate.UserName).ConfigureAwait(false);
                userToUpdate.PasswordHash = _passwordHasher.HashPassword(passwordUpdate.Password);
                _ = await UpdateUserEntity(userToUpdate).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateUserPassword)} failed for User with {nameof(passwordUpdate.UserName)} = '{passwordUpdate.UserName}'{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        private static async Task<UserEntity> AddUserWriter(UserEntity userEntity, SecurityDbContext context)
        {
            var result = await context.Users
                .AddAsync(userEntity)
                .ConfigureAwait(false);

            _ = await context.SaveChangesAsync().ConfigureAwait(false);
            return result.Entity;
        }

        private static async Task<int> DeleteUserWriter(int id, SecurityDbContext context) =>
            _ = await context.Users
                .Where(x => x.Id == id)
                .AsNoTracking()
                .DeleteAsync().ConfigureAwait(false);

        private static string FormatAsMatchExpression(string searchTerm) =>
            // Takes a enumerable of strings i.e. ["one", "two"]
            // And formats them as a Sqlite FTS5 MATCH expression, i.e. '"one"* "two*"'
            // https://www.sqlite.org/fts5.html
            $"\"{searchTerm.Replace("\"", "\"\"", StringComparison.Ordinal)}\"*";

        private static async Task<UserEntity> UpdateWriter(UserEntity userEntity, SecurityDbContext context)
        {
            var updated = context.Users
                .Update(userEntity);

            _ = await context.SaveChangesAsync().ConfigureAwait(false);
            return updated.Entity;
        }

        private Task<UserEntity> AddUserEntity(UserEntity userEntity)
        {
            Task<UserEntity> writerFunction(SecurityDbContext context) => AddUserWriter(userEntity, context);
            return _writer.Write(writerFunction);
        }

        private List<string> GetSortingColumns()
        {
            var entityType = _context.Model.FindEntityType(typeof(UserEntity));
            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);

            return new List<string>
            {
                entityType.GetProperties().Select(x => x.GetColumnName(storeObjectIdentifier)).First(x => x == nameof(UserEntity.LastName)),
                entityType.GetProperties().Select(x => x.GetColumnName(storeObjectIdentifier)).First(x => x == nameof(UserEntity.FirstName))
            };
        }

        private async Task<UserEntity?> GetUserEntity(string userName) =>
                    await _context.Users
                .Where(x => x.UserName == userName)
                .AsNoTracking()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

        private async Task<UserEntity> GetUserEntityAndThrowIfNull(string userName)
        {
            var user = await GetUserEntity(userName).ConfigureAwait(false);
            if (user == null)
            {
                throw new InvalidOperationException($"Cannot {nameof(GetUserEntity)} for User with {nameof(userName)} = '{userName}' because it does not already exist");
            }

            return user;
        }
        private async Task<UserEntity> UpdateUserEntity(UserEntity userEntity)
        {
            try
            {
                // Lock around checking for existing entity and then updating it
                await _locker.WaitAsync().ConfigureAwait(false);

                var existing = await _context.Users
                .Where(x => x.Id == userEntity.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

                if (existing == null)
                {
                    throw new InvalidOperationException($"Cannot update {nameof(UserEntity)} with Id '{userEntity.Id}' because it does not already exist");
                }

                Task<UserEntity> writerFunction(SecurityDbContext context) => UpdateWriter(userEntity, context);
                return await _writer.Write(writerFunction).ConfigureAwait(false);
            }
            finally
            {
                _ = _locker.Release();
            }
        }
        #region Disposable
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    _context.Dispose();
                    _locker.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposedValue = true;
            }
        }
        #endregion Disposable
    }
}
