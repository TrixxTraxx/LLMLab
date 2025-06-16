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

namespace LLMLab.Server.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class SharedChatController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SharedChatController(ApplicationDbContext db)
        {
            _db = db;
        }

        // POST api/SharedChat/{threadId}
        [HttpPost("{threadId}")]
        [Authorize]
        public async Task<IActionResult> CreateSnapshot(int threadId)
        {
            var thread = await _db.MessageThreads.FirstOrDefaultAsync(t => t.Id == threadId);
            if (thread == null)
                return NotFound();

            var messages = await _db.Messages.Where(m => m.ThreadId == threadId).OrderBy(m => m.CreatedAt).ToListAsync();
            var messageDtos = messages.Select(m => new MessageDto
            {
                Id = m.Id,
                AttachmentIds = m.AttachmentIds,
                Text = m.Text,
                ModelResponse = m.ModelResponse,
                PreviousMessageId = m.PreviousMessageId,
                Complete = m.Complete,
                ModelId = m.ModelId,
                ThreadId = m.ThreadId,
                CreatedAt = m.CreatedAt,
                ThinkingResponse = m.ThinkingResponse,
                ReasoningEffortLevel = m.ReasoningEffortLevel,
                Error = m.Error,
                ErrorMessage = m.ErrorMessage
            }).ToList();

            var snapshot = new SharedChatSnapshotDto
            {
                Thread = new SharedThreadDto { Id = thread.Id, Title = thread.Title },
                Messages = messageDtos
            };
            var serialized = JsonSerializer.Serialize(snapshot);

            var shared = new SharedChat
            {
                Uuid = Guid.NewGuid(),
                UserId = User.Identity?.Name,
                SerializedData = serialized
            };
            _db.SharedChats.Add(shared);
            await _db.SaveChangesAsync();
            var url = $"/share/{shared.Uuid}";
            return Ok(new { url });
        }

        // GET api/SharedChat/{uuid}
        [HttpGet("{uuid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSnapshot(Guid uuid)
        {
            var shared = await _db.SharedChats.FirstOrDefaultAsync(s => s.Uuid == uuid);
            if (shared == null)
                return NotFound();
            return Content(shared.SerializedData, "application/json");
        }
    }
} 