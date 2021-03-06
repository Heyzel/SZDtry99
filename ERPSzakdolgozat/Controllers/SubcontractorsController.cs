﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERPSzakdolgozat.Models;
using ERPSzakdolgozat.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace ERPSzakdolgozat.Controllers
{
    public class SubcontractorsController : MyController
    {
        public SubcontractorsController(ERPDBContext context) : base(context)
		{
            _context = context;
        }

        // GET: Subcontractors
        public async Task<IActionResult> Index(string search, bool active = true)
        {
			IQueryable<Subcontractor> subs = _context.Subcontractors;

			if (!string.IsNullOrEmpty(search))
			{
				subs = subs.Where(s => s.SubcontractorName.ToLower().Contains(search.ToLower()));
			}
			if (active)
			{
				subs = subs.Where(s => s.IsActive);
			}

			ViewData["search"] = search;
			ViewData["active"] = active;

			return View(await subs.OrderBy(s => s.SubcontractorName).ToListAsync());
        }

		// GET: Subcontractors/Create
		[Authorize(Policy = "HRAssistant")]
		public IActionResult Create()
        {
			Subcontractor sub = new Subcontractor();
            return View(sub);
        }

        // POST: Subcontractors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Policy = "HRAssistant")]
		public async Task<IActionResult> Create(Subcontractor subcontractor)
        {
            if (ModelState.IsValid)
            {
				subcontractor.CreatedDate = DateTime.Now;
				subcontractor.ModifiedDate = DateTime.Now;
				subcontractor.Id = _context.Subcontractors.Max(c => c.Id) + 1;

				_context.Add(subcontractor);
                await _context.SaveChangesAsync();

				TempData["Toast"] = Toasts.Created;
				return RedirectToAction(nameof(Index));
            }
            return View(subcontractor);
        }

        // GET: Subcontractors/Edit/5
		[Authorize(Policy = "HRAssistant")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcontractor = await _context.Subcontractors.FindAsync(id);
            if (subcontractor == null)
            {
                return NotFound();
            }
            return View(subcontractor);
        }

        // POST: Subcontractors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Policy = "HRAssistant")]
		public async Task<IActionResult> Edit(int id, Subcontractor subcontractor)
        {
            if (id != subcontractor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
				{
					_context.Update(subcontractor);
                    await _context.SaveChangesAsync();

					TempData["Toast"] = Toasts.Saved;
				}
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubcontractorExists(subcontractor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(subcontractor);
        }

		// GET: Subcontractors/Delete/5
		[Authorize(Policy = "HR")]
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcontractor = await _context.Subcontractors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subcontractor == null)
            {
                return NotFound();
            }

            return View(subcontractor);
        }

        // POST: Subcontractors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[Authorize(Policy = "HR")]
		public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subcontractor = await _context.Subcontractors.FindAsync(id);
            _context.Subcontractors.Remove(subcontractor);
            await _context.SaveChangesAsync();

			TempData["Toast"] = Toasts.Deleted;
			return RedirectToAction(nameof(Index));
        }

        private bool SubcontractorExists(int id)
        {
            return _context.Subcontractors.Any(e => e.Id == id);
        }
    }
}
