﻿using Serilog;
using System.Web;
using Domain.Admins;
using UseCases.Exceptions;
using MediatR;
using AutoMapper;
using UseCases.Admins.Models;
using UseCases.Admins.Emails;
using Core.Providers.Hmac;
using Core.Providers.Rand;
using Infrastructure.Repositories;

namespace UseCases.Admins.Commands.CreateAdmin;

public class CreateAdminCommandHandler(
    IRepository<Admin> adminRepository,
    IEncryptionProvider encryptionProvider,
    IAdminEmailManager adminEmailManager,
    ILogger logger,
    IRandomizer randomizer,
    IMapper mapper
    ) : IRequestHandler<CreateAdminCommand, AdminResponse>
{
    public async Task<AdminResponse> Handle(CreateAdminCommand command, CancellationToken cancellationToken = default)
    {
        if (await adminRepository.AnyAsync(a => a.Email == command.Email && !a.IsDeleted))
        {
            throw new NotFoundException($"Admin with email={command.Email} is already exist.");
        }
        var newHashedPassword = encryptionProvider.HashPassword(command.Password);
        var admin = new Admin
        {
            Email = command.Email,
            FirstName = HttpUtility.UrlDecode(command.FirstName),
            LastName = HttpUtility.UrlDecode(command.LastName),
            HashedPassword = newHashedPassword.Hash,
            HashedSalt = newHashedPassword.Salt,
            Role = "default",
            TokenForStart = randomizer.CreateHash(10),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
        await adminRepository.AddAsync(admin, cancellationToken);
        adminEmailManager.SetupPassword(admin.TokenForStart, admin.Email);
        logger.Information($"Був створений новий адмін, id={admin.Id}.");
        return mapper.Map<AdminResponse>(admin);
    }
}