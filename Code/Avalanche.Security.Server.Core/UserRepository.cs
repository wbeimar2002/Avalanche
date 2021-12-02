using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Ism.Common.Core.Aspects;
using Avalanche.Security.Server.Core.Entities;
using Avalanche.Security.Server.Core.Interfaces;
using Avalanche.Security.Server.Core.Models;
using Ism.Storage.Core.Infrastructure.Exceptions;
using Ism.Storage.Core.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;
using static Ism.Utility.Core.Preconditions;
using EFCore.BulkExtensions;

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
        private readonly DataManagementContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly IMapper _mapper;
        private readonly IValidator<UserModel> _validator;
        private readonly IDatabaseWriter<DataManagementContext> _writer;
        private bool _disposedValue;

        public UserRepository(
            ILogger<UserRepository> logger,
            IMapper mapper,
            DataManagementContext context,
            IDatabaseWriter<DataManagementContext> writer,
            IValidator<UserModel> validator
        )
        {
            _logger = ThrowIfNullOrReturn(nameof(logger), logger);
            _mapper = ThrowIfNullOrReturn(nameof(mapper), mapper);
            _context = ThrowIfNullOrReturn(nameof(context), context);
            _writer = ThrowIfNullOrReturn(nameof(writer), writer);
            _validator = ThrowIfNullOrReturn(nameof(validator), validator);
        }

        [AspectLogger]
        public async Task<UserModel> AddUser(UserModel user)
        {
            ThrowIfNull(nameof(user), user);
            _validator.ValidateAndThrow(user);

            try
            {
                var entity = _mapper.Map<UserEntity>(user);
                var added = await AddUserEntity(entity).ConfigureAwait(false);
                return _mapper.Map<UserModel>(added);
            }
            catch (DbUpdateException ex) when (ex.InnerException.Message.Contains("UNIQUE Constraint Failed", StringComparison.OrdinalIgnoreCase))
            {
                var id = EqualityComparer<int>.Default.Equals(user.Id) ? "<Id Not Set>" : user.Id.ToString(CultureInfo.InvariantCulture);
                throw new DuplicateEntityException(typeof(UserModel), id, nameof(UserModel.UserName), user.UserName!, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddUser)} failed for {nameof(UserModel)} with {nameof(UserModel.UserName)} = {user.UserName}{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task<int> DeleteUser(int userId)
        {
            try
            {
                Task<int> writerFunction(DataManagementContext context) =>
                    DeleteUserWriter(userId, context);

                return await _writer.Write(writerFunction).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(DeleteUser)} failed for {nameof(UserEntity)} with {nameof(UserEntity.Id)} {userId}{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task AddOrUpdateUser(UserModel User)
        {
            ThrowIfNull(nameof(User), User);
            _validator.ValidateAndThrow(User);

            try
            {
                var entity = _mapper.Map<UserEntity>(User);

                await ThrowIfDuplicate(entity, true).ConfigureAwait(false);

                // perform update
                _ = await UpdateUserEntity(entity).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AddOrUpdateUser)} failed for {nameof(User)} with {nameof(User.UserName)} = {User.UserName}{Environment.NewLine}Error: {ex.Message}");
                throw;
            }
        }

        [AspectLogger]
        public async Task<IEnumerable<UserModel>> GetAllUsers()
        {
            var result = await _context.Users
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
            return _mapper.Map<IList<UserEntity>, IList<UserModel>>(result);
        }

        [AspectLogger]
        public async Task<UserModel?> GetUser(string userName)
        {
            var entity = await GetUserEntity(userName).ConfigureAwait(false);

            if (entity != null)
            {
                return _mapper.Map<UserModel>(entity);
            }

            _logger.LogWarning($"{nameof(GetUser)} failed to retrieve a {nameof(UserModel)} with {nameof(UserModel.UserName)} {userName}");
            return null;
        }

        private static async Task<UserEntity> AddUserWriter(UserEntity userEntity, DataManagementContext context)
        {
            var result = await context.Users
                .AddAsync(userEntity)
                .ConfigureAwait(false);

            _ = await context.SaveChangesAsync().ConfigureAwait(false);
            return result.Entity;
        }

        private static async Task<int> DeleteUserWriter(int id, DataManagementContext context) =>
            _ = await context.Users
                .Where(x => x.Id == id)
                .AsNoTracking()
                .DeleteAsync().ConfigureAwait(false);

        private Task<UserEntity> AddUserEntity(UserEntity userEntity)
        {
            Task<UserEntity> writerFunction(DataManagementContext context) => AddUserWriter(userEntity, context);
            return _writer.Write(writerFunction);
        }

        private async Task<UserEntity?> GetUserEntity(string userName) =>
            await _context.Users
                .Where(x => x.UserName == userName)
                .AsNoTracking()
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);


        private Task<UserEntity> UpdateUserEntity(UserEntity UserEntity)
        {
            Task<UserEntity> writerFunction(DataManagementContext context) => AddOrUpdateWriter(UserEntity, context);
            return _writer.Write(writerFunction);
        }

        private async Task ThrowIfDuplicate(UserEntity entity, bool checkUpdate)
        {
            // Check for existing data
            var query = _context.Users.Where(
                x => x.UserName == entity.UserName
            );

            if (checkUpdate)
            {
                query = query.Where(x => x.Id != entity.Id);
            }

            var existing = await query.FirstOrDefaultAsync().ConfigureAwait(false);

            // Throw exception if duplicate exists
            if (existing != null)
            {
                var id = EqualityComparer<int>.Default.Equals(entity.Id) ? "<Id Not Set>" : entity.Id.ToString(CultureInfo.InvariantCulture);
                throw new DuplicateEntityException(typeof(UserModel), id, nameof(UserModel.UserName), entity.UserName!);
            }
        }

        private static async Task<UserEntity> AddOrUpdateWriter(UserEntity User, DataManagementContext context)
        {
            // Normally we would have to lock around dependent operations
            // But because this class orchestrates it's writes through a single threaded DatabaseWriter we can skip that overhead

            // No need to deal with logic around updating detatched entities and children.  Just delete the existing index if it already exists
            _ = await context.Users
                .Where(x => x.Id == User.Id)
                .BatchDeleteAsync()
                .ConfigureAwait(false);

            User = context.Users.Add(User).Entity;
            _ = await context.SaveChangesAsync().ConfigureAwait(false);

            return User;
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
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposedValue = true;
            }
        }
        #endregion Disposable
    }
}

