using CleanArchitecture.Core.DTOs.Chat;
using CleanArchitecture.Core.Features.ClubChannels.Commands.CreateClubChannel;
using CleanArchitecture.Core.Features.ClubChannels.Commands.DeleteClubChannel;
using CleanArchitecture.Core.Features.ClubChannels.Commands.SendChannelMessage;
using CleanArchitecture.Core.Features.ClubChannels.Commands.UpdateClubChannel;
using CleanArchitecture.Core.Features.ClubChannels.Queries.GetChannelMessages;
using CleanArchitecture.Core.Features.ClubChannels.Queries.GetClubChannels;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/Clubs/{clubId}/channels")]
    public class ClubChannelsController : BaseApiController
    {
        private readonly IAuthenticatedUserService _authenticatedUser;

        public ClubChannelsController(IAuthenticatedUserService authenticatedUser)
        {
            _authenticatedUser = authenticatedUser;
        }

        /// <summary>
        /// Get all channels for a club
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetClubChannels(int clubId)
        {
            return Ok(await Mediator.Send(new GetClubChannelsQuery
            {
                ClubId = clubId,
                CurrentUserId = _authenticatedUser.UserId
            }));
        }

        /// <summary>
        /// Get message history for a channel
        /// </summary>
        [HttpGet("{channelId}/messages")]
        public async Task<IActionResult> GetChannelMessages(int clubId, int channelId)
        {
            return Ok(await Mediator.Send(new GetChannelMessagesQuery
            {
                ClubId = clubId,
                ChannelId = channelId,
                CurrentUserId = _authenticatedUser.UserId
            }));
        }

        /// <summary>
        /// Send a message to a channel
        /// </summary>
        [HttpPost("{channelId}/messages")]
        public async Task<IActionResult> SendChannelMessage(int clubId, int channelId, [FromBody] SendChannelMessageRequest request)
        {
            return Ok(await Mediator.Send(new SendChannelMessageCommand
            {
                ClubId = clubId,
                ChannelId = channelId,
                SenderId = _authenticatedUser.UserId,
                Content = request.Content
            }));
        }

        /// <summary>
        /// Create a new channel (Leader only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateChannel(int clubId, [FromBody] CreateClubChannelRequest request)
        {
            return Ok(await Mediator.Send(new CreateClubChannelCommand
            {
                ClubId = clubId,
                CurrentUserId = _authenticatedUser.UserId,
                Name = request.Name,
                Description = request.Description,
                WriteRoleIds = request.WriteRoleIds
            }));
        }

        /// <summary>
        /// Update a channel's name, description and write roles (Leader only)
        /// </summary>
        [HttpPut("{channelId}")]
        public async Task<IActionResult> UpdateChannel(int clubId, int channelId, [FromBody] UpdateClubChannelRequest request)
        {
            return Ok(await Mediator.Send(new UpdateClubChannelCommand
            {
                ClubId             = clubId,
                ChannelId          = channelId,
                CurrentUserId      = _authenticatedUser.UserId,
                Name               = request.Name,
                Description        = request.Description,
                WriteRoleIds       = request.WriteRoleIds       ?? new System.Collections.Generic.List<int>(),
                VisibilityRoleIds  = request.VisibilityRoleIds  ?? new System.Collections.Generic.List<int>()
            }));
        }

        /// <summary>
        /// Delete a channel (Leader only — default channel cannot be deleted)
        /// </summary>
        [HttpDelete("{channelId}")]
        public async Task<IActionResult> DeleteChannel(int clubId, int channelId)
        {
            return Ok(await Mediator.Send(new DeleteClubChannelCommand
            {
                ClubId        = clubId,
                ChannelId     = channelId,
                CurrentUserId = _authenticatedUser.UserId
            }));
        }
    }
}
