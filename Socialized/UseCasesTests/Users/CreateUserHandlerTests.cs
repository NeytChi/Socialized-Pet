﻿using Core;
using Domain.Users;
using NSubstitute;
using Serilog;
using UseCases.Exceptions;
using UseCases.Users.Emails;
using UseCases.Users.Commands.CreateUser;
using NSubstitute.ReturnsExtensions;

namespace UseCasesTests.Users;

public class CreateUserHandlerTests
{
    private ILogger logger = Substitute.For<ILogger>();
    private IUserRepository userRepository = Substitute.For<IUserRepository>();
    private ProfileCondition profileCondition = Substitute.For<ProfileCondition>();
    private IEmailMessanger emailMessanger = Substitute.For<IEmailMessanger>();

    [Fact]
    public async Task Create_WhenUserIsAlreadyExistAndNotDeleted_ThrowNotFoundException()
    {
        var command = new CreateUserCommand
        {
            Email = "test@test.com",
            FirstName = "Rick",
            LastName = "Dolton",
            Password = "Pass1234!",
            CountryName = "USA",
            TimeZone = 6,
            Culture = "en_EN"
        };
        var user = new User { Email = command.Email, IsDeleted = false };
        userRepository.GetByEmail(command.Email).Returns(user);
        var handler = new CreateUserCommandHandler(userRepository, emailMessanger, profileCondition, logger);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
    [Fact]
    public async Task Create_WhenUserWasDeleted_RestoreUserAndReturn()
    {
        var command = new CreateUserCommand
        {
            Email = "test@test.com",
            FirstName = "Rick",
            LastName = "Dolton",
            Password = "Pass1234!",
            CountryName = "USA",
            TimeZone = 6,
            Culture = "en_EN"
        };
        var user = new User { Email = command.Email, IsDeleted = true };
        userRepository.GetByEmail(command.Email).Returns(user);
        var handler = new CreateUserCommandHandler(userRepository, emailMessanger, profileCondition, logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
    }
    [Fact]
    public async Task Create_WhenJustNewUser_CreateNewUserAndReturn()
    {
        var command = new CreateUserCommand
        {
            Email = "test@test.com",
            FirstName = "Rick",
            LastName = "Dolton",
            Password = "Pass1234!",
            CountryName = "USA",
            TimeZone = 6,
            Culture = "en_EN"
        };
        userRepository.GetByEmail(command.Email).ReturnsNull();
        var handler = new CreateUserCommandHandler(userRepository, emailMessanger, profileCondition, logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Success);
    }
}
