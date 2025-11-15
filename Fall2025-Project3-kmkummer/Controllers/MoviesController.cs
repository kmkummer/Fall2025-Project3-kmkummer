using Azure;
using Azure.AI.OpenAI;
using System.IO;
using Fall2025_Project3_kmkummer.Data;
using Microsoft.AspNetCore.Http;
using Fall2025_Project3_kmkummer.Models;
using Fall2025_Project3_kmkummer.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VaderSharp2;

namespace Fall2025_Project3_kmkummer.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;



        public MoviesController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }



        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gptUri = _config["ChatGPT:GPT_URI"];
            var gptApiKey = _config["ChatGPT:GPT_ApiKey"];
            var gptDeployment = _config["ChatGPT:DeploymentName"];

            var movie = await _context.Movie
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var actors = movie.MovieActors
                .Where(ma => ma.Actor != null)
                .Select(ma => ma.Actor!)
                .ToList();


            var endpoint = new Uri(gptUri);
            var apiKey = gptApiKey;
            var deploymentName = gptDeployment;

            var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
            ChatClient chatClient = azureClient.GetChatClient(deploymentName);

            List<ChatMessage> messages = new List<ChatMessage>()
            {
                new SystemChatMessage("You are a film critic"),
                new UserChatMessage($"Generate 10 brief, unique movie reviews for '{movie.Title}' released in {movie.Year}.")
            };


            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages);

            var text = result.Value.Content[0].Text;

            var matches = System.Text.RegularExpressions.Regex.Matches(
                text,
                @"^\s*\d+[\.\)]\s*(.+)$",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            var reviews = matches
                .Select(m => m.Groups[1].Value.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(10)
                .ToList();

            var analyzer = new SentimentIntensityAnalyzer();

            SentimentAnalysisResults sentiment;
            List<double> reviewSentiments = new List<double>();
            double avgSentiment = 0;

            for (int i = 0; i < 10; i++)
            {
                sentiment = analyzer.PolarityScores(reviews.ElementAt(i));
                reviewSentiments.Add(sentiment.Compound);
                avgSentiment += sentiment.Compound;
            }

            avgSentiment = avgSentiment / 10;

            var vm = new MovieDetailsViewModel()
            {
                Movie = movie,
                Actors = actors,
                AIReviews = reviews,
                ReviewSentiments = reviewSentiments,
                AverageSentiment = avgSentiment

            };

            return View(vm);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDB,Genre,Year")] Movie movie, IFormFile? PosterFile)
        {
            if (ModelState.IsValid)
            {
                if (PosterFile != null && PosterFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await PosterFile.CopyToAsync(ms);
                    movie.Poster = ms.ToArray();
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDB,Genre,Year")] Movie movie, IFormFile? PosterFile)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            var existingMovie = await _context.Movie.FindAsync(id);
            if (existingMovie == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingMovie.Title = movie.Title;
                existingMovie.IMDB = movie.IMDB;
                existingMovie.Genre = movie.Genre;
                existingMovie.Year = movie.Year;

                if (PosterFile != null && PosterFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await PosterFile.CopyToAsync(ms);
                    existingMovie.Poster = ms.ToArray();
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(existingMovie.Id))
                        return NotFound();
                    else
                        throw;
            }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Poster(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie == null || movie.Poster == null || movie.Poster.Length == 0)
            {
                return NotFound();
            }

            return File(movie.Poster, "image/jpeg");
        }
    }
}

