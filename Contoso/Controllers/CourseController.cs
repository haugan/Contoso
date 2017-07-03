using Contoso.DAL;
using Contoso.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Contoso.Controllers
{
    public class CourseController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Course
        public ActionResult Index(string sortOrder, string searchString)
        {
            ViewBag.TitleSortParam = (String.IsNullOrEmpty(sortOrder)) ? "title_desc" : ""; // Empty is default; title_asc
            ViewBag.CreditsSortParam = (sortOrder == "credits_asc") ? "credits_desc" : "credits_asc";

            var courses = from c in db.Courses select c;

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString.ToUpper();
                courses = courses.Where(c => 
                    c.Title.ToUpper().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    courses = courses.OrderByDescending(c => c.Title);
                    break;
                case "credits_asc":
                    courses = courses.OrderBy(c => c.Credits);
                    break;
                case "credits_desc":
                    courses = courses.OrderByDescending(c => c.Credits);
                    break;
                default:
                    courses = courses.OrderBy(c => c.Title);
                    break;
            }

            var coursesList = courses.ToList(); // ToList() executes SQL query
            db.Dispose();
            return View(coursesList);
        }

        // GET: Course/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Course course = db.Courses.Find(id);
            if (course == null)
            {
                db.Dispose();
                return HttpNotFound($"No course found matching id: {id}.");
            }

            db.Dispose();
            return View(course);
        }

        // GET: Course/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Course/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseID,Title,Credits")] Course course)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Courses.Add(course);
                    db.SaveChanges();
                    db.Dispose();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException dex)
            {
                Console.WriteLine($"DataException: {dex.Message}");
                ModelState.AddModelError("", "Unable to register new course, please try again.");
            }

            db.Dispose();
            return View(course);
        }

        // GET: Course/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var course = db.Courses.Find(id);
            if (course == null)
            {
                db.Dispose();
                return HttpNotFound();
            }

            db.Dispose();
            return View(course);
        }

        // POST: Course/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var courseToUpdate = db.Courses.Find(id);
            // Set Modified flag on entity, whitelisted fields in parameters.
            // Future data model fields are automatically "blacklisted" until added here.
            if (TryUpdateModel(courseToUpdate, new string[] { "CourseID", "Title", "Credits" }))
            {
                try
                {
                    db.SaveChanges();
                    db.Dispose();
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    Console.WriteLine($"DataException: {dex.Message}");
                    ModelState.AddModelError("", "Unable to save changes, please try again.");
                }
            }

            db.Dispose();
            return View(courseToUpdate);
        }

        // GET: Course/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage = "Unable to delete course, please try again.";

            var course = db.Courses.Find(id);
            if (course == null)
            {
                db.Dispose();
                return HttpNotFound();
            }

            db.Dispose();
            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // Avoid unnecessary SQL query for retrieving row
                var courseToDelete = new Course () { CourseID = id };
                // Set state of entity in context to Deleted (not yet removed from data store)
                db.Entry(courseToDelete).State = EntityState.Deleted;
                // Generate SQL DELETE command
                db.SaveChanges();
                db.Dispose();
            }
            catch (DataException dex)
            {
                Console.WriteLine($"DataException: {dex.Message}");
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }

            db.Dispose();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
