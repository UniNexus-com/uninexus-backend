using CleanArchitecture.Core.Features.Chat.Commands.MarkAsRead;
using CleanArchitecture.Core.Features.Chat.Commands.ReactToMessage;
using CleanArchitecture.Core.Features.Chat.Commands.SendMessage;
using CleanArchitecture.Core.Features.Chat.Queries.GetChatHistory;
using CleanArchitecture.Core.Features.Chat.Queries.GetUserChats;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    public class MessagesController : BaseApiController
    {
        private readonly IAuthenticatedUserService _authenticatedUser;

        public MessagesController(IAuthenticatedUserService authenticatedUser)
        {
            _authenticatedUser = authenticatedUser;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            return Ok(await Mediator.Send(new GetUserChatsQuery { UserId = _authenticatedUser.UserId }));
        }

        [HttpGet("{targetUserId}")]
        public async Task<IActionResult> GetChatHistory(string targetUserId)
        {
            return Ok(await Mediator.Send(new GetChatHistoryQuery 
            { 
                CurrentUserId = _authenticatedUser.UserId, 
                TargetUserId = targetUserId 
            }));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command)
        {
            command.SenderId = _authenticatedUser.UserId;
            return Ok(await Mediator.Send(command));
        }

        [HttpPut("{senderId}/read")]
        public async Task<IActionResult> MarkAsRead(string senderId)
        {
            return Ok(await Mediator.Send(new MarkAsReadCommand 
            { 
                CurrentUserId = _authenticatedUser.UserId, 
                SenderId = senderId 
            }));
        }

        [HttpPut("{messageId}/reaction")]
        public async Task<IActionResult> ReactToMessage(int messageId, [FromBody] ReactToMessageCommand command)
        {
            command.MessageId = messageId;
            command.CurrentUserId = _authenticatedUser.UserId;
            return Ok(await Mediator.Send(command));
        }
    }
}
