﻿using MediatR;
using Serilog;
using Domain.Users;
using UseCases.Exceptions;
using Infrastructure.Repositories;

namespace UseCases.Users.Commands.Delete;

public class DeleteCommandHandler (
    ILogger logger,
    IRepository<User> userRepository) : IRequestHandler<DeleteCommand, DeleteResponse>
{
    public async Task<DeleteResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
    {
        logger.Information("Початок видалення користувача по його токену.");
        var user = await userRepository.FirstOrDefaultAsync(u=> u.TokenForUse == request.UserToken && !u.IsDeleted);
        if (user == null)
        {
            throw new NotFoundException("Сервер не визначив користувача по його токену для видалення аккаунту.");
        }
        user.IsDeleted = true;
        user.TokenForUse = "";
        userRepository.Update(user);
        logger.Information($"Користувач був видалений, id={user.Id}.");
        return new DeleteResponse(true, $"Користувач був видалений, id={user.Id}.");
    }
}