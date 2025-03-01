using Core;
using Serilog;
using System.Web;
using UseCases.Exceptions;
using UseCases.Admins.Commands;
using Domain.Users;
using Domain.Admins;

namespace UseCases.Admins
{
    public class AdminManager : BaseManager, IAdminManager
    {
        private IAdminRepository AdminRepository;
        private IAdminEmailManager AdminEmailManager;
        private ProfileCondition ProfileCondition = new ProfileCondition();
        
        public AdminManager(ILogger logger, 
            IAdminRepository adminRepository,
            IAdminEmailManager adminEmailManager) : base(logger)
        {
            AdminRepository = adminRepository;
            AdminEmailManager = adminEmailManager;
        }
        public Admin Create(CreateAdminCommand command)
        {
            if (AdminRepository.GetByEmail(command.Email) != null)
            {
                throw new NotFoundException($"Admin with email={command.Email} is already exist.");
            }
            var admin = new Admin
            {
                Email = command.Email,
                FirstName = HttpUtility.UrlDecode(command.FirstName),
                LastName = HttpUtility.UrlDecode(command.LastName),
                Password = ProfileCondition.HashPassword(command.Password),
                Role = "default",
                TokenForStart = ProfileCondition.CreateHash(10),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            AdminRepository.Create(admin);
            AdminEmailManager.SetupPassword(admin.TokenForStart, admin.Email);
            Logger.Information($"��� ��������� ����� ����, id={admin.Id}.");
            return admin;
        }
        public void SetupPassword(SetupPasswordCommand command)
        {
            var admin = AdminRepository.GetByPasswordToken(command.Token);
            if (admin == null)
            {
                throw new NotFoundException("�� ���� �������� ����� �� ������ ��� ���� ������.");
            }   
            admin.Password = ProfileCondition.HashPassword(command.Password);
            admin.TokenForStart = "";
            AdminRepository.Update(admin);
            Logger.Information($"��� ���������� ������ ��� ����� id={admin.Id}.");
        }
        public Admin Authentication(AuthenticationCommand command)
        {
            var admin = AdminRepository.GetByEmail(command.Email);
            if (admin == null)
            {
                throw new NotFoundException("�� ���� �������� ����� �� email-�����.");
            }
            if (!ProfileCondition.VerifyHashedPassword(admin.Password, command.Password))
            {
                throw new ValidationException("������� ������.");
            }
            Logger.Information($"��� ���������������� ���� id={admin.Id}.");
            return admin;
        }
        public void Delete(DeleteAdminCommand command)
        {
            var admin = AdminRepository.GetByAdminId(command.AdminId);
            if (admin == null)
            {
                throw new NotFoundException("�� ���� �������� ����� �� id.");
            }
            admin.IsDeleted = true;
            admin.DeletedAt = DateTime.UtcNow;
            AdminRepository.Update(admin);
            Logger.Information($"���� ��� ���������, id={admin.Id}.");
        }
        public ICollection<Admin> GetAdmins(long adminId, int since, int count)
        {
            Logger.Information($"�������� ������ �����, �={since} ��={count} ������ id={adminId}.");
            return AdminRepository.GetActiveAdmins(adminId, since, count);
        }
        public ICollection<User> GetUsers(int since, int count)
        {
            Logger.Information($"�������� ������ ������������, �={since} ��={count}.");
            return AdminRepository.GetUsers(since, count);
        }
        public void CreateCodeForRecoveryPassword(string adminEmail)
        {
            var admin = AdminRepository.GetByEmail(adminEmail);
            if (admin == null)
            {
                throw new NotFoundException("�� ���� �������� ����� �� email-�����.");
            }
            admin.RecoveryCode = ProfileCondition.CreateCode(6);
            AdminRepository.Update(admin);
            AdminEmailManager.RecoveryPassword(admin.RecoveryCode.Value, admin.Email);
            Logger.Information($"��� ��������� ����� ��� ���������� ������ �����, id={admin.Id}.");   
        }
        public void ChangePassword(ChangePasswordCommand command) 
        {
            var admin = AdminRepository.GetByRecoveryCode(command.RecoveryCode);
            if (admin == null)
            {
                throw new ArgumentNullException("������ �� �������� ����� �� ����. ������������ ���.");
            }    
            admin.Password = ProfileCondition.HashPassword(command.Password);
            admin.RecoveryCode = null;
            AdminRepository.Update(admin);
            Logger.Information($"��� ������� ������ � �����, id={admin.Id}.");
        }
    }
}