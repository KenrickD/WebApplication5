using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication5.Controllers
{
    public class OTPSettingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OTPSettingController> _logger;
        private readonly IConfiguration _configuration;

        public OTPSettingController(
            ApplicationDbContext context,
            ILogger<OTPSettingController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var otpSettings = await _context.OTPSettings.ToListAsync();

                if (!otpSettings.Any())
                {
                    await CreateDefaultOTPSettings();
                    otpSettings = await _context.OTPSettings.ToListAsync();
                }

                return View(otpSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading OTP settings");
                TempData["ErrorMessage"] = "Error loading OTP settings. Please try again.";
                return View(new List<OTPSetting>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(List<OTPSetting> otpSettings)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", otpSettings);
            }

            try
            {
                // Option 1: Using Entity Framework (existing approach)
                await UpdateUsingEntityFramework(otpSettings);

                TempData["SuccessMessage"] = "OTP settings updated successfully!";
                _logger.LogInformation("OTP settings updated successfully using Entity Framework");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OTP settings");
                TempData["ErrorMessage"] = "Error updating OTP settings. Please try again.";
                return View("Index", otpSettings);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWithSQL(List<OTPSetting> otpSettings)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", otpSettings);
            }

            try
            {
                // Option 2: Using direct SQL execution
                await UpdateUsingDirectSQL(otpSettings);

                TempData["SuccessMessage"] = "OTP settings updated successfully using SQL!";
                _logger.LogInformation("OTP settings updated successfully using direct SQL");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OTP settings with SQL");
                TempData["ErrorMessage"] = "Error updating OTP settings with SQL. Please try again.";
                return View("Index", otpSettings);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var settings = await _context.OTPSettings.ToListAsync();
                return Json(new { success = true, data = settings });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OTP settings");
                return Json(new { success = false, message = "Error retrieving OTP settings" });
            }
        }

        // Private helper methods
        private async Task UpdateUsingEntityFramework(List<OTPSetting> otpSettings)
        {
            foreach (var setting in otpSettings)
            {
                var existingSetting = await _context.OTPSettings
                    .FirstOrDefaultAsync(x => x.OTPId == setting.OTPId);

                if (existingSetting != null)
                {
                    existingSetting.Email = setting.Email;
                    existingSetting.Whatsapp = setting.Whatsapp;
                    _context.Update(existingSetting);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateUsingDirectSQL(List<OTPSetting> otpSettings)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var setting in otpSettings)
                {
                    var sql = @"
                        UPDATE TB_OTPSetting 
                        SET Email = @Email, 
                            Whatsapp = @Whatsapp 
                        WHERE OTPId = @OTPId";

                    using var command = new SqlCommand(sql, connection, transaction);
                    command.Parameters.Add(new SqlParameter("@Email", SqlDbType.Bit) { Value = setting.Email });
                    command.Parameters.Add(new SqlParameter("@Whatsapp", SqlDbType.Bit) { Value = setting.Whatsapp });
                    command.Parameters.Add(new SqlParameter("@OTPId", SqlDbType.UniqueIdentifier) { Value = setting.OTPId });

                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task CreateDefaultOTPSettings()
        {
            var defaultSettings = new List<OTPSetting>
            {
                new OTPSetting
                {
                    OTPId = Guid.NewGuid(),
                    OTPAction = "Checklist All",
                    Email = false,
                    Whatsapp = false
                },
                new OTPSetting
                {
                    OTPId = Guid.NewGuid(),
                    OTPAction = "Withdrawal",
                    Email = false,
                    Whatsapp = false
                },
                new OTPSetting
                {
                    OTPId = Guid.NewGuid(),
                    OTPAction = "Forgot Password",
                    Email = false,
                    Whatsapp = false
                },
                new OTPSetting
                {
                    OTPId = Guid.NewGuid(),
                    OTPAction = "Reset Password",
                    Email = false,
                    Whatsapp = false
                }
            };

            _context.OTPSettings.AddRange(defaultSettings);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Default OTP settings created");
        }
    }
}