using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OpenMic.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Text.RegularExpressions;

namespace OpenMic.Controllers
{
    public class TracksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tracks
        public ActionResult Index(string trackGenre, string searchString, string artist, string thoughtContent)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            if (currentUser != null)
                @ViewBag.CurUserID = currentUser.Id;

       // Track Search
            var GenreLst = new List<string>();
            var GenreQry = from d in db.Tracks
                           orderby d.Genre
                           select d.Genre;

            GenreLst.AddRange(GenreQry.Distinct());
            ViewBag.trackGenre = new SelectList(GenreLst);

            var tracks = from m in db.Tracks
                         select m;


            // Search by Title
            if (!String.IsNullOrEmpty(searchString))
            {
                tracks = tracks.Where(s => s.Title.Contains(searchString));
            }

            // Search by Genre
            if (!string.IsNullOrEmpty(trackGenre))
            {
                tracks = tracks.Where(x => x.Genre == trackGenre);
            }

            // Search by Artist
            if (!string.IsNullOrEmpty(artist))
            {
                tracks = tracks.Where(x => x.Artists.Name.Contains(artist));
            }

            // Search by Thougth  -- Usage of Join
            if (!string.IsNullOrEmpty(thoughtContent))
            {
                var thoughts = from n in db.Thoughts
                             select n;
                thoughts = thoughts.Where(s => s.Content.Contains(thoughtContent));

                tracks =
                from track in tracks
                join thought in thoughts on track.ID equals thought.TrackID
                select track;
            }

           // Gold / Trash Graphical display in index
            string goldenString = null;
            string trashedString = null;
            foreach (Track x in db.Tracks)
            {
                int golden = (int)x.Gold;
                int trashed = (int)x.Trash;
                int total = (int)x.TotalRankers;
                if (total!=0) { 
                double goldenResult = (double)Decimal.Divide(golden, total);
                double trashedResult = (double)Decimal.Divide(trashed, total);
                goldenString += goldenResult * 100 + ",";
                trashedString += trashedResult * 100 + ",";
                }
                else
                {
                    goldenString += 0 + ",";
                    trashedString += 0 + ",";
                }
               
            }
            ViewBag.golden = goldenString;
            ViewBag.trashed = trashedString;
            // Group By (Shown only to admin at the bottom of tracksindex)
            var ranks = (from t in db.Tracks
                         group t by (t.Gold - t.Trash) into g
                         select new { rank = g.Key, tracks = g.Count() });
            @ViewBag.ranked = ranks;

            return View(tracks.ToList());
        }


 


// Your Art display
        public ActionResult Uploaded()
        {

            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            ViewBag.currentUser = currentUser.Name;

            var tracks = from m in db.Tracks
                         select m;
            tracks = tracks.Where(x => x.ArtistsID == currentUser.Id);

            return View(tracks.ToList());

        }




