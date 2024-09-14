using ContosoUni.Data;
using ContosoUni.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUni.Controllers
{
	public class StudentsController : Controller
	{
		private readonly SchoolContext _context;

		public StudentsController
			(
			SchoolContext context
			)
		{
			_context = context;
		}
		public async Task<IActionResult> Index(
			string sortOrder,
			string currentFilter,
			string searchString,
			int? pageNumber)
		{
			ViewData["CurrentSort"] = sortOrder;
			ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}

			ViewData["CurrentFilter"] = searchString;
			var students = from s in _context.Students
						   select s;

			if (!String.IsNullOrEmpty(searchString))
			{
				students = students.Where(s => s.LastName.Contains(searchString)
									|| s.FirstMidName.Contains(searchString));
			}

			switch (sortOrder)
			{
				case "name_desc":
					students = students.OrderByDescending(s => s.LastName);
					break;

				case "Date":
					students = students.OrderByDescending(s => s.EnrollmentDate);
					break;

				case "date_desc":
					students = students.OrderByDescending(s => s.EnrollmentDate);
					break;
				default:
					students = students.OrderBy(s => s.LastName);
					break;
			}

			int pageSize = 3;
			return View(await PaginatedList<Student>
				.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var student = await _context.Students
				.Include(s => s.Enrollments)
					.ThenInclude(e => e.Course)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);

			if (student == null)
			{
				return NotFound();
			}

			return View(student);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Student student)
		{
			try
			{
				if (ModelState.IsValid)
				{
					_context.Add(student);
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Unable to save changes. " +
					"Try again, and if the problem persists " +
					"see your system administrator.");

			}

			return View(student);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var student = await _context.Students.FindAsync(id);
			if (student == null)
			{
				return NotFound();
			}

			return View(student);
		}

		[HttpPost, ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPost(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var studentToUpdate = await _context.Students
				.FirstOrDefaultAsync(s => s.ID == id);

			if (await TryUpdateModelAsync<Student>(
				studentToUpdate,
				"",
				s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
			{
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateException)
				{
					ModelState.AddModelError("", "Unable to save changes. " +
						"Try again, and if the problem persists, " +
						"see your system administrator.");

				}
			}
			return View(studentToUpdate);
		}

		public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
		{
			if (id == null)
			{
				return NotFound();
			}

			var student = await _context.Students
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);

			if (student == null)
			{
				return NotFound();
			}

			if (saveChangesError.GetValueOrDefault())
			{
				ViewData["ErrorMessage"] = "Delete failed. " +
					"Try again, and if the problem persists " +
					"see your system administrator.";
			}

			return View(student);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			//var student = await _context.Students.FindAsync(id);
			//if (student == null)
			//{
			//    return RedirectToAction(nameof(Index));
			//}

			try
			{
				Student studentToDelete = new Student() { ID = id };
				_context.Entry(studentToDelete).State = EntityState.Deleted;
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateException)
			{
				return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
			}
		}
	}
}