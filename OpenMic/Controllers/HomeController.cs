using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenMic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenMic.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        // Front page of the App, Displays the User's List
        public ActionResult Index(string artistGenre, string artist, string email)
        {
            // Fetch the genres from the User's DB
            var GenreLst = new List<string>();

            var GenreQry = from d in db.Users
                           orderby d.Genre
                           select d.Genre;

            GenreLst.AddRange(GenreQry.Distinct());
            ViewBag.artistGenre = new SelectList(GenreLst);

            var users = from u in db.Users
                        select u;


            // Search By Artist
            if (!string.IsNullOrEmpty(artist))
            {
                users = users.Where(x => x.Name.Contains(artist));
            }

            // Search By  Genre
            if (!string.IsNullOrEmpty(artistGenre))
            {
                users = users.Where(x => x.Genre == artistGenre);
            }

            // Search By Email
            if (!string.IsNullOrEmpty(email))
            {
                users = users.Where(x => x.Email.Contains(email));
            }

            // And return the Filtered (or not) User's list
            return View(users.ToList());
        }




        // Shows - Maps Methods
        // Shows the Map page
        public ActionResult maps(string searchString)
        {
            var userShows = Enumerable.Empty<ApplicationUser>();

            // Will take the searchString and if it is not null, will return a view with the list of all users that their name
            // contains the given string
            if (!String.IsNullOrEmpty(searchString))
            {
                userShows = from m in db.Users
                            select m;
                userShows = userShows.Where(s => s.Name.Contains(searchString));
            }
            return View(userShows);
        }

        // Shows the specific location of the next show for the selected Artist
        public ActionResult maps2(string id)
        {
            // Checks user's id (given as parameter) validity
            if (id != null)
            {
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                var currentUser = manager.FindById(id);

                // Pass user's name
                ViewBag.User = currentUser.Name;

                // If ther user has a show in the db
                if (currentUser.nextShow != null)
                {
                    ViewBag.place = currentUser.nextShow;
                }
                else
                    // else pass error string via viewbag
                    ViewBag.place = "Sorry, no shows Up ahead.";                
            }
            return View();
        }



        [HttpGet]       // AdminZone Get Method
        public ActionResult Admin()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            // Fetch All Tracks
            var tracks = from m in db.Tracks
                         select m;

            // If connected at all
            if (currentUser != null)
            {
                // if the connected user is an admin
                if (User.IsInRole("admin"))
                    return View(tracks.ToList());
                else
                    // Not admin
                    return View("~/Views/Home/NoAdmin.cshtml");
            }
                // Not connected at all
            else
                return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost]      // AdminZone Post Method
        public ActionResult Admin(int id=0){

            // Fetching Data from the form in the adminZone View Page
            // First Email is for turning normal user to admin and the second for the opposite
            string makeAdmin = Request.Form["Email1"].ToString();
            string makeSimple = Request.Form["Email2"].ToString();

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            // Find the actual users by email
            var givenUser1 = manager.FindByEmail(makeAdmin);
            var givenUser2 = manager.FindByEmail(makeSimple);

            // Ehecks validity for the first email
            if ((givenUser1 != null) && (givenUser2 != givenUser1) && (givenUser1 != currentUser))
            {
                manager.AddToRole(givenUser1.Id, "Admin");
                ViewBag.msg = "Success";
            }

            // Checks validity for the second email
            if ((givenUser1 != givenUser2) && (givenUser2 != null) && (givenUser2 != currentUser))
            {
                manager.RemoveFromRole(givenUser2.Id, "Admin");
                ViewBag.msg = "Success";
            }            

            return View(db.Tracks.ToList());
            
        }
    }
}