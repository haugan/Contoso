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
    public class StudentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Student
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.LastNameSortParam = (String.IsNullOrEmpty(sortOrder)) ? "lastName_desc" : "";
            ViewBag.FirstNameSortParam = (sortOrder == "firstName_asc") ? "firstName_desc" : "firstName_asc";
            ViewBag.DateSortParam = (sortOrder == "date_asc") ? "date_desc" : "date_asc";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var studentQuery = from s in db.Students select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToUpper();
                studentQuery = studentQuery.Where(s =>
                    s.LastName.ToUpper().Contains(searchString) ||
                    s.FirstMidName.ToUpper().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "lastName_desc":
                    studentQuery = studentQuery.OrderByDescending(s => s.LastName);
                    break;
                case "firstName_desc":
                    studentQuery = studentQuery.OrderByDescending(s => s.FirstMidName);
                    break;
                case "firstName_asc":
                    studentQuery = studentQuery.OrderBy(s => s.FirstMidName);
                    break;
                case "date_desc":
                    studentQuery = studentQuery.OrderByDescending(s => s.EnrollmentDate);
                    break;
                case "date_asc":
                    studentQuery = studentQuery.OrderBy(s => s.EnrollmentDate);
                    break;
                default:
                    studentQuery = studentQuery.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1); // (Null-coalescing operator) return 1 if page is null
            return View(studentQuery.ToPagedList(pageNumber, pageSize));
        }

        // GET: Student/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var student = db.Students.Find(id);
            if (student == null)
                return HttpNotFound($"No student found matching id: {id}.");

            return View(student);
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Students.Add(student);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException dex)
            {
                ModelState.AddModelError("", $"Unable to register new student: {dex.Message}.");
            }

            return View(student);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var student = db.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            return View(student);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var existingStudent = db.Students.Find(id);

            // Set Modified flag on entity, whitelisted fields in parameters.
            // Future data model fields are automatically "blacklisted" until added here.
            if (TryUpdateModel(existingStudent, new string[] { "LastName", "FirstMidName", "EnrollmentDate" }))
            {
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DataException dex)
                {
                    ModelState.AddModelError("", $"Unable to save changes: {dex.Message}.");
                }
            }

            return View(existingStudent);
        }

        // GET: Student/Delete/5
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (saveChangesError.GetValueOrDefault())
                ViewBag.ErrorMessage = "Unable to delete student, please try again.";

            var student = db.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                // Avoid unnecessary SQL query for retrieving row
                var studentToDelete = new Student() { ID = id };
                // Set state of entity in context to Deleted (not yet removed from data store)
                db.Entry(studentToDelete).State = EntityState.Deleted;
                // Generate SQL DELETE command
                db.SaveChanges();
            }
            catch (DataException dex)
            {
                ModelState.AddModelError("", $"Unable to delete student: {dex.Message}.");
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
