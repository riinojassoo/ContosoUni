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
		public async Task<IActionResult> Index()
		{
			var result = await _context
				.Students.ToListAsync();
			return View(result);
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
	}
}