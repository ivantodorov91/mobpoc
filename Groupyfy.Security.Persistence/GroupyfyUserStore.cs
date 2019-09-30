using Groupyfy.Security.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Groupyfy.Security.Persistence
{
    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    public class GroupyfyUserStore : 
        UserStore<
            GroupyfyUser, 
            GroupyfyRole, 
            GroupyfySecurityDbContext,
            Guid, 
            IdentityUserClaim<Guid>, 
            GroupyfyUserRole, 
            IdentityUserLogin<Guid>, 
            IdentityUserToken<Guid>, 
            IdentityRoleClaim<Guid>>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="describer"></param>
        public GroupyfyUserStore(GroupyfySecurityDbContext context, IdentityErrorDescriber describer = null)
            :base(context, describer)
        {

        }

        private DbSet<GroupyfyUserRole> UserRoles { get { return Context.Set<GroupyfyUserRole>(); } }

        /// <summary>
        /// Retrieves the roles the specified <paramref name="user"/> is a member of.
        /// </summary>
        /// <param name="user">The user whose roles should be retrieved.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
        public async Task<IList<KeyValuePair<string, Guid?>>> GetGroupyfyRolesAsync(GroupyfyUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return await Context.UserRoles
                .Join(Context.Roles, ur => ur.RoleId, r => r.Id, (userRole, role) => new { UserRole = userRole, Role = role })
                .Where(x => x.UserRole.RoleId == x.Role.Id && x.UserRole.UserId == user.Id)
                .Select(x => new KeyValuePair<string, Guid?>(x.Role.Name, x.UserRole.CorporateId))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds the given <paramref name="normalizedRoleName"/> to the specified <paramref name="user"/> with optional <paramref name="corporateId"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedRoleName"></param>
        /// <param name="corporateId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task AddToGroupyfyRoleAsync(GroupyfyUser user, string normalizedRoleName, Guid? corporateId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("ValueCannotBeNullOrEmpty", nameof(normalizedRoleName));
            }
            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "RoleNotFound", normalizedRoleName));
            }

            await Context.UserRoles.AddAsync(new GroupyfyUserRole(user.Id, roleEntity.Id, corporateId));
        }

        /// <summary>
        /// Returns a flag indicating if the specified user is a member of the give <paramref name="normalizedRoleName"/>.
        /// </summary>
        /// <param name="user">The user whose role membership should be checked.</param>
        /// <param name="normalizedRoleName">The role to check membership of</param>
        /// <param name="CorporateId">The corporate id to filter with</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user is a member of the given group. If the
        /// user is a member of the group the returned value with be true, otherwise it will be false.</returns>
        public async Task<bool> IsInGroupyfyRoleAsync(GroupyfyUser user, string normalizedRoleName, Guid? corporateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("ValueCannotBeNullOrEmpty", nameof(normalizedRoleName));
            }
            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (role != null)
            {
                var userRole = await FindGroupyfyUserRoleAsync(user.Id, role.Id, corporateId, cancellationToken);
                return userRole != null;
            }
            return false;
        }

        /// <summary>
        /// Return a user role for the userId and roleId if it exists.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <param name="roleId">The role's id.</param>
        /// <param name="corporateId">The id of the corporate associated with the role.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The user role if it exists.</returns>
        protected Task<GroupyfyUserRole> FindGroupyfyUserRoleAsync(Guid userId, Guid roleId, Guid? corporateId, CancellationToken cancellationToken)
            => Context.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId && x.CorporateId == corporateId, cancellationToken);
    }
}
