﻿using Microsoft.AspNetCore.Mvc;
using UseCases.Users;
using UseCases.Users.Commands;
using WebAPI.Responses;

namespace WebAPI.Controllers
{
    public class UsersController : ControllerResponseBase
    {
        private IUsersManager UserManager;
        private IUserLoginManager UserLoginManager;
        private IUserPasswordRecoveryManager UserPasswordRecoveryManager;

        public UsersController(IUsersManager usersManager, 
            IUserLoginManager userLoginManager,
            IUserPasswordRecoveryManager userPasswordRecoveryManager)
        {
            UserManager = usersManager;
            UserLoginManager = userLoginManager;
            UserPasswordRecoveryManager = userPasswordRecoveryManager;
        }
        [HttpPost]
        [ActionName("Registration")]
        public ActionResult<dynamic> Registration(CreateUserCommand command)
        {
            UserManager.Create(command);

            return Ok();
        }
        [HttpPost]
        [ActionName("RegistrationEmail")]
        public ActionResult<dynamic> RegistrationEmail([FromQuery] string email)
        {
            var culture = GetCulture();

            UserManager.RegistrationEmail(email, culture);

            return Ok();
        }
        [HttpPost]
        [ActionName("Login")]
        public ActionResult<dynamic> Login(LoginUserCommand command)
        {
            var result = UserLoginManager.Login(command);

            return Ok(result);
        }
        [HttpPost]
        [ActionName("LogOut")]
        public ActionResult<dynamic> LogOut([FromBody] string userToken)
        {
            UserLoginManager.LogOut(userToken);

            return Ok();
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword([FromQuery] string email)
        {
            var culture = GetCulture();

            UserPasswordRecoveryManager.RecoveryPassword(email, culture);

            return Ok();
        }
        [HttpPost]
        [ActionName("CheckRecoveryCode")]
        public ActionResult<dynamic> CheckRecoveryCode(CheckRecoveryCodeCommand command)
        {
            string recoveryToken = UserPasswordRecoveryManager.CheckRecoveryCode(command);

            return Ok(new { recovery_token = recoveryToken });
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        public ActionResult<dynamic> ChangePassword(ChangeUserPasswordCommand command)
        {
            UserPasswordRecoveryManager.ChangePassword(command);

            return Ok();
        }
        [HttpPost]
        [ActionName("ChangeOldPassword")]
        public ActionResult<dynamic> ChangeOldPassword(ChangeOldPasswordCommand command)
        {
            UserPasswordRecoveryManager.ChangeOldPassword(command);

            return Ok();
        }
        [HttpGet]
        [ActionName("Activate")]
        public ActionResult<dynamic> Activate([FromQuery] string hash)
        {
            UserManager.Activate(hash);

            return Ok();
        }
        [HttpDelete]
        public ActionResult<dynamic> Delete([FromBody] string userToken)
        {
            UserManager.Delete(userToken);

            return Ok();
        }
    }
}