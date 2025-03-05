﻿using Domain.Admins;
using UseCases.Admins.Commands;
using UseCases.Response;

namespace UseCases.Admins
{
    public interface IAdminManager
    {
        Admin Create(CreateAdminCommand command);
        Admin Authentication(AuthenticationCommand command);
        void SetupPassword(SetupPasswordCommand command);
        void Delete(DeleteAdminCommand command);
        ICollection<AdminResponse> GetAdmins(long adminId, int since, int count);
        ICollection<UserResponse> GetUsers(int since, int count);
        void CreateCodeForRecoveryPassword(string adminEmail);
        void ChangePassword(ChangePasswordCommand command);
    }
}
