using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OLX.Data;
using OLX.Models;
using OLX.ViewModels;
using System.Diagnostics;

namespace OLX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly OLX2Context _context;

        public HomeController(ILogger<HomeController> logger, OLX2Context context, UserManager<Users> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Posty()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var posts = _context.Posts.ToList();
            var k = _context.Kosz.ToList();
            var c = _context.Comments.ToList();

            var vm = new PostsWithComments
            {
                Posts = posts,
                Kosz = k,
                Comments = c
            };

            return View(vm);
        }

        public IActionResult Transakcje()
        {
            var transactions = _context.Paragons
                .GroupBy(p => p.TransId)
                .Select(g => new TransactionViewModel
                {
                    TransId = g.Key,
                    Items = g.ToList()
                })
                .ToList();

            return View(transactions);
        }
        public IActionResult Details(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var post = _context.Posts
                .Include(x => x.User)
                .SingleOrDefault(x => x.Id == id);

            if (post == null)
                return NotFound();

            var comments = _context.Comments
                .Where(c => c.Post == id)
                .ToList();

            var kosz = _context.Kosz
                .Where(k => k.PostId == id)
                .ToList();

            var vm = new PostsWithComments
            {
                Posts = new List<Post> { post },
                Comments = comments,
                Kosz = kosz
            };

            return View(vm);
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult Oops()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            var posts = _context.Users.ToList();

            return View(posts);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Ban(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = _context.Users.SingleOrDefault(x => x.Id == id);

            if (user == null)
                return NotFound();

            user.Banned = !user.Banned;

            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Admin");
        }

        [Authorize(Roles = "User")]
        public IActionResult UserPanel()
        {
            var posts = _context.Posts.ToList();
            var k = _context.Kosz.ToList();
            var c = _context.Comments.ToList();

            var vm = new PostsWithComments
            {
                Posts = posts,
                Kosz = k,
                Comments = c
            };

            return View(vm);
        }

        public IActionResult Kosz()
        {
            var p = _context.Posts.ToList();
            var u = _context.Users.ToList();
            var k = _context.Kosz.ToList();

            var vm = new PostsWithUsers
            {
                Posts = p,
                Users = u,
                Kosz = k
            };

            return View(vm);
        }

        public async Task<IActionResult> CommentForm(Comment model)
        {
            var user = _userManager.GetUserAsync(User).Result;

            if (user.Banned == true)
            {
                return RedirectToAction("Oops");
            }
            else
            {
                model.UserId = user.Id;
                model.UserName = user.FullName;
            }

            _context.Comments.Add(model);

            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Create(int? id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (User.IsInRole("Admin"))
                return RedirectToAction("Admin", "Home");

            var user = _userManager.GetUserAsync(User).Result;

            if (user.Banned == true)
            {
                return RedirectToAction("Oops");
            }

            var model = new Post
            {
                UserId = user.Id,
                UserName = user.FullName,
                User = user
            };

            if (id != null)
            {
                var postsies = _context.Posts.SingleOrDefault(x => x.Id == id);

                return View(postsies);
            }

            return View(model);
        }

        public IActionResult DeleteKosz(int id)
        {
            var postsies = _context.Kosz.SingleOrDefault(x => x.Id == id);
            _context.Kosz.Remove(postsies);
            _context.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Delete(int id)
        {
            var postsies = _context.Posts.SingleOrDefault(x => x.Id == id);
            _context.Posts.Remove(postsies);
            foreach (var post in _context.Kosz)
            {
                if (post.PostId == id)
                {
                    _context.Kosz.Remove(post);
                }
            }
            _context.SaveChanges();
            return RedirectToAction("UserPanel");
        }

        public IActionResult Delete2(int id)
        {
            var postsies = _context.Posts.SingleOrDefault(x => x.Id == id);
            _context.Posts.Remove(postsies);
            foreach (var post in _context.Kosz)
            {
                if (post.PostId == id)
                {
                    _context.Kosz.Remove(post);
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Posty");
        }

        public IActionResult DeleteComment(int id)
        {
            var postsies = _context.Comments.SingleOrDefault(x => x.Id == id);
            _context.Comments.Remove(postsies);
            _context.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }


        [HttpPost]
        public async Task<IActionResult> PostForm(Post model, IFormFile file)
        {
            var user = _userManager.GetUserAsync(User).Result;

            if (user.Banned == true)
            {
                return RedirectToAction("Oops");
            }

            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                model.Picture = ms.ToArray();
            } 
            else
            {
                var existing = _context.Posts.AsNoTracking()
                                     .SingleOrDefault(x => x.Id == model.Id);
                if (existing != null)
                {
                    model.Picture = existing.Picture; // zachowujemy stare zdjêcie
                }
            }

            if (model.Id == 0)
                _context.Posts.Add(model);
            else
                _context.Posts.Update(model);

            await _context.SaveChangesAsync();
            return RedirectToAction("Posty");
        }

        [HttpGet]
        public IActionResult FilterPosts(string search, string sortBy, string sortOrder, string hideNotActual)
        {
            var posts = _context.Posts.AsQueryable();

            bool hide = hideNotActual == "true" || hideNotActual == "True";

            if (!string.IsNullOrWhiteSpace(search))
                posts = posts.Where(p =>
                    p.Title.Contains(search) ||
                    p.Description.Contains(search));

            if (hide)
                posts = posts.Where(p => p.Aktualne);

            sortBy = sortBy?.ToLower();
            sortOrder = sortOrder?.ToLower();

            switch (sortBy)
            {
                case "price":
                    posts = sortOrder == "asc" ? posts.OrderBy(p => p.Price) : posts.OrderByDescending(p => p.Price);
                    break;

                case "title":
                    posts = sortOrder == "asc" ? posts.OrderBy(p => p.Title) : posts.OrderByDescending(p => p.Title);
                    break;

                case "date":
                default:
                    posts = sortOrder == "asc" ? posts.OrderBy(p => p.PostedDate) : posts.OrderByDescending(p => p.PostedDate);
                    break;
            }

            var vm = new PostsWithComments
            {
                Posts = posts.ToList(),
                Comments = _context.Comments.ToList(),
                Kosz = _context.Kosz.ToList()
            };

            return PartialView("_PostsPartial", vm);
        }

        public async Task<IActionResult> KoszForm(Kosz model)
        {
            var user = _userManager.GetUserAsync(User).Result;

            if (user.Banned == true)
            {
                return RedirectToAction("Oops");
            }

            _context.Kosz.Add(model);

            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Kup()
        {
            var idsuper = -1;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (user.Banned == true)
                return RedirectToAction("Oops");

            var kosz = await _context.Kosz
                .Where(k => k.UserId == user.Id)
                .ToListAsync();

            if (!kosz.Any())
                return RedirectToAction("Kosz");

            var postIds = kosz.Select(k => k.PostId).Distinct().ToList();

            var posts = await _context.Posts
                .Where(p => postIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in kosz)
            {
                var post = posts.FirstOrDefault(p => p.Id == item.PostId);

                if (post == null)
                    continue;

                if (!post.Aktualne)
                    continue;

                if (idsuper == -1)
                    idsuper = post.Id;

                var seller = await _userManager.FindByIdAsync(post.UserId);

                var trans = new Paragon
                {
                    PostName = post.Title,
                    PostPrice = post.Price,
                    UserId = user.Id,
                    TransId = idsuper,
                    Contact = seller.Email,
                };


                post.Aktualne = false;
                _context.Paragons.Add(trans);
                _context.Kosz.Remove(item);
                _context.Posts.Update(post);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Kosz");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
