using Contoso.DAL;
using Contoso.Models;
using PagedList;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Contoso.Controllers
{
    public class EnrollmentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Enrollment
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.TitleSortParam = (String.IsNullOrEmpty(sortOrder)) ? "title_desc" : ""; // Empty is default; title_asc
            ViewBag.NameSortParam = (sortOrder == "name_asc") ? "name_desc" : "name_asc";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var enrollmentQuery = db.Enrollments.Include(e => e.Course).Include(e => e.Student);
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString.ToUpper();
                enrollmentQuery = enrollmentQuery.Where(e =>
                    e.Course.Title.ToUpper().Contains(searchString) ||
                    e.Student.LastName.ToUpper().Contains(searchString) ||
                    e.Student.FirstMidName.ToUpper().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    enrollmentQuery = enrollmentQuery.OrderByDescending(c => c.Course.Title);
                    break;
                case "name_asc":
                    enrollmentQuery = enrollmentQuery.OrderBy(c => c.Student.LastName);
                    break;
                case "name_desc":
                    enrollmentQuery = enrollmentQuery.OrderByDescending(c => c.Student.LastName);
                    break;
                default:
                    enrollmentQuery = enrollmentQuery.OrderBy(c => c.Course.Title);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1); // (Null-coalescing operator) return 1 if page is null
            return View(enrollmentQuery.ToPagedList(pageNumber, pageSize));
        }

        // GET: Enrollment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
                return HttpNotFound($"No enrollment found matching id: {id}.");

            return View(enrollment);
        }

        // GET: Enrollment/Create
        public ActionResult Create()
        {
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName");
            return View();
        }

        // POST: Enrollment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseID,StudentID,Grade")] Enrollment enrollment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Enrollments.Add(enrollment);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException dex)
            {
                ModelState.AddModelError("", $"Unable to register new enrollment: {dex.Message}.");
            }

            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);

            return View(enrollment);
        }

        // GET: Enrollment/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
                return HttpNotFound();

            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title", enrollment.CourseID);
            ViewBag.StudentID = new SelectList(db.Students, "ID", "LastName", enrollment.StudentID);

            return View(enrollment);
        }

        // POST: Enrollment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var enrollmentToUpdate = db.Enrollments.Find(id);
            // Set Modified flag on entity, whitelisted fields in parameters.
            // Future data model fields are automatically "blacklisted" until added here.
            if (TryUpdateModel(enrollmentToUpdate, new string[] { "CourseID", "StudentID", "Grade" }))
            {
                try
                {
                    // Flag causes EF to create SQL to update ALL columns in db row (even the ones not changed).
                    // Set entity to Unchanged and individual fields to Modified to control column updates.
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    ModelState.AddModelError("", $"Unable to save changes: {dex.Message}.");
                }
            }

            return View(enrollmentToUpdate);
        }

        // GET: Enrollment/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage = "Unable to delete enrollment, please try again.";

            var enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
                return HttpNotFound();

            return View(enrollment);
        }

        // POST: Enrollment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // Avoid unnecessary SQL query for retrieving row
                var enrollmentToDelete = new Enrollment() { EnrollmentID = id };
                // Set state of entity in context to Deleted (not yet removed from data store)
                db.Entry(enrollmentToDelete).State = EntityState.Deleted;
                // Generate SQL DELETE command
                db.SaveChanges();
            }
            catch (DataException dex)
            {
                ModelState.AddModelError("", $"Unable to delete enrollment: {dex.Message}.");
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }

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
