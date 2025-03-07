﻿using Domain.Admins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UseCases.Appeals.Replies;
using UseCases.Appeals.Replies.Commands;
using WebAPI.Responses;

namespace WebAPI.Controllers.Appeals
{
    public class AppealMessageReplyController : ControllerResponseBase
    {
        private IAppealMessageReplyManager AppealMessageReplyManager;

        public AppealMessageReplyController(IAppealMessageReplyManager appealMessageReplyManager)
        {
            AppealMessageReplyManager = appealMessageReplyManager;
        }
        [HttpPost]
        [Authorize]
        public ActionResult<DataResponse> Create(CreateAppealMessageReplyCommand command)
        {
            var result = AppealMessageReplyManager.Create(command);

            return Ok();
        }
        [HttpPut]
        [Authorize]
        public ActionResult<SuccessResponse> Update(UpdateAppealMessageReplyCommand command)
        {
            AppealMessageReplyManager.Update(command);

            return Ok();
        }
        [HttpDelete]
        [Authorize]
        public ActionResult<SuccessResponse> Delete(DeleteAppealMessageReplyCommand command)
        {
            AppealMessageReplyManager.Delete(command);

            return Ok();
        }
    }
}
