using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Domain.Models;

namespace WebApplication5.Pages
{
    public class OTPSettingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OTPSettingModel> _logger;

        public OTPSettingModel(ApplicationDbContext context, ILogger<OTPSettingModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public List<OTPSetting> OTPSettings { get; set; } = new List<OTPSetting>();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                OTPSettings = await _context.OTPSettings.ToListAsync();

                if (!OTPSettings.Any())
                {
                    await CreateDefaultOTPSettings();
                    OTPSettings = await _context.OTPSettings.ToListAsync();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading OTP settings");
                TempData["ErrorMessage"] = "Error loading OTP settings. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                foreach (var setting in OTPSettings)
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

                TempData["SuccessMessage"] = "OTP settings updated successfully!";
                _logger.LogInformation("OTP settings updated successfully");

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating OTP settings");
                TempData["ErrorMessage"] = "Error updating OTP settings. Please try again.";
                return Page();
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