using UserAPI.Models;
using UserAPI.Models.DTOs;
using UserAPI.Repositories;

namespace UserAPI.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ServiceResult<UserFullInfoDto>> CreateUserAsync(CreateUserDto createUserDto, string createdBy)
        {
            var existingUser = await _userRepository.GetByLoginAsync(createUserDto.Login);
            if (existingUser != null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Пользователь с таким логином уже существует");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = createUserDto.Login,
                Password = createUserDto.Password,
                Name = createUserDto.Name,
                Gender = createUserDto.Gender,
                Birthday = createUserDto.Birthday,
                Admin = createUserDto.Admin,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            var createdUser = await _userRepository.CreateAsync(user);
            var userDto = MapToUserFullInfoDto(createdUser);

            return ServiceResult<UserFullInfoDto>.Success(userDto);
        }

        public async Task<ServiceResult<UserFullInfoDto>> UpdateUserInfoAsync(UpdateUserInfoDto updateInfoDto, string modifiedBy)
        {
            var user = await _userRepository.GetByLoginAsync(updateInfoDto.Login);

            if (user == null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Пользователь не найден");
            }

            if (user.RevokedOn != null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Невозможно обновить информацию удаленного пользователя");
            }

            user.Name = updateInfoDto.Name;
            user.Gender = updateInfoDto.Gender;
            user.Birthday = updateInfoDto.Birthday;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            var updatedUser = await _userRepository.UpdateAsync(user);
            if (updatedUser == null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Не удалось обновить информацию пользователя");
            }

            return ServiceResult<UserFullInfoDto>.Success(MapToUserFullInfoDto(updatedUser));
        }

        public async Task<ServiceResult<UserFullInfoDto>> UpdatePasswordAsync(
            string login,
            string oldPassword,
            string newPassword,
            string modifiedBy,
            bool isAdmin = false)
        {
            User? user;

            if (isAdmin)
            {
                user = await _userRepository.GetByLoginAsync(login);
                if (user == null)
                {
                    return ServiceResult<UserFullInfoDto>.Failure("Пользователь не найден");
                }
            }
            else
            {
                user = await _userRepository.GetUserByLoginAndPasswordAsync(login, oldPassword);
                if (user == null)
                {
                    return ServiceResult<UserFullInfoDto>.Failure("Неверный логин или пароль");
                }
            }

            if (user.RevokedOn != null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Невозможно изменить пароль удаленного пользователя");
            }

            user.Password = newPassword;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            var updatedUser = await _userRepository.UpdateAsync(user);
            if (updatedUser == null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Не удалось обновить пароль");
            }

            return ServiceResult<UserFullInfoDto>.Success(MapToUserFullInfoDto(updatedUser));
        }

        public async Task<ServiceResult<UserFullInfoDto>> UpdateLoginAsync(
            string oldLogin,
            string newLogin,
            string password,
            string modifiedBy,
            bool isAdmin = false)
        {
            User? user;

            if (isAdmin)
            {
                user = await _userRepository.GetByLoginAsync(oldLogin);
                if (user == null)
                {
                    return ServiceResult<UserFullInfoDto>.Failure("Пользователь не найден");
                }
            }
            else
            {
                user = await _userRepository.GetUserByLoginAndPasswordAsync(oldLogin, password);
                if (user == null)
                {
                    return ServiceResult<UserFullInfoDto>.Failure("Неверный логин или пароль");
                }
            }

            if (user.RevokedOn != null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Невозможно изменить логин удаленного пользователя");
            }

            var existingUserWithNewLogin = await _userRepository.GetByLoginAsync(newLogin);
            if (existingUserWithNewLogin != null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Пользователь с таким логином уже существует");
            }

            user.Login = newLogin;
            user.ModifiedOn = DateTime.UtcNow;
            user.ModifiedBy = modifiedBy;

            var updatedUser = await _userRepository.UpdateAsync(user);
            if (updatedUser == null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Не удалось обновить логин");
            }

            return ServiceResult<UserFullInfoDto>.Success(MapToUserFullInfoDto(updatedUser));
        }

        public async Task<ServiceResult<List<UserDto>>> GetAllActiveUsersAsync()
        {
            var users = await _userRepository.GetAllActiveUsersAsync();
            var userDtos = users.Select(MapToUserDto).ToList();

            return ServiceResult<List<UserDto>>.Success(userDtos);
        }

        public async Task<ServiceResult<UserFullInfoDto>> GetUserByLoginAsync(string login)
        {
            var user = await _userRepository.GetByLoginAsync(login);

            if (user == null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Пользователь не найден");
            }

            return ServiceResult<UserFullInfoDto>.Success(MapToUserFullInfoDto(user));
        }

        public async Task<ServiceResult<UserFullInfoDto>> GetUserByLoginAndPasswordAsync(string login, string password)
        {
            var user = await _userRepository.GetUserByLoginAndPasswordAsync(login, password);

            if (user == null)
            {
                return ServiceResult<UserFullInfoDto>.Failure("Неверный логин или пароль");
            }

            return ServiceResult<UserFullInfoDto>.Success(MapToUserFullInfoDto(user));
        }

        public async Task<ServiceResult<List<UserDto>>> GetUsersOlderThanAsync(int age)
        {
            var users = await _userRepository.GetUsersOlderThanAsync(age);
            var userDtos = users.Select(MapToUserDto).ToList();

            return ServiceResult<List<UserDto>>.Success(userDtos);
        }

        public async Task<ServiceResult<bool>> DeleteUserAsync(string login, string deletedBy, bool hardDelete = false)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                return ServiceResult<bool>.Failure("Пользователь не найден");
            }

            if (user.RevokedOn != null && !hardDelete)
            {
                return ServiceResult<bool>.Failure("Пользователь уже удален");
            }

            var result = await _userRepository.DeleteAsync(login, deletedBy, hardDelete);
            if (!result)
            {
                return ServiceResult<bool>.Failure("Не удалось удалить пользователя");
            }

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> RestoreUserAsync(string login)
        {
            var user = await _userRepository.GetByLoginAsync(login);
            if (user == null)
            {
                return ServiceResult<bool>.Failure("Пользователь не найден");
            }

            if (user.RevokedOn == null)
            {
                return ServiceResult<bool>.Failure("Пользователь не был удален");
            }

            var result = await _userRepository.RestoreUserAsync(login);
            if (!result)
            {
                return ServiceResult<bool>.Failure("Не удалось восстановить пользователя");
            }

            return ServiceResult<bool>.Success(true);
        }

        private UserDto MapToUserDto(User user)
        {
            string genderString = user.Gender switch
            {
                0 => "Женщина",
                1 => "Мужчина",
                _ => "Неизвестно"
            };

            return new UserDto
            {
                Login = user.Login,
                Name = user.Name,
                Gender = genderString,
                Birthday = user.Birthday,
                IsActive = user.RevokedOn == null,
                IsAdmin = user.Admin
            };
        }

        private UserFullInfoDto MapToUserFullInfoDto(User user)
        {
            string genderString = user.Gender switch
            {
                0 => "Женщина",
                1 => "Мужчина",
                _ => "Неизвестно"
            };

            return new UserFullInfoDto
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Name,
                Gender = genderString,
                Birthday = user.Birthday,
                IsAdmin = user.Admin,
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy,
                RevokedOn = user.RevokedOn,
                RevokedBy = user.RevokedBy,
                IsActive = user.RevokedOn == null
            };
        }
    }
}