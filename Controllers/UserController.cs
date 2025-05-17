using Microsoft.AspNetCore.Mvc;
using UserAPI.Models.DTOs;
using UserAPI.Services;

namespace UserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public UsersController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        // Создание пользователя 
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.IsAdmin(currentUser))
            {
                return Forbid("Только администраторы могут создавать новых пользователей");
            }

            var result = await _userService.CreateUserAsync(createUserDto, currentUser.Login);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetUserByLogin), new { login = result.Data!.Login }, result.Data);
        }

        // Изменение имени, пола или даты рождения пользователя
        [HttpPut("info")]
        public async Task<IActionResult> UpdateUserInfo(UpdateUserInfoDto updateInfoDto)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.CanModifyUser(currentUser, updateInfoDto.Login))
            {
                return Forbid("У вас нет прав для изменения этого пользователя");
            }

            var result = await _userService.UpdateUserInfoAsync(updateInfoDto, currentUser.Login);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Изменение пароля
        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto updatePasswordDto)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            bool isAdmin = _authService.IsAdmin(currentUser);

            if (isAdmin || currentUser.Login.Equals(updatePasswordDto.Login, StringComparison.OrdinalIgnoreCase))
            {
                var result = await _userService.UpdatePasswordAsync(
                    updatePasswordDto.Login,
                    updatePasswordDto.OldPassword,
                    updatePasswordDto.NewPassword,
                    currentUser.Login);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result.Data);
            }

            return Forbid("У вас нет прав для изменения пароля этого пользователя");
        }

        // Изменение логина
        [HttpPut("login")]
        public async Task<IActionResult> UpdateLogin(UpdateLoginDto updateLoginDto)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.CanModifyUser(currentUser, updateLoginDto.OldLogin))
            {
                return Forbid("У вас нет прав для изменения логина этого пользователя");
            }

            var result = await _userService.UpdateLoginAsync(
                updateLoginDto.OldLogin,
                updateLoginDto.NewLogin,
                updateLoginDto.Password,
                currentUser.Login);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Запрос списка всех активных пользователей
        [HttpGet]
        public async Task<IActionResult> GetAllActiveUsers()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.IsAdmin(currentUser))
            {
                return Forbid("Только администраторы могут просматривать список всех пользователей");
            }

            var result = await _userService.GetAllActiveUsersAsync();

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Запрос пользователя по логину
        [HttpGet("{login}")]
        public async Task<IActionResult> GetUserByLogin(string login)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.IsAdmin(currentUser))
            {
                return Forbid("Только администраторы могут просматривать информацию о пользователях");
            }

            var result = await _userService.GetUserByLoginAsync(login);

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Запрос пользователя по логину и паролю
        [HttpPost("auth")]
        public async Task<IActionResult> GetUserByLoginAndPassword(AuthDto authDto)
        {
            var result = await _userService.GetUserByLoginAndPasswordAsync(authDto.Login, authDto.Password);

            if (!result.IsSuccess)
            {
                return Unauthorized(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Запрос всех пользователей старше определённого возраста
        [HttpGet("older-than/{age}")]
        public async Task<IActionResult> GetUsersOlderThan(int age)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.IsAdmin(currentUser))
            {
                return Forbid("Только администраторы могут использовать эту функцию");
            }

            var result = await _userService.GetUsersOlderThanAsync(age);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        // Удаление пользователя
        [HttpDelete("{login}")]
        public async Task<IActionResult> DeleteUser(string login, [FromQuery] bool hardDelete = false)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.IsAdmin(currentUser))
            {
                return Forbid("Только администраторы могут удалять пользователей");
            }

            var result = await _userService.DeleteUserAsync(login, currentUser.Login, hardDelete);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return NoContent();
        }

        // Восстановление пользователя
        [HttpPost("{login}/restore")]
        public async Task<IActionResult> RestoreUser(string login)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Unauthorized("Необходима авторизация");
            }

            if (!_authService.IsAdmin(currentUser))
            {
                return Forbid("Только администраторы могут восстанавливать пользователей");
            }

            var result = await _userService.RestoreUserAsync(login);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok();
        }
    }
}