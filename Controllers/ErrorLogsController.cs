using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Blogic.Service;
using WebAppPedalaCom.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorLogsController : ControllerBase
    {
        private readonly CredentialWorks2024Context _context;
        private readonly ErrorLogService _errorLogService;

        public ErrorLogsController(CredentialWorks2024Context context)
        {
            _context = context;
            CredentialWorks2024Context CWcontext = new();
            this._errorLogService = new(CWcontext);
        }

        // GET: api/ErrorLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetErrorLogs(int pageNumber = 1)
        {
            int pageSize = 10;
            object? paginationInfo = null;
            List<ErrorLog> errors = new List<ErrorLog>();

            try
            {
                var errorLogsQuery = _context.ErrorLogs
                .Where(errorLog => errorLog != null);

                // Assicurati che il metodo ToListAsync() sia asincrono
                var paginatedErrorLogs = await errorLogsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

                // Converti i risultati paginati in una lista
                errors.AddRange(paginatedErrorLogs);

                if (errors.Count == 0)
                {
                    return Ok();
                }

                // Ottieni il numero totale di elementi nella query
                int totalItems = errorLogsQuery.Count();

                // Calcola il numero totale di pagine
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Aggiungi i risultati paginati e le informazioni sulla paginazione alla risposta
                paginationInfo = new
                {
                    pageNumber = pageNumber,
                    TotalPages = totalPages
                };

            }
            catch (ArgumentNullException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
            }

            return Ok(new { ErrorLog = errors, PaginationInfo = paginationInfo });
        }


        // GET: api/ErrorLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ErrorLog>> GetErrorLog(int id)
        {
            if (_context.ErrorLogs == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.ErrorLogs is Null");
            }

            var errorLog = await _context.ErrorLogs.FindAsync(id);

            if (errorLog == null)
                return NotFound();

            return Ok(errorLog);
        }
    }
}
