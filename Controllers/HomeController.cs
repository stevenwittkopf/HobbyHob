using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HobbyHub.Models;

namespace HobbyHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Context _context;

        public int? UserId {
            get => HttpContext.Session.GetInt32("userId");
            set
            {
                if (!value.HasValue)
                {
                    HttpContext.Session.Remove("userId");
                }
                else
                {
                    HttpContext.Session.SetInt32("userId", value.Value);
                }
            }
        }

        public HomeController(ILogger<HomeController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            if (this.UserId.HasValue)
            {
                return RedirectToAction("Hobbies");
            }
            else
            {
                return RedirectToAction("LoginRegistration");
            }
        }

        [HttpGet("/LoginRegistration")]
        public IActionResult LoginRegistration()
        {
            if (this.UserId.HasValue){
                return RedirectToAction("Hobbies");
            }
            return View();
        }

        [HttpPost("/Login")]
        public IActionResult Login(LoginCredentials credentials)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginRegistration");
            }
            var user = _context.Users
                .FirstOrDefault(user => user.Username == credentials.LoginUsername);
            if (user == null){
                ModelState.AddModelError("LoginUsername", "Invalid username/password!");
                return View("LoginRegistration");
            }
            var passwordHasher = new PasswordHasher<LoginCredentials>();
            if (passwordHasher.VerifyHashedPassword(credentials, user.Password, credentials.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("LoginUsername", "Invalid username/password!");
                return View("LoginRegistration");
            }
            this.UserId = user.UserId;
            return RedirectToAction("Hobbies");
        }

        [HttpPost("/Register")]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("LoginRegistration");
            }
            if (_context.Users.Any(existingUser => existingUser.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username is already in use!");
                return View("LoginRegistration");
            }
            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            this.UserId = user.UserId;
            return RedirectToAction("Hobbies");
        }


        [HttpGet("/Logout")]
        public IActionResult Logout()
        {
            if (this.UserId.HasValue)
            {
                this.UserId = null;
            }
            return RedirectToAction("LoginRegistration"); 
        }

        [HttpGet("/Hobbies")]
        public IActionResult Hobbies()
        {
            if (!this.UserId.HasValue)
            {
                return RedirectToAction("LoginRegistration");
            }
            ViewBag.hobbies = _context.Hobbies
                .Include(hobby => hobby.Enthusiasts)
                .OrderByDescending(hobby => hobby.Enthusiasts.Count)
                .ToList();
            // TODO
            Func<string, List<string>> GetTopHobbiesByProficiency = (profiency) =>
            {
                var userHobbiesByProficiency = _context.UserHobbies
                    .Include(userHobby => userHobby.Hobby)
                    .Where(userHobby => userHobby.Proficiency == profiency);
                var frequencyTable = new Dictionary<string, int>();
                foreach (var userHobby in userHobbiesByProficiency)
                {
                    if (!frequencyTable.ContainsKey(userHobby.Hobby.Name))
                    {
                        frequencyTable[userHobby.Hobby.Name] = 0;
                    }
                    frequencyTable[userHobby.Hobby.Name]++;
                }
                var result = new List<string>();
                foreach (var entry in frequencyTable)
                {
                    if (result.Count == 0 || entry.Value == frequencyTable[result[0]])
                    {
                        result.Add(entry.Key);
                    }
                    else if (entry.Value > frequencyTable[result[0]])
                    {
                        result = new List<string>
                        {
                            entry.Key
                        };
                    }
                }
                return result;
            };
            ViewBag.topNoviceHobbies = GetTopHobbiesByProficiency("Novice");
            ViewBag.topIntermediateHobbies = GetTopHobbiesByProficiency("Intermediate");
            ViewBag.topExpertHobbies = GetTopHobbiesByProficiency("Expert");
            return View();
        }

        [HttpGet("/Hobbies/New")]
        public IActionResult HobbyCreation()
        {
            if (!this.UserId.HasValue){
                return RedirectToAction("LoginRegistration");
            }
            return View();
        }

        [HttpPost("/Hobbies/New")]
        public IActionResult CreateHobby(Hobby hobby)
        {
            if (!ModelState.IsValid)
            {
                return View("HobbyCreation");
            }
            else if (_context.Hobbies.Any(existingHobby => existingHobby.Name == hobby.Name)){
                ModelState.AddModelError("Name", "Name is already in use!");
                return View("HobbyCreation");
            }
            _context.Add(hobby);
            _context.SaveChanges();
            return RedirectToAction("HobbyInfo", new 
            {
                hobbyId = hobby.HobbyId
            });
        }

        [HttpGet("/Hobbies/{hobbyId}")]
        public IActionResult HobbyInfo(int hobbyId)
        {
            if (!this.UserId.HasValue)
            {
                return RedirectToAction("LoginRegistration");
            }
            ViewBag.hobby = _context.Hobbies
                .Include(hobby => hobby.Enthusiasts)
                    .ThenInclude(userHobby => userHobby.Enthusiast)
                .SingleOrDefault(hobby => hobby.HobbyId == hobbyId);
            if (ViewBag.hobby == null)
            {
                return RedirectToAction("Hobbies");
            }
            ViewBag.user = _context.Users
                .SingleOrDefault(user => user.UserId == this.UserId);
            return View();
        }

        [HttpPost("/Hobbies/{hobbyId}")]
        public IActionResult CreateUserHobby(int hobbyId, UserHobby userHobby)
        {
            if (!ModelState.IsValid)
            {
                return View("Hobbies");
            }
            _context.UserHobbies.Add(userHobby);
            _context.SaveChanges();
            return RedirectToAction("HobbyInfo", new 
            {
                hobbyId = hobbyId
            });
        }

        [HttpGet("/Hobbies/{hobbyId}/Edit")]
        public IActionResult HobbyEditing(int hobbyId)
        {
            if (!this.UserId.HasValue)
            {
                return RedirectToAction("LoginRegistration");
            }
            ViewBag.hobby = _context.Hobbies
                .SingleOrDefault(hobby => hobby.HobbyId == hobbyId);
            if (ViewBag.hobby == null)
            {
                return RedirectToAction("Hobbies");
            }
            return View();
        }

        [HttpPost("/Hobbies/{hobbyId}/Edit")]
        public IActionResult UpdateHobby(int hobbyId, Hobby updatedHobby)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.hobby = updatedHobby;
                return View("HobbyEditing");
            }
            else if (_context.Hobbies.Any(existingHobby => existingHobby.Name == updatedHobby.Name && existingHobby.HobbyId != updatedHobby.HobbyId))
            {
                ModelState.AddModelError("Name", "The name given is already in use by another hobby!");
                ViewBag.hobby = updatedHobby;
                return View("HobbyEditing");
            }
            var existingHobby = _context.Hobbies
                .SingleOrDefault(existingHobby => existingHobby.HobbyId == hobbyId);
            existingHobby.Name = updatedHobby.Name;
            existingHobby.Description = updatedHobby.Description;
            _context.SaveChanges();
            return RedirectToAction("HobbyInfo", new
            {
                hobbyId = hobbyId
            });
        }


    }
}
