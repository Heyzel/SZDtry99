﻿using ERPSzakdolgozat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ERPSzakdolgozat.Controllers
{
	public class AppUsersController : Controller
	{
		private readonly ERPDBContext _context;

		public AppUsersController(ERPDBContext context)
		{
			_context = context;
		}

		// GET: Users
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> Index()
		{
			return View(await _context.AppUsers.ToListAsync());
		}

		// GET: Users/Details/5
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _context.AppUsers
				.FirstOrDefaultAsync(m => m.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// GET: Users/Create
		[Authorize(Policy = "Admin")]
		public IActionResult Create()
		{
			AppUser user = new AppUser();
			return View(user);
		}

		// POST: Users/Create
		[HttpPost]
		[Authorize(Policy = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(AppUser user)
		{
			if (ModelState.IsValid)
			{
				user.CreatedDate = DateTime.Now;
				user.ModifiedDate = DateTime.Now;
				user.Id = _context.AppUsers.Max(t => t.Id) + 1;

				_context.Add(user);
				await _context.SaveChangesAsync();

				TempData["Toast"] = "created-success";
				return RedirectToAction(nameof(Index));
			}
			return View(user);
		}

		// GET: Users/Edit/5
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _context.AppUsers.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			return View(user);
		}

		// POST: Users/Edit/5
		[HttpPost]
		[Authorize(Policy = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, AppUser user)
		{
			if (id != user.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(user);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!UserExists(user.Id))
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
			return View(user);
		}

		// GET: Users/Delete/5
		[Authorize(Policy = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _context.AppUsers
				.FirstOrDefaultAsync(m => m.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// POST: Users/Delete/5
		[HttpPost, ActionName("Delete")]
		[Authorize(Policy = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var user = await _context.AppUsers.FindAsync(id);
			_context.AppUsers.Remove(user);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool UserExists(int id)
		{
			return _context.AppUsers.Any(e => e.Id == id);
		}

		// Self User functions
		[HttpGet]
		public async Task<IActionResult> SelfEdit()
		{
			// TODO change own Name, Email, Mobile
			AppUser user = await _context.AppUsers.Where(u => u.ADName == User.Identity.Name).FirstOrDefaultAsync();

			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		[HttpPost]
		public async Task<IActionResult> SelfEdit(AppUser user)
		{
			if (ModelState.IsValid)
			{
				try
				{
					// Handling profile picture upload
					IFormFile newPic = Request.Form.Files["profile"];
					if (newPic != null)
					{
						if (newPic.FileName.Contains(".jpg")
							|| newPic.FileName.Contains(".png")
							|| newPic.FileName.Contains(".gif")
							|| newPic.FileName.Contains(".jpeg"))
						{
							using (var memoryStream = new MemoryStream())
							{
								await newPic.CopyToAsync(memoryStream);
								user.ProfilePicture = memoryStream.ToArray();

							}
						}
					}

					_context.Update(user);
					await _context.SaveChangesAsync();

					TempData["Toast"] = "saved-success";
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!UserExists(user.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction("SelfEdit");
			}

			return View(user);
		}

		/// <summary>
		/// For resetting the User's picture if it is inappropriate
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<IActionResult> ResetPicture(int id, string viewName)
		{
			AppUser user = await _context.AppUsers.FindAsync(id);

			if (user != null)
			{
				user.ProfilePicture = null;

				_context.Update(user);
				await _context.SaveChangesAsync();

				TempData["Toast"] = "saved-success";
			}
			else
			{
				return NotFound();
			}
			
			if (viewName == "SelfEdit")
			{
				return RedirectToAction("SelfEdit");
			}

			return RedirectToAction("Edit", new { id });
		}
	}
}