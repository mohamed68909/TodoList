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

        public IActionResult Index(DateTime? day)
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var selectedDate = day ?? DateTime.Today;

           
            var tasks = _context.TodoItems
                .Where(t => t.UserId == userId && t.CreatedAt.Date == selectedDate.Date)
                .ToList();

     
            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.IsDone);
            double percentage = totalTasks == 0 ? 0 : (double)completedTasks / totalTasks * 100;

            ViewBag.Percent = Math.Round(percentage);
            ViewBag.Total = totalTasks;
            ViewBag.Completed = completedTasks;
            ViewBag.CurrentDate = selectedDate;

            return View(tasks);
        }

        [HttpPost]
        public IActionResult Create(string title, DateTime taskDate)
        {
            if (!string.IsNullOrEmpty(title))
            {
                
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var newItem = new TodoItem
                {
                    Title = title,
                    CreatedAt = taskDate,
                    UserId = userId 
                };
                _context.TodoItems.Add(newItem);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", new { day = taskDate });
        }

    
        public IActionResult Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            var item = _context.TodoItems.FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (item != null)
            {
                _context.TodoItems.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        
        public IActionResult Toggle(int id, DateTime? returnDate)
        {
          
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var item = _context.TodoItems.FirstOrDefault(t => t.Id == id && t.UserId == userId);

            if (item != null)
            {
              
                item.IsDone = !item.IsDone;
                _context.SaveChanges();
            }

          
            return RedirectToAction("Index", new { day = returnDate });
        }
     
    }
}