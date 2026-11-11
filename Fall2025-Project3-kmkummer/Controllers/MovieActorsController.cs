using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_kmkummer.Data;
using Fall2025_Project3_kmkummer.Models;
using Fall2025_Project3_kmkummer.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Fall2025_Project3_kmkummer.Controllers
{
    public class MovieActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MovieActors
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: MovieActors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movieActor == null)
            {
                return NotFound();
            }

            return View(movieActor);
        }

        // GET: MovieActors/Create
        public async Task<IActionResult> Create()
        {
            var vm = new MovieActorEditViewModel
            {
                MovieOptions = await _context.Movie
                    .OrderBy(m => m.Title)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Title
                    }).ToListAsync(),

                ActorOptions = await _context.Actor
                    .OrderBy(a => a.Name)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToListAsync()
            };

            return View(vm);
        }

        // POST: MovieActors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieActorEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.MovieActors
                    .AnyAsync(ma => ma.MovieId == vm.MovieId && ma.ActorId == vm.ActorId);

                if (!exists)
                {
                    var entity = new MovieActor
                    {
                        MovieId = vm.MovieId,
                        ActorId = vm.ActorId
                    };

                    _context.MovieActors.Add(entity);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "That actor is already linked to that movie.");
            }

            vm.MovieOptions = await _context.Movie
                .OrderBy(m => m.Title)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Title
                }).ToListAsync();

            vm.ActorOptions = await _context.Actor
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToListAsync();

            return View(vm);
        }

        // GET: MovieActors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActors
                .Include(ma => ma.Movie)
                .Include(ma => ma.Actor)
                .FirstOrDefaultAsync(ma => ma.Id == id);

            if (movieActor == null)
            {
                return NotFound();
            }

            var vm = new MovieActorEditViewModel
            {
                Id = movieActor.Id,
                MovieId = movieActor.MovieId,
                ActorId = movieActor.ActorId,
                MovieOptions = await _context.Movie
                    .OrderBy(m => m.Title)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Title
                    }).ToListAsync(),
                ActorOptions = await _context.Actor
                    .OrderBy(a => a.Name)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToListAsync()
            };

            return View(vm);
        }

        // POST: MovieActors/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieActorEditViewModel vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // prevent duplicate relationships, but allow the same record
                bool exists = await _context.MovieActors
                    .AnyAsync(ma =>
                        ma.Id != vm.Id &&
                        ma.MovieId == vm.MovieId &&
                        ma.ActorId == vm.ActorId);

                if (exists)
                {
                    ModelState.AddModelError(string.Empty, "That actor is already linked to that movie.");
                }
                else
                {
                    var entity = await _context.MovieActors.FindAsync(vm.Id);
                    if (entity == null)
                    {
                        return NotFound();
                    }

                    entity.MovieId = vm.MovieId;
                    entity.ActorId = vm.ActorId;

                    try
                    {
                        _context.Update(entity);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MovieActorExists(vm.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            // repopulate dropdowns when there are validation errors
            vm.MovieOptions = await _context.Movie
                .OrderBy(m => m.Title)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Title
                }).ToListAsync();

            vm.ActorOptions = await _context.Actor
                .OrderBy(a => a.Name)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToListAsync();

            return View(vm);
        }

        // GET: MovieActors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movieActor = await _context.MovieActors
                .Include(m => m.Actor)
                .Include(m => m.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movieActor == null)
            {
                return NotFound();
            }

            return View(movieActor);
        }

        // POST: MovieActors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movieActor = await _context.MovieActors.FindAsync(id);
            if (movieActor != null)
            {
                _context.MovieActors.Remove(movieActor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieActorExists(int id)
        {
            return _context.MovieActors.Any(e => e.Id == id);
        }
    }
}
