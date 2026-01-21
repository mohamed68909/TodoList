using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoList.Data;
using TodoList.Models;

namespace TodoListMVC.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly AppDbContext _context;

        public TodoController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? day, string mode = "daily")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var selectedDate = day ?? DateTime.Today;

            IQueryable<TodoItem> query = _context.TodoItems
                .Where(t => t.UserId == userId);

            if (mode == "daily")
            {
                query = query.Where(t => t.CreatedAt.Date == selectedDate.Date);
            }
            else if (mode == "weekly")
            {
                var startOfWeek = selectedDate.AddDays(-(int)selectedDate.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);

                query = query.Where(t =>
                    t.CreatedAt.Date >= startOfWeek.Date &&
                    t.CreatedAt.Date < endOfWeek.Date);
            }
            else if (mode == "monthly")
            {
                query = query.Where(t =>
                    t.CreatedAt.Month == selectedDate.Month &&
                    t.CreatedAt.Year == selectedDate.Year);
            }

            var tasks = query.ToList();

            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.IsDone);
            double percentage = totalTasks == 0 ? 0 : (double)completedTasks / totalTasks * 100;

            ViewBag.Percent = Math.Round(percentage);
            ViewBag.Total = totalTasks;
            ViewBag.Completed = completedTasks;
            ViewBag.CurrentDate = selectedDate;
            ViewBag.Mode = mode;

            return View(tasks);
        }

        [HttpPost]
        public IActionResult Create(string title, DateTime taskDate, string mode)
        {
            if (!string.IsNullOrEmpty(title))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var item = new TodoItem
                {
                    Title = title,
                    CreatedAt = taskDate,
                    UserId = userId
                };

                _context.TodoItems.Add(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", new { day = taskDate, mode });
        }

        public IActionResult Toggle(int id, DateTime? returnDate, string mode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = _context.TodoItems
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (item != null)
            {
                item.IsDone = !item.IsDone;
                _context.SaveChanges();
            }

            return RedirectToAction("Index", new { day = returnDate, mode });
        }

        public IActionResult Delete(int id, DateTime? day, string mode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = _context.TodoItems
                .FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (item != null)
            {
                _context.TodoItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index", new { day, mode });
        }
    }
}
