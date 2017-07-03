
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
    public class StudentController : Controller
    {
        private SchoolContext db = new SchoolContext();

        // GET: Student
        public ActionResult Index(string sortOrder, string searchString)
        {
            var students = from s in db.Students select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToUpper();
                students = students.Where(s =>
                    s.LastName.ToUpper().Contains(searchString) ||
                    s.FirstMidName.ToUpper().Contains(searchString));
            }

            ViewBag.LastNameSortParam = (String.IsNullOrEmpty(sortOrder)) ? "lastName_desc" : "";
            ViewBag.FirstNameSortParam = (sortOrder == "firstName_asc") ? "firstName_desc" : "firstName_asc";
            ViewBag.DateSortParam = (sortOrder == "date_asc") ? "date_desc" : "date_asc";

            switch (sortOrder)
            {
                case "lastName_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "firstName_desc":
                    students = students.OrderByDescending(s => s.FirstMidName);
                    break;
                case "firstName_asc":
                    students = students.OrderBy(s => s.FirstMidName);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                case "date_asc":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            var studentsList = students.ToList(); // ToList() executes SQL query
            db.Dispose();
            return View(studentsList);
        }

        // GET: Student/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var student = db.Students.Find(id);
            if (student == null)
            {
                db.Dispose();
                return HttpNotFound($"No student found matching id: {id}.");
            }

            db.Dispose();
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
                    db.Dispose();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException dex)
            {
                Console.WriteLine($"DataException: {dex.Message}");
                ModelState.AddModelError("", "Unable to register new student, please try again.");
            }

            db.Dispose();
            return View(student);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var student = db.Students.Find(id);
            if (student == null)
            {
                db.Dispose();
                return HttpNotFound();
            }

            db.Dispose();
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
            {
                db.Dispose();
                return HttpNotFound();
            }

            db.Dispose();
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
