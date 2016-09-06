using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace PingYourPackage.Domain.Entities.Service
{
    public class MembershipService : IMembershipService
    {
        private readonly IEntityRepository<User> _userRepository;
        private readonly IEntityRepository<Role> _roleRepository;
        private readonly IEntityRepository<UserInRole> _userInRoleRepository;
        private readonly ICryptoService _cryptoService;

        public MembershipService(
            IEntityRepository<User> userRepository, 
            IEntityRepository<Role> roleRepository,
            IEntityRepository<UserInRole> userInRoleRepository,
            ICryptoService cryptoService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userInRoleRepository = userInRoleRepository;
            _cryptoService = cryptoService;
        }

        public ValidUserContext ValidateUser(string username, string password)
        {
            var userCtx = new ValidUserContext();
            var user = _userRepository.GetSingleByUsername(username);
            if (user != null && isUserValid(user, password))
            {
                var userRoles = GetUserRoles(user.Key);
                userCtx.User = new UserWithRoles()
                {
                    User = user,
                    Roles = userRoles
                };

                var identity = new GenericIdentity(user.Name);
                userCtx.Principal = new GenericPrincipal(identity, userRoles.Select(x => x.Name).ToArray());
            }
            return userCtx;
        }

        private bool isUserValid(User user, string password)
        {
            if (isPasswordValid(user, password))
            {
                return !user.IsLocked;
            }

            return false;
        }

        private bool isPasswordValid(User user, string password)
        {
            return string.Equals(_cryptoService.EncryptPassword(password, user.Salt), user.HashedPassword);
        }

        private IEnumerable<Role> GetUserRoles(Guid userKey)
        {
            var userInRoles = _userInRoleRepository.FindBy(x => x.User.Key == userKey).ToList();

            if (userInRoles != null && userInRoles.Count > 0)
            {
                var userRoleKeys = userInRoles.Select(x => x.RoleKey).ToArray();
                var userRoles = _roleRepository.FindBy(x => userRoleKeys.Contains(x.Key));

                return userRoles;
            }

            return Enumerable.Empty<Role>();
        }

        public OperationResult<UserWithRoles> CreateUser(string username, string email, string password)
        {
            return CreateUser(username, password, email, roles: null);
        }

        public OperationResult<UserWithRoles> CreateUser(string username, string email, string password, string role)
        {
            return CreateUser(username, password, email, roles: new[] { role });
        }

        public OperationResult<UserWithRoles> CreateUser(string username, string email, string password, string[] roles)
        {
            var existingUser = _userRepository.GetAll().Any(x => x.Name == username);

            if (existingUser)
            {
                return new OperationResult<UserWithRoles>(false);
            }

            var passwordSalt = _cryptoService.GenerateSalt();

            var user = new User()
            {
                Name = username,
                Salt = passwordSalt,
                Email = email,
                IsLocked = false,
                HashedPassword = _cryptoService.EncryptPassword(password, passwordSalt),
                CreateOn = DateTime.Now
            };

            _userRepository.Add(user);
            _userRepository.Save();

            if (roles != null || roles.Length > 0)
            {
                foreach (var roleName in roles)
                {
                    addUserToRole(user, roleName);
                }
            }

            return new OperationResult<UserWithRoles>(true) {
                Entity = GetUserWithRoles(user)
            };
        }

        private void addUserToRole(User user, string roleName)
        {
            var role = _roleRepository.GetSingleByRoleName(roleName);

            if (role == null)
            {
                var tempRole = new Role()
                {
                    Name = roleName
                };

                _roleRepository.Add(tempRole);
                _roleRepository.Save();
                role = tempRole;
            }

            var userInRole = new UserInRole()
            {
                RoleKey = role.Key,
                UserKey = user.Key
            };

            _userInRoleRepository.Add(userInRole);
            _userInRoleRepository.Save();
        }

        private UserWithRoles GetUserWithRoles(User user)
        {
            if (user != null)
            {
                var userRoles = GetUserRoles(user.Key);
                return new UserWithRoles()
                {
                    User = user,
                    Roles = userRoles
                };
            }

            return null;
        }

        public UserWithRoles UpdateUser(User user, string username, string email)
        {
            throw new NotImplementedException();
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public bool AddToRole(Guid userKey, string role)
        {
            throw new NotImplementedException();
        }

        public bool AddToRole(string username, string role)
        {
            throw new NotImplementedException();
        }

        public bool RemoveFromRole(string username, string role)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Role> GetRoles()
        {
            throw new NotImplementedException();
        }

        public Role GetRole(Guid key)
        {
            throw new NotImplementedException();
        }

        public Role GetRole(string name)
        {
            throw new NotImplementedException();
        }

        public PaginatedList<UserWithRoles> GetUsers(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public UserWithRoles GetUser(Guid key)
        {
            throw new NotImplementedException();
        }

        public UserWithRoles GetUser(string name)
        {
            throw new NotImplementedException();
        }
    }
}
