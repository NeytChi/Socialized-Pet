﻿using Core;
using Serilog;

namespace UseCases.Admins;

public class AdminEmailManager : BaseManager, IAdminEmailManager
{
    private ISmtpSender SmtpSender;
    public AdminEmailManager(ISmtpSender smtpSender, ILogger logger) : base(logger)
    {
        SmtpSender = smtpSender;
    }
    public void SetupPassword(string tokenForStart, string email)
    {
        SmtpSender.SendEmail(email, "Створення паролю", tokenForStart);
        Logger.Information("Був відправлений URL для активації вашого адмін аккаунту.");
    }
    public void RecoveryPassword(int code, string email)
    {
        SmtpSender.SendEmail(email, "Відновлення паролю", $"Code: {code}");
        Logger.Information("Був відправлений 6 знаковий код для відновлення паролю.");
    }
}
