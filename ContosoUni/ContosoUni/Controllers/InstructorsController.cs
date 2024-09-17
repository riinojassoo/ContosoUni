using ContosoUni.Data;
using ContosoUni.Models;
using ContosoUni.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContosoUni.Controllers
{
	public class InstructorsController : Controller
	{
		private readonly SchoolContext _context;
		public InstructorsController
			(
			SchoolContext context
			)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int? id, int? courseId)
		{
			var viewModel = new InstructorIndexData();
			viewModel.Instructors = await _context.Instructors
				.Include(i => i.OfficeAssignment)
				.Include(c => c.CourseAssignments)
					.ThenInclude(x => x.Course)
						.ThenInclude(y => y.Enrollments)
							.ThenInclude(z => z.Student)
				.Include(a => a.CourseAssignments)
					.ThenInclude(b => b.Course)
						.ThenInclude(d => d.Department)
				.AsNoTracking()
				.OrderBy(e => e.LastName)
				.ToListAsync();

			if(id != null)
			{
				ViewData["InstructorID"] = id.Value;
				Instructor instructor = viewModel.Instructors.Where(
					i => i.ID == id.Value).Single();
				viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
			}

			if(courseId != null)
			{
				ViewData["CourseId"] = courseId.Value;
				var selectedCourse = viewModel.Courses
					.Where(x => x.CourseID == courseId)
					.Single();

				await _context.Entry(selectedCourse)
					.Collection(x => x.Enrollments)
					.LoadAsync();

				foreach (Enrollment enrollment in selectedCourse.Enrollments)
				{
					await _context.Entry(enrollment)
						.Reference(x => x.Student)
						.LoadAsync();
				}
				viewModel.Enrollments = selectedCourse.Enrollments;
			}

			return View(viewModel);
		}
	}
	
}