//ThoughtSearcher
        public ActionResult ThoughtSearcher(string trackGenre, string SearchString, string artist)
        {

            var GenreLst = new List<string>();

            var GenreQry = from d in db.Tracks
                           orderby d.Genre
                           select d.Genre;

            GenreLst.AddRange(GenreQry.Distinct());
            ViewBag.trackGenre = new SelectList(GenreLst);


            var tracks = from n in db.Tracks
                         select n;

            //Search by tracks
            if (!String.IsNullOrEmpty(trackGenre))
            {

               tracks = tracks.Where(s => s.Genre == trackGenre);
            }

            //Search by title
            if (!String.IsNullOrEmpty(SearchString))
            {
                tracks = tracks.Where(s => s.Title.Contains(SearchString));
            }

            //Search by artist
            if (!String.IsNullOrEmpty(artist))
            {
                tracks = tracks.Where(s => s.Artists.Name.Contains(artist));
            }

            //Join
            var thoughts =
            from track in tracks
            join thought in db.Thoughts on track.ID equals thought.TrackID
            select thought;

            return View(thoughts.ToList());
        }
       


     




        // GET: Tracks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Track track = db.Tracks.Find(id);
            if (track == null)
            {
                return HttpNotFound();
            }
            return View(track);
        }

        // GET: Tracks/Create
        public ActionResult Create()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId()); ///User.Identity.GetUserId());
            if (currentUser != null)
            {
                ViewBag.id = currentUser.Id;
                ViewBag.name = manager.FindById(currentUser.Id).UserName;
                return View();

            }
            ViewBag.error = "Please Login First!";
            return RedirectToAction("Login", "Account");
        }

        // POST: Tracks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Title,Video,Date,Genre")] Track track)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
            var regex = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
            foreach (Match match in regex.Matches(track.Video))
                foreach (var groupdata in match.Groups.Cast<Group>().Where(groupdata => !groupdata.ToString().StartsWith("http://") && !groupdata.ToString().StartsWith("https://") && !groupdata.ToString().StartsWith("youtu") && !groupdata.ToString().StartsWith("www.")))
                    track.Video = "https://www.youtube.com/embed/" + groupdata.ToString();

            track.ArtistsID = currentUser.Id;
            track.Trash = 0;
            track.Gold = 0;
            track.TotalRankers = 0;
            track.Date = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.Tracks.Add(track);
                currentUser.Uploaded.Add(track);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return RedirectToAction("index");
        }

        // GET: Tracks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            Track track = db.Tracks.Find(id);
            if (currentUser != null)
            {
                if ((currentUser.Id != track.ArtistsID) && !(User.IsInRole("admin")))
                {
                    return RedirectToAction("index");
                }
            }
            else
                return RedirectToAction("Login", "Account");
            if (track == null)
            {
                return HttpNotFound();
            }
            return View(track);
        }

        // POST: Tracks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Title,IdentityUserID,Video,Date,Genre")] Track track)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            var originalTrack = db.Tracks.Find(track.ID);
            if (ModelState.IsValid)
            {
                if (currentUser != null)
                {
                    if ((currentUser.Id != originalTrack.ArtistsID) && !(User.IsInRole("admin")))
                    {
                        return RedirectToAction("index");
                    }
                }
                else
                    return RedirectToAction("Login", "Account");
                originalTrack.Genre = track.Genre;
                originalTrack.Title = track.Title;
                originalTrack.Video = track.Video;
                originalTrack.Date = track.Date;
                originalTrack.ArtistsID = currentUser.Id;

                db.Entry(originalTrack).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(track);
        }


        public ActionResult Gold(int? id)
        {
            var currentTrack = db.Tracks.Find(id);
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            if ((currentTrack != null) && (currentUser != null))
            {
                currentTrack.TotalRankers++;
                currentTrack.Gold++;
                currentUser.Playlist.Add(currentTrack);
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Tracks");
        }



        public ActionResult Trash(int id)
        {
            var currentTrack = db.Tracks.Find(id);
            if (currentTrack != null)
            {
                currentTrack.TotalRankers++;
                currentTrack.Trash++;
            }
            db.SaveChanges();

            return RedirectToAction("Index", "Tracks");
        }

        

        // GET: Tracks/Delete/5
        public ActionResult Delete(int? id)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Track track = db.Tracks.Find(id);
            if ((currentUser.Id != track.ArtistsID)&&!(User.IsInRole("admin")))
            {
                return RedirectToAction("index");
            }
            if (track == null)
            {
                return HttpNotFound();
            }
            return View(track);
        }

        // POST: Tracks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            Track track = db.Tracks.Find(id);
            if ((currentUser.Id != track.ArtistsID)&&!(User.IsInRole("admin")))
            {
                return RedirectToAction("index");
            }
            db.Tracks.Remove(track);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        // GET: Thoughts/Create
        public ActionResult AddThought(int? trackId)
        {
            // If the user is not logged in redirect to login
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // If there is a track such as the given id
            if (db.Tracks.Find(trackId) != null)
            {
                ViewBag.trackid = trackId;
                return View();
            }
            return RedirectToAction("Index");
        }

        // POST: Thoughts/Create
        [HttpPost]
        public ActionResult AddThought([Bind(Include = "Content,TrackID")] Thought thought)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(User.Identity.GetUserId());

            // Sets the thought. user id to the current user
            thought.ApplicationUserID = currentUser.Id;
            thought.date = DateTime.Now;

            // if the new thougth is valid
            if (ModelState.IsValid)
            {
                // if the given track id was found
                if (db.Tracks.Find(thought.TrackID) != null)
                {
                    // add the thought to the db
                    db.Thoughts.Add(thought);

                    // add the thought to the "thougths" icollection of the track
                    db.Tracks.Find(thought.TrackID).Thoughts.Add(thought);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            //else
            return View(thought);
        }

        // Thoughts Index
        public ActionResult thoughtIndex(int trackid = 0)
        {
            //Pass the current user's id to the view via viewbag
            ViewBag.userID = User.Identity.GetUserId();

            // if the Track was found (by the given id)
            if (db.Tracks.Find(trackid) != null)
            {
                Track track = db.Tracks.Find(trackid);
                // For the headline
                ViewBag.trackName = track.Title;
                return View(track.Thoughts.ToList());
            }
            else
                return RedirectToAction("Index");
        }


        // GET: Tracks/DeleteThought/5
         public ActionResult DeleteThought(int? id)
         {
             var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
             var currentUser = manager.FindById(User.Identity.GetUserId());
             if (id == null)
             {
                 return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
             }
             Thought thought = db.Thoughts.Find(id);
             if (thought == null)
             {
                 return HttpNotFound();
             }
             if ((currentUser.Id != thought.ApplicationUserID) && !(User.IsInRole("admin")))
             {
                 return RedirectToAction("index");
             }
             db.Tracks.Find(thought.TrackID).Thoughts.Remove(thought);
             db.Thoughts.Remove(thought);
             db.SaveChanges();
             return Redirect(Request.UrlReferrer.ToString());
         }

         // GET: Tracks/ThougthEdit/5
         public ActionResult ThougthEdit(int? id)
         {
             if (id == null)
             {
                 return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
             }
             var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
             var currentUser = manager.FindById(User.Identity.GetUserId());
             Thought thought = db.Thoughts.Find(id);
             if (currentUser != null)
             {
                 if ((currentUser.Id != thought.ApplicationUserID) && !(User.IsInRole("admin")))
                 {
                     return RedirectToAction("index");
                 }
             }
             else
                 return RedirectToAction("Login", "Account");
             if (thought == null)
             {
                 return HttpNotFound();
             }
             return View(thought);
         }


         // POST: Tracks/ThougthEdit/5
         // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
         // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
         [HttpPost]
         [ValidateAntiForgeryToken]
         public ActionResult ThougthEdit([Bind(Include = "ID,TrackID,ApplicationUserID,Content,date")] Thought thought)
         {
             var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
             var currentUser = manager.FindById(User.Identity.GetUserId());
             var originalThougth = db.Thoughts.Find(thought.ID);
             if (ModelState.IsValid)
             {
                 if (currentUser != null)
                 {
                     if ((currentUser.Id != originalThougth.ApplicationUserID) && !(User.IsInRole("admin")))
                     {
                         return RedirectToAction("index");
                     }
                 }
                 else
                     return RedirectToAction("Login", "Account");
                 originalThougth.Content = thought.Content ;
                 originalThougth.date = DateTime.Now;


                 db.Entry(originalThougth).State = EntityState.Modified;
                 db.SaveChanges();
                 return RedirectToAction("Index");
             }
             return View(thought);
         }

    }
}
