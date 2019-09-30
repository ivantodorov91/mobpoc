using Groupyfy.Security.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Groupyfy.Security.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupyfyUserManager : UserManager<GroupyfyUser>
    {
        /// <summary>
        /// 
        /// </summary>
        //public GroupyfyRoleUserStore GroupyfyRoleUserStore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupyfyRoleStore"></param>
        /// <param name="store"></param>
        /// <param name="optionsAccessor"></param>
        /// <param name="passwordHasher"></param>
        /// <param name="userValidators"></param>
        /// <param name="passwordValidators"></param>
        /// <param name="keyNormalizer"></param>
        /// <param name="errors"></param>
        /// <param name="services"></param>
        /// <param name="logger"></param>
        public GroupyfyUserManager(
            IUserStore<GroupyfyUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<GroupyfyUser> passwordHasher,
            IEnumerable<IUserValidator<GroupyfyUser>> userValidators,
            IEnumerable<IPasswordValidator<GroupyfyUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<GroupyfyUser>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            //GroupyfyRoleUserStore = groupyfyRoleStore;
        }

        /// <summary>
        /// Gets a list of role names the specified <paramref name="user"/> belongs to.
        /// </summary>
        /// <param name="user">The user whose role names to retrieve.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a list of role names.</returns>
        public async Task<IList<KeyValuePair<string, Guid?>>> GetGroupyfyRolesAsync(GroupyfyUser user, bool hasPassword = true)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await GetUserRoleStore().GetGroupyfyRolesAsync(user, hasPassword, CancellationToken);
        }

        /// <summary>
        /// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the give named role.
        /// </summary>
        /// <param name="user">The user whose role membership should be checked.</param>
        /// <param name="role">The name of the role to be checked.</param>
        /// <param name="corporateId">The id of the corporate if applicable.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <paramref name="user"/> is
        /// a member of the named role.
        /// </returns>
        public async Task<bool> IsInGroupyfyRoleAsync(GroupyfyUser user, string role, Guid? corporateId)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await GetUserRoleStore().IsInGroupyfyRoleAsync(user, NormalizeKey(role), corporateId, CancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <param name="corporateId"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddToGroupyfyRoleAsync(GroupyfyUser user, string role, Guid? corporateId = null)
        {
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var normalizedRole = NormalizeKey(role);
            if (await GetUserRoleStore().IsInGroupyfyRoleAsync(user, normalizedRole, corporateId, CancellationToken))
            {
                return await UserAlreadyInRoleError(user, role);
            }

            await GetUserRoleStore().AddToGroupyfyRoleAsync(user, normalizedRole, corporateId, CancellationToken);
            return await UpdateUserAsync(user);
        }

        /// <summary>
        /// Generates an offer login token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate an email confirmation token for.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, an email confirmation token.
        /// </returns>
        public Task<string> GenerateOfferTokenAsync(GroupyfyUser user, Guid offerId)
        {
            ThrowIfDisposed();
            
            return GenerateUserTokenAsync(user, "offertoken", offerId.ToString());
        }

        /// <summary>
        /// Returns a flag indicating whether the specified <paramref name="user"/>'s offer
        /// token is valid for the given <paramref name="offerId"/>.
        /// </summary>
        /// <param name="user">The user to validate the token against.</param>
        /// <param name="token">The telephone number change token to validate.</param>
        /// <param name="offerId">The offer id associated with the token.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, returning true if the <paramref name="token"/>
        /// is valid, otherwise false.
        /// </returns>
        public virtual Task<bool> VerifyOfferTokenAsync(GroupyfyUser user, string token, Guid offerId)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return VerifyUserTokenAsync(user, "offertoken", offerId.ToString(), token);
        }

        private async Task<IdentityResult> UserAlreadyInRoleError(GroupyfyUser user, string role)
        {
            Logger.LogWarning(5, "User {userId} is already in role {role}.", await GetUserIdAsync(user), role);
            return IdentityResult.Failed(ErrorDescriber.UserAlreadyInRole(role));
        }

        private GroupyfyUserStore GetUserRoleStore()
        {
            var cast = Store as GroupyfyUserStore;
            if (cast == null)
            {
                throw new NotSupportedException("StoreNotIUserRoleStore");
            }
            return cast;
        }
    }
}
