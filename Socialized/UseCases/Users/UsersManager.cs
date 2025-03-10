using Core;
using Serilog;
using System.Web;
using Domain.Users;
using UseCases.Users.Commands;
using UseCases.Exceptions;

namespace UseCases.Users
{
    public class UsersManager : BaseManager, IUsersManager
    {
        private IUserRepository UserRepository;
        
        public ProfileCondition ProfileCondition = new ProfileCondition();
        private IEmailMessanger EmailMessanger;

        public UsersManager(ILogger logger,
            IUserRepository userRepository,
            IEmailMessanger emailMessager) : base(logger) 
        {
            UserRepository = userRepository;
            EmailMessanger = emailMessager;
        }
        public void Create(CreateUserCommand command)
        {
            Logger.Information("������� ��������� ������ �����������.");
            var user = UserRepository.GetByEmail(command.Email);
            if (user != null && user.IsDeleted)
            {
                user.IsDeleted = false;
                user.TokenForUse = Guid.NewGuid().ToString();
                UserRepository.Update(user);
                Logger.Information($"��� ����������� ��������� �������, id={user.Id}.");
                return;   
            }
            if (user != null && !user.IsDeleted)
            {
                throw new NotFoundException("���������� � ����� email-������� ��� ����.");
            }
            user = new User
            {
                Email = command.Email,
                FirstName = HttpUtility.UrlDecode(command.FirstName),
                LastName = HttpUtility.UrlDecode(command.LastName),
                Password = ProfileCondition.HashPassword(command.Password),
                HashForActivate = ProfileCondition.CreateHash(100),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                TokenForUse = ProfileCondition.CreateHash(40),
                RecoveryToken = ""
            };            
            UserRepository.Create(user);
            EmailMessanger.SendConfirmEmail(user.Email, command.Culture, user.HashForActivate);
            Logger.Information($"����� ���������� ��� ���������, id={user.Id}.");
        }
        public void RegistrationEmail(string userEmail, string culture)
        {
            Logger.Information($"������� ����������� ����� �� ������������ ��������� �����������, email={userEmail}.");
            var user = UserRepository.GetByEmail(userEmail);
            if (user == null)
            {
                throw new NotFoundException("������ �� �������� ����������� �� email ��� ��������� ��������.");
            }
            EmailMessanger.SendConfirmEmail(user.Email, culture, user.HashForActivate);
            Logger.Information($"³�������� ���� �� ������������ ��������� �����������, id={user.Id}.");                
        }
        public void Activate(string hash)
        {
            Logger.Information("������� ��������� �������� ����������� �� ��������� ����.");
            var user = UserRepository.GetByHash(hash, false, false);
            if (user == null)
            {
                throw new NotFoundException("������ �� �������� ����������� �� ���� ��� ��������� ��������.");
            }
            user.Activate = true;
            UserRepository.Update(user);
            Logger.Information($"���������� ��� ����������� ������� ���� � �����, id={user.Id}.");
        }
        public void Delete(string userToken)
        {
            Logger.Information("������� ��������� ����������� �� ���� ������.");
            var user = UserRepository.GetByUserTokenNotDeleted(userToken);
            if (user == null)
            {
                throw new NotFoundException("������ �� �������� ����������� �� ���� ������ ��� ��������� ��������.");                
            }
            user.IsDeleted = true;
            user.TokenForUse = "";
            UserRepository.Update(user);
            Logger.Information($"���������� ��� ���������, id={user.Id}.");
        }
    }
}