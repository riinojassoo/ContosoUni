using ContosoUni.Data;
using ContosoUni.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUni.Controllers
{
	public class CoursesController : Controller
	{
		private readonly SchoolContext _context;

		public CoursesController
			(
			SchoolContext context
			)
		{
			_context = context;
		}
		public async Task<IActionResult> Index()
		{
			var result = await _context.Courses
				.Include(c => c.Department)
				.AsNoTracking()
				.ToListAsync();


			return View(result);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if ( id == null )
			{
				return NotFound();
			}

			var course = await _context.Courses
				.Include(c => c.Department)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.CourseID == id);

			if (course == null)
			{
				return NotFound();
			}

			return View(course);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if ( id == null )
			{
				return NotFound();
			}

			var course = await _context.Courses
				.AsNoTracking()
				.FirstOrDefaultAsync (m => m.CourseID == id);

			if (course == null)
			{
				return NotFound();
			}

			PopulateDepartmentDropDownList(course.DepartmentID);
			return View(course);
		}

		[HttpPost, ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPost(Course model)
		{
			if (model.CourseID == null)
			{
				return NotFound();
			}

			Course domain = new();

			domain = await _context.Courses
				.FirstOrDefaultAsync(c => c.CourseID == model.CourseID);

			domain.CourseID = model.CourseID;
			domain.Title = model.Title;
			domain.Credits = model.Credits;
			domain.DepartmentID = model.DepartmentID;

			_context.Update(domain);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Courses == null)
			{
				return NotFound();
			}

			var courses = await _context.Courses
				.Include(c => c.Department)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.CourseID == id);

			if (courses == null)
			{
				return NotFound();
			}

			return View(courses);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if(_context.Courses == null)
			{
				return Problem("Entity set 'SchoolContext.Corses' is null.");
			}

			var course = await _context.Courses.FindAsync(id);
			if (course != null)
			{
				_context.Courses.Remove(course);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Create()
		{
			PopulateDepartmentDropDownList();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Course course)
		{
			//if (ModelState.IsValid)
			//{
				_context.Add(course);
				await _context.SaveChangesAsync();
				//return RedirectToAction(nameof(Index));
			//}

			PopulateDepartmentDropDownList(course.DepartmentID);
			return RedirectToAction(nameof(Index));
		}

		private void PopulateDepartmentDropDownList(object selectedDepartment = null)
		{
			var departmentsQuery = from d in _context.Departments
								   orderby d.Name
								   select d;
			ViewBag.DepartmentID = new SelectList(departmentsQuery
                .AsNoTracking(), "DepartmentId", "Name", selectedDepartment);
		}
	}
}