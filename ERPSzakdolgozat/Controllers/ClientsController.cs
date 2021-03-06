﻿using ERPSzakdolgozat.Helpers;
using ERPSzakdolgozat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERPSzakdolgozat.Controllers
{
	public class ClientsController : MyController
	{
		public ClientsController(ERPDBContext context) : base(context)
		{
			_context = context;
		}

		// GET: Clients
		public async Task<IActionResult> Index(string search, bool active = true)
		{
			IQueryable<Client> clients = _context.Clients.AsNoTracking();

			if (!string.IsNullOrEmpty(search))
			{
				clients = clients.Where(c => c.ClientId.ToLower().Contains(search.ToLower())
					|| c.ClientName.ToLower().Contains(search.ToLower())
					|| c.ContactName.ToLower().Contains(search.ToLower()));
			}
			if (active)
			{
				clients = clients.Where(c => c.Active);
			}

			ViewData["search"] = search;
			ViewData["active"] = active;

			return View(await clients.OrderBy(c => c.ClientName).ToListAsync());
		}

		// GET: Clients/Details/5
		[Authorize(Policy = "Sales")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var client = await _context.Clients
				.FirstOrDefaultAsync(m => m.Id == id);
			if (client == null)
			{
				return NotFound();
			}

			return View(client);
		}

		// GET: Clients/Create
		[Authorize(Policy = "Sales")]
		public IActionResult Create()
		{
			Client client = new Client();
			return View(client);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = "Sales")]
		public async Task<IActionResult> Create(Client client)
		{
			if (ModelState.IsValid)
			{
				client.CreatedDate = DateTime.Now;
				client.ModifiedDate = DateTime.Now;
				client.Id = _context.Clients.Max(t => t.Id) + 1;

				_context.Add(client);
				await _context.SaveChangesAsync();

				TempData["Toast"] = Toasts.Created;
				return RedirectToAction(nameof(Index));
			}
			return View(client);
		}

		// GET: Clients/Edit/5
		[Authorize(Policy = "Sales")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var client = await _context.Clients.FindAsync(id);
			if (client == null)
			{
				return NotFound();
			}
			return View(client);
		}

		// POST: Clients/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = "Sales")]
		public async Task<IActionResult> Edit(int id, Client client)
		{
			if (id != client.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(client);
					await _context.SaveChangesAsync();

					TempData["Toast"] = Toasts.Saved;
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ClientExists(client.Id))
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
			return View(client);
		}

		// GET: Clients/Delete/5
		[Authorize(Policy = "Sales")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var client = await _context.Clients
				.FirstOrDefaultAsync(m => m.Id == id);
			if (client == null)
			{
				return NotFound();
			}

			return View(client);
		}

		// POST: Clients/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = "Sales")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var client = await _context.Clients.FindAsync(id);
			_context.Clients.Remove(client);
			await _context.SaveChangesAsync();

			TempData["Toast"] = Toasts.Deleted;
			return RedirectToAction(nameof(Index));
		}

		private bool ClientExists(int id)
		{
			return _context.Clients.Any(e => e.Id == id);
		}
	}
}