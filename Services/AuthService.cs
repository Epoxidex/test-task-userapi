using UserAPI.Models;
using UserAPI.Repositories;

namespace UserAPI.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Add JWT
            if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                return null;

            var authValue = authHeader.ToString();
            if (string.IsNullOrEmpty(authValue) || !authValue.StartsWith("Basic "))
                return null;

            try
            {
                var encodedCredentials = authValue.Substring("Basic ".Length).Trim();
                var decodedCredentials = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(encodedCredentials));

                var credentials = decodedCredentials.Split(':', 2);
                if (credentials.Length != 2)
                    return null;

                var login = credentials[0];
                var password = credentials[1];

                return await _userRepository.GetUserByLoginAndPasswordAsync(login, password);
            }
            catch
            {
                return null;
            }
        }

        public bool IsAdmin(User user)
        {
            return user.Admin;
        }

        public bool CanModifyUser(User currentUser, string targetUserLogin)
        {
            return IsAdmin(currentUser) ||
                   currentUser.Login.Equals(targetUserLogin, StringComparison.OrdinalIgnoreCase);
        }
    }
}