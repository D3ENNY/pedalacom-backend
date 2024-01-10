using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Models;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorLogsController : ControllerBase
    {
        private readonly CredentialWorks2024Context _context;

        public ErrorLogsController(CredentialWorks2024Context context)
        {
            _context = context;
        }

        // GET: api/ErrorLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetErrorLogs()
        {
          if (_context.ErrorLogs == null)
          {
              return NotFound();
          }
            return await _context.ErrorLogs.ToListAsync();
        }

        // GET: api/ErrorLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ErrorLog>> GetErrorLog(int id)
        {
          if (_context.ErrorLogs == null)
          {
              return NotFound();
          }
            var errorLog = await _context.ErrorLogs.FindAsync(id);

            if (errorLog == null)
            {
                return NotFound();
            }

            return errorLog;
        }
    }
}
