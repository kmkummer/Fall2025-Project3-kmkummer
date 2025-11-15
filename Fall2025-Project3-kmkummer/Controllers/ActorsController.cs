using Azure;
using Azure.AI.OpenAI;
using Fall2025_Project3_kmkummer.Data;
using Fall2025_Project3_kmkummer.Models;
using Fall2025_Project3_kmkummer.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public ActorsController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var gptUri = _config["ChatGPT:GPT_URI"];
            var gptApiKey = _config["ChatGPT:GPT_ApiKey"];
            var gptDeployment = _config["ChatGPT:DeploymentName"];

            var actor = await _context.Actor
                     .Include(a => a.MovieActors)
                     .ThenInclude(ma => ma.Movie)
                     .FirstOrDefaultAsync(a => a.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var movies = actor.MovieActors
                 .Where(ma => ma.Movie != null)
                 .Select(ma => ma.Movie!)
                 .ToList();


            var endpoint = new Uri(gptUri);
            var apiKey = gptApiKey;
            var deploymentName = gptDeployment;



            var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
            ChatClient chatClient = azureClient.GetChatClient(deploymentName);

            var messages = new ChatMessage[]
            {
                new SystemChatMessage($"You are a Twitter user"),
                new UserChatMessage($"Generate 20 brief, unique tweets about '{actor.Name}'" +
                        "Return them as a numbered list, like: 1. Tweet one ... 2. Tweet two ...")
            };
            ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages);

            var text = result.Value.Content[0].Text;

            var matches = System.Text.RegularExpressions.Regex.Matches(
                text,
                @"^\s*\d+[\.\)]\s*(.+)$",
                System.Text.RegularExpressions.RegexOptions.Multiline);

            var tweets = matches
                .Select(m => m.Groups[1].Value.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(20)
                .ToList();

            var analyzer = new SentimentIntensityAnalyzer();

            SentimentAnalysisResults sentiment;
            List<double> tweetSentiments = new List<double>();
            double avgSentiment = 0;

            for (int i = 0; i < 20; i++)
            {
                sentiment = analyzer.PolarityScores(tweets.ElementAt(i));
                tweetSentiments.Add(sentiment.Compound);
                avgSentiment += sentiment.Compound;
            }

            avgSentiment = avgSentiment / 10;


            var vm = new ActorDetailsViewModel()
            {
                Actor = actor,
                Movies = movies,
                AITweets = tweets,
                TweetSentiments = tweetSentiments,
                AverageSentiment = avgSentiment

            };

            return View(vm);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Link,Age")] Actor actor, IFormFile? PhotoFile)
        {
            if (ModelState.IsValid)
            {
                if (PhotoFile != null && PhotoFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await PhotoFile.CopyToAsync(ms);
                    actor.Photo = ms.ToArray();
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Link,Age")] Actor actor, IFormFile? PhotoFile)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            var existingActor = await _context.Actor.FindAsync(id);
            if (existingActor == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingActor.Name = actor.Name;
                existingActor.Link = actor.Link;
                existingActor.Gender = actor.Gender;
                existingActor.Age = actor.Age;

                if (PhotoFile != null && PhotoFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await PhotoFile.CopyToAsync(ms);
                    existingActor.Photo = ms.ToArray();
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(existingActor.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Photo(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor == null || actor.Photo == null || actor.Photo.Length == 0)
            {
                return NotFound();
            }

            return File(actor.Photo, "image/jpeg");
        }
    }

}
