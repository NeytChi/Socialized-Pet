using Serilog;
using Domain.Packages;
using UseCases.Packages.Command;
using Domain.Users;
using UseCases.Exceptions;

namespace UseCases.Packages
{
    public class PackageManager : BaseManager, IPackageManager
    {
        private IServiceAccessRepository ServiceAccessRepository;
        private IPackageAccessRepository PackageAccessRepository;
        private IDiscountRepository DiscountRepository;
        private IForServerAccessCountingRepository CounterRepository;
        private IUserRepository UserRepository;

        public PackageManager(ILogger logger,
            IServiceAccessRepository serviceAccessRepository,
            IPackageAccessRepository packageAccessRepository,
            IDiscountRepository discountRepository,
            IForServerAccessCountingRepository autoPostCounterRepository,
            IUserRepository userRepository) : base(logger)
        {
            ServiceAccessRepository = serviceAccessRepository;
            PackageAccessRepository = packageAccessRepository;
            DiscountRepository = discountRepository;
            CounterRepository = autoPostCounterRepository;
            UserRepository = userRepository;
        }
        public ServiceAccess CreateDefaultServiceAccess(long userId)
        {
            var user = UserRepository.GetBy(userId);

            return user == null 
                ? throw new NotFoundException($"������ �� �������� ����������� �� id={userId}.") 
                : CreateDefaultServiceAccess(user);
        }
        public ServiceAccess CreateDefaultServiceAccess(User user)
        {
            var package = PackageAccessRepository.GetFirst();
            var access = new ServiceAccess()
            {
                UserId = user.Id,
                Available = true,
                Type = package.Id,
                Paid = false,
                User = user
            };
            ServiceAccessRepository.Create(access);
            Logger.Information($"��� ��������� ������������ ������ �� ������ ��� �����������, id={access.Id}.");
            return access;
        }
        public void SetPackage(long userId, long packageId, int monthCount)
        {
            var access = ServiceAccessRepository.GetBy(userId);
            if (access == null)
            {
                access = CreateDefaultServiceAccess(userId);
            }
            var package = PackageAccessRepository.GetBy(packageId);
            access.Available = true;
            access.Type = package.Id;
            access.Paid = true;
            access.PaidAt = DateTime.UtcNow;
            access.DisableAt = DateTime.UtcNow.AddMonths(monthCount);
            ServiceAccessRepository.Update(access);
            Logger.Information($"�������� ������ ���� ��������, id={access.Id}.");
        }
        public bool IsServicePackagePersonalize(long userId)
        {
            var access = GetWorkingServiceAccess(userId);

            if (access == null)
            {
                return false;
            }
            var package = PackageAccessRepository.GetBy(access.Type);

            if (package.IGAccounts == -1
                && package.Stories == -1
                && package.Posts == -1)
            {
                return true;
            }
            var accounts = CounterRepository.GetAccounts(userId);
            var posts = CounterRepository.Get(userId, true);
            var storyPosts = CounterRepository.Get(userId, false);

            if (accounts.Count > package.IGAccounts)
            {
                Logger.Warning("Instagram �������� ����� �� �������� �� ������.");
                return false;
            }
            if (posts.Count > package.Posts)
            {
                Logger.Warning("ʳ������ ����� ��������� ������� ��������� �� ��������� ������.");
                return false;
            }
            if (storyPosts.Count > package.Stories)
            {
                Logger.Warning("ʳ������ ����c ��������� ������� ��������� �� ��������� ������.");
                return false;
            }
            return true;
        }
        public ServiceAccess? GetWorkingServiceAccess(long userId)
        {
            var access = ServiceAccessRepository.GetByUser(userId);
            if (access == null)
            {
                return null;
            }
            if (access.Type != 1 && access.DisableAt < DateTime.UtcNow)
            {
                SetPackage(userId, 1, -1);
                Logger.Information($"�������� ����� ������� ��� ����� ����������, id={access.Id}.");
            }
            return access;
        }
        public decimal CalcPackagePrice(PackageAccess package, int monthCount)
        {
            decimal discountPrice = 0;
            var discount = DiscountRepository.GetBy(monthCount);

            if (discount != null)
            {
                discountPrice = (decimal)(package.Price * discount.Month / 100 * discount.Percent);
            }
            Logger.Information($"���������� ���� �� �������� �����, id={package.Id}.");
            return (decimal)package.Price * monthCount - discountPrice;
        }
        public void PayForPackage(PayForPackageCommand command)
        {
            /*
            var user = GetNonDeletedUser(command.UserToken);
            if (user != null)
            {

            }
            var package = access.GetPackageById(command.PackageId);

            if (package == null)
            { 
                message = "Server can't define package by package id.";
            }

            var price = access.CalcPackagePrice(package, command.MonthCount);

            if (!access.PayForPackage(price, command.NonceToken, deviceData))
            {

            }

            access.SetPackage(user.userId, package.package_id, command.MonthCount);
            */
        }

        public ICollection<PackageAccess> GetPackageAccess()
        {
            return PackageAccessRepository.GetAll();
        }

        public ICollection<DiscountPackage> GetDiscountPackageAccess()
        {
            return DiscountRepository.GetAll();
        }

        public string GetClientTokenForPay(GetClientTokenForPayCommand command)
        {
            return string.Empty;
        }
    }
}








