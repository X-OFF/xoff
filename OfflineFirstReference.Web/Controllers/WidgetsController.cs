using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfflineFirstReference.Web.Models;

namespace OfflineFirstReference.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Widgets")]
    public class WidgetsController : Controller
    {
        private readonly OfflineFirstReferenceWebContext _context;

        public WidgetsController(OfflineFirstReferenceWebContext context)
        {
            _context = context;
        }

        // GET: api/Widgets
        [HttpGet]
        public IEnumerable<Widget> GetWidgets()
        {
            return _context.Widgets.ToList();
        }

        // GET: api/Widgets/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWidget([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var widget = await _context.Widgets.SingleOrDefaultAsync(m => m.Id == id);

            if (widget == null)
            {
                return NotFound();
            }

            return Ok(widget);
        }

        // PUT: api/Widgets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWidget([FromRoute] Guid id, [FromBody] Widget widget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != widget.Id)
            {
                return BadRequest();
            }

            _context.Entry(widget).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WidgetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Widgets
        [HttpPost]
        public async Task<IActionResult> PostWidget([FromBody] Widget widget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Widgets.Add(widget);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWidget", new { id = widget.Id }, widget);
        }

        // DELETE: api/Widgets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWidget([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var widget = await _context.Widgets.SingleOrDefaultAsync(m => m.Id == id);
            if (widget == null)
            {
                return NotFound();
            }

            _context.Widgets.Remove(widget);
            await _context.SaveChangesAsync();

            return Ok(widget);
        }

        private bool WidgetExists(Guid id)
        {
            return _context.Widgets.Any(e => e.Id == id);
        }
    }
}