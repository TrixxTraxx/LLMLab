using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LLMLab.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LLMLab.Dtos.Messages;
using LLMLab.Dtos.Threads;
using LLMLab.Server.Mappers;
using System.Security.Claims;
using LLMLab.Server.Service;

namespace LLMLab.Server.Controller;


[ApiController]
[Route("api/[controller]")]
public class SharedChatController(ChatSharingService service) : ControllerBase
{

    // POST api/SharedChat/{threadId}
    [HttpPost("{threadId}")]
    [Authorize]
    public async Task<IActionResult> CreateSnapshot(int threadId)
    {
        var url = await service.CreateSnapshot(threadId);
        return Ok(url);
    }

    // GET api/SharedChat/{uuid}
    [HttpGet("{uuid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSnapshot(Guid uuid)
    {
        var sharedChatData = await service.GetSharedChatData(uuid);
        return Ok(sharedChatData);
    }
}