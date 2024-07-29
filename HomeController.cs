using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModifiedCrudApp.Data;
using ModifiedCrudApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ModifiedCrudApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ModiCrudDbContext _context;

        public HomeController(ModiCrudDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Login")]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email is already registered
                var existingUser = _context.Logins.FirstOrDefault(l => l.Email == model.Email);
                if (existingUser == null)
                {
                    ModelState.AddModelError(string.Empty, "The email is not registered.");
                    return View(model);
                }

                // Add logic here to verify the user credentials
                // For example, checking the email and password against the database
                // Assuming password verification logic here

                // If successful, redirect to the home page or another page
                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [Route("Register")]
        public IActionResult Register(Register model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email or username already exists
                var existingUser = _context.Registers
                    .FirstOrDefault(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email or Username already exists.");
                    return View(model);
                }

                // Hash the password
                string hashedPassword = HashPassword(model.Password);

                // Create a new user
                var user = new Register
                {
                    Email = model.Email,
                    Password = hashedPassword,
                    ConfirmPassword = hashedPassword
                };

                // Save the user to the database
                _context.Registers.Add(user);
                _context.SaveChanges();

                // Redirect to the login page
                return RedirectToAction("Login");
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        [Route("Create")]
        public IActionResult Create()
        {
            ViewData["Countries"] = GetCountries();
            ViewData["States"] = GetStates();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create([Bind("EmpName,Email,Gender,Country,State")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var existingUser = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == employee.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                }
                else
                {
                    // Add new employee to the database
                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewData["Countries"] = GetCountries();
            ViewData["States"] = GetStates();
            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmail(string email)
        {
            bool exists = await _context.Employees.AnyAsync(u => u.Email == email);
            return Json(new { exists });
        }


        // GET: Employee/Edit/5
        [Route("Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            ViewData["Countries"] = GetCountries();
            ViewData["States"] = GetStates();

            return View(employee);
        }

        // POST: Employee/Edit/5
        [HttpPost]
[Route("Edit")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int empno, [Bind("EmpNo,EmpName,Email,Gender,Country,State")] Employee employee)
{
    if (ModelState.IsValid)
    {
        var emailExists = _context.Employees.Any(u => u.Email == employee.Email && u.EmpNo != employee.EmpNo);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "Email already exists. Please try a different email.");
        }
        else
        {
            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.EmpNo))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
    }
    ViewData["Countries"] = GetCountries();
    ViewData["States"] = GetStates();
    return View(employee);
}


        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmpNo == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public JsonResult IsEmailAvailable(string email, int empno)
        {
            var emailExists = _context.Employees.Any(u => u.Email == email && u.EmpNo != empno);
            return Json(new { isAvailable = !emailExists });
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmpNo == id);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        private List<string> GetCountries()
        {
            // Ideally, this should come from a database or a service
            return new List<string> { "Germany", "India", "Japan", "USA" };
        }

        private Dictionary<string, List<string>> GetStates()
        {
            return new Dictionary<string, List<string>> {
        { "India", new List<string> {
            "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chhattisgarh", "Goa", "Gujarat", "Haryana", "Himachal Pradesh",
            "Jharkhand", "Karnataka", "Kerala", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram", "Nagaland",
            "Odisha", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu", "Telangana", "Tripura", "Uttar Pradesh", "Uttarakhand", "West Bengal"
        }},
        { "USA", new List<string> {
            "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia",
            "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts",
            "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey",
            "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island",
            "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia",
            "Wisconsin", "Wyoming"
        }},
        { "Germany", new List<string> {
            "Baden-Württemberg", "Bavaria (Bayern)", "Berlin", "Brandenburg", "Bremen", "Hamburg", "Hesse (Hessen)", "Lower Saxony (Niedersachsen)",
            "Mecklenburg-Vorpommern", "North Rhine-Westphalia (Nordrhein-Westfalen)", "Rhineland-Palatinate (Rheinland-Pfalz)", "Saarland",
            "Saxony (Sachsen)", "Saxony-Anhalt (Sachsen-Anhalt)", "Schleswig-Holstein", "Thuringia (Thüringen)"
        }},
        { "Japan", new List<string> {
            "Aichi", "Akita", "Aomori", "Chiba", "Ehime", "Fukui", "Fukuoka", "Fukushima", "Gifu", "Gunma", "Hiroshima", "Hokkaido",
            "Hyogo", "Ibaraki", "Ishikawa", "Iwate", "Kagawa", "Kagoshima", "Kanagawa", "Kochi", "Kumamoto", "Kyoto", "Mie",
            "Miyagi", "Miyazaki", "Nagano", "Nagasaki", "Nara", "Niigata", "Oita", "Okayama", "Okinawa", "Osaka", "Saga", "Saitama",
            "Shiga", "Shimane", "Shizuoka", "Tochigi", "Tokushima", "Tokyo", "Tottori", "Toyama", "Wakayama", "Yamagata", "Yamaguchi", "Yamanashi"
        }}
        
    };
        }


    }
}
