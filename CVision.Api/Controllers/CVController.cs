// Controllers/CVController.cs
using CVision.Api.Data;
using CVision.Api.DTOs.CV;
using CVision.Api.Models;
using CVision.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CVision.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CVController : ControllerBase
    {
        private readonly CVisionDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPdfService _pdfService;

        public CVController(CVisionDbContext context, UserManager<ApplicationUser> userManager, IPdfService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
        }

        // GET: api/cv/my-cvs
        [HttpGet("my-cvs")]
        public async Task<ActionResult<List<object>>> GetMyCVs()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cvs = await _context.CVs
                    .Where(c => c.UserId == userId)
                    .Include(c => c.PersonalInfo)
                    .Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.Template,
                        c.Summary,
                        c.IsPrimary,
                        c.CreatedAt,
                        c.UpdatedAt,
                        PersonalInfo = c.PersonalInfo != null ? new
                        {
                            c.PersonalInfo.FullName,
                            c.PersonalInfo.JobTitle,
                            c.PersonalInfo.Email
                        } : null
                    })
                    .OrderByDescending(c => c.IsPrimary)
                    .ThenByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                return Ok(new
                {
                    count = cvs.Count,
                    cvs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving CVs", error = ex.Message });
            }
        }

        // GET: api/cv/1
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCV(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cv = await _context.CVs
                    .Include(c => c.PersonalInfo)
                    .Include(c => c.Experiences.OrderBy(e => e.DisplayOrder))
                    .Include(c => c.Educations.OrderBy(e => e.DisplayOrder))
                    .Include(c => c.Skills.OrderBy(s => s.DisplayOrder))
                    .Include(c => c.Projects.OrderBy(p => p.DisplayOrder))
                    .Include(c => c.Certifications.OrderBy(c => c.DisplayOrder))
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cv == null)
                    return NotFound(new { message = "CV not found" });

                return Ok(cv);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving CV", error = ex.Message });
            }
        }

        // POST: api/cv
        [HttpPost]
        public async Task<ActionResult<CV>> CreateCV([FromBody] CreateCvDto createCvDto)
        {
            // Validation
            if (createCvDto.PersonalInfo == null)
                return BadRequest(new { message = "Personal info is required" });

            if (string.IsNullOrEmpty(createCvDto.PersonalInfo.FullName))
                return BadRequest(new { message = "Full name is required" });

            if (string.IsNullOrEmpty(createCvDto.PersonalInfo.Email))
                return BadRequest(new { message = "Email is required" });

            var userId = GetCurrentUserId();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // إذا كان CV أساسي، إلغاء الأساسية من السجلات الأخرى
                if (createCvDto.IsPrimary)
                {
                    await ResetPrimaryCVAsync(userId);
                }

                // Create CV
                var cv = new CV
                {
                    UserId = userId,
                    Template = createCvDto.Template,
                    Title = createCvDto.Title,
                    Summary = createCvDto.Summary,
                    IsPrimary = createCvDto.IsPrimary || !await _context.CVs.AnyAsync(c => c.UserId == userId),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();

                // Create related data
                await CreateCVRelatedData(cv.Id, createCvDto);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var createdCv = await _context.CVs
                    .Include(c => c.PersonalInfo)
                    .Include(c => c.Experiences)
                    .Include(c => c.Educations)
                    .Include(c => c.Skills)
                    .Include(c => c.Projects)
                    .Include(c => c.Certifications)
                    .FirstOrDefaultAsync(c => c.Id == cv.Id);

                return CreatedAtAction(nameof(GetCV), new { id = cv.Id }, new { message = "CV created successfully", cv = createdCv });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error creating CV", error = ex.Message });
            }
        }

        // PUT: api/cv/1
        [HttpPut("{id}")]
        public async Task<ActionResult<CV>> UpdateCV(int id, [FromBody] CreateCvDto updateCvDto)
        {
            // Validation
            if (updateCvDto.PersonalInfo == null)
                return BadRequest(new { message = "Personal info is required" });

            var userId = GetCurrentUserId();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cv = await _context.CVs
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cv == null)
                    return NotFound(new { message = "CV not found" });

                // إذا أصبح أساسي، إلغاء الأساسية من الآخرين
                if (updateCvDto.IsPrimary && !cv.IsPrimary)
                {
                    await ResetPrimaryCVAsync(userId);
                }

                // Update CV basic info
                cv.Template = updateCvDto.Template;
                cv.Title = updateCvDto.Title;
                cv.Summary = updateCvDto.Summary;
                cv.IsPrimary = updateCvDto.IsPrimary;
                cv.UpdatedAt = DateTime.Now;

                // Delete old related data
                await DeleteCVRelatedData(id);

                // Recreate related data
                await CreateCVRelatedData(cv.Id, updateCvDto);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var updatedCv = await _context.CVs
                    .Include(c => c.PersonalInfo)
                    .Include(c => c.Experiences)
                    .Include(c => c.Educations)
                    .Include(c => c.Skills)
                    .Include(c => c.Projects)
                    .Include(c => c.Certifications)
                    .FirstOrDefaultAsync(c => c.Id == cv.Id);

                return Ok(new { message = "CV updated successfully", cv = updatedCv });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error updating CV", error = ex.Message });
            }
        }

        // DELETE: api/cv/1
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCV(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                var cv = await _context.CVs
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cv == null)
                    return NotFound(new { message = "CV not found" });

                _context.CVs.Remove(cv);
                await _context.SaveChangesAsync();

                return Ok(new { message = "CV deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting CV", error = ex.Message });
            }
        }

        // POST: api/cv/1/set-primary
        [HttpPost("{id}/set-primary")]
        public async Task<ActionResult> SetPrimaryCV(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                // Reset all primary CVs
                var primaryCVs = await _context.CVs
                    .Where(c => c.UserId == userId && c.IsPrimary)
                    .ToListAsync();

                foreach (var primaryCv in primaryCVs)
                {
                    primaryCv.IsPrimary = false;
                }

                // Set new primary CV
                var cv = await _context.CVs
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cv == null)
                    return NotFound(new { message = "CV not found" });

                cv.IsPrimary = true;
                cv.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { message = "CV set as primary successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error setting primary CV", error = ex.Message });
            }
        }

        // GET: api/cv/1/export-pdf
        [HttpGet("{id}/export-pdf")]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var cv = await _context.CVs
                    .Include(c => c.PersonalInfo)
                    .Include(c => c.Experiences.OrderBy(e => e.DisplayOrder))
                    .Include(c => c.Educations.OrderBy(e => e.DisplayOrder))
                    .Include(c => c.Skills.OrderBy(s => s.DisplayOrder))
                    .Include(c => c.Projects.OrderBy(p => p.DisplayOrder))
                    .Include(c => c.Certifications.OrderBy(c => c.DisplayOrder))
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cv == null)
                    return NotFound(new { message = "CV not found" });

                var pdfBytes = await _pdfService.GeneratePdf(cv);

                var fileName = $"{cv.PersonalInfo?.FullName?.Replace(" ", "_") ?? "CV"}_Resume.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        // GET: api/cv/templates
        [HttpGet("templates")]
        public ActionResult<List<object>> GetTemplates()
        {
            var templates = new[]
            {
                new { Id = "Modern", Name = "Modern", Description = "Clean and professional design" },
                new { Id = "Classic", Name = "Classic", Description = "Traditional and formal design" },
                new { Id = "Executive", Name = "Executive", Description = "Professional executive style" },
                new { Id = "Creative", Name = "Creative", Description = "Modern and creative design" }
            };

            return Ok(templates);
        }

        // ========== Helper Methods ==========

        private async Task CreateCVRelatedData(int cvId, CreateCvDto dto)
        {
            // Create Personal Info
            if (dto.PersonalInfo != null)
            {
                var personalInfo = new PersonalInfo
                {
                    CvId = cvId,
                    FirstName = dto.PersonalInfo.FirstName,
                    LastName = dto.PersonalInfo.LastName,
                    JobTitle = dto.PersonalInfo.JobTitle,
                    Email = dto.PersonalInfo.Email,
                    Phone = dto.PersonalInfo.Phone,
                    Location = dto.PersonalInfo.Location,
                    LinkedIn = dto.PersonalInfo.LinkedIn,
                    GitHub = dto.PersonalInfo.GitHub,
                    Website = dto.PersonalInfo.Website
                };
                _context.PersonalInfos.Add(personalInfo);
            }

            // Create Experiences
            foreach (var expDto in dto.Experiences)
            {
                var experience = new Experience
                {
                    CvId = cvId,
                    JobTitle = expDto.JobTitle,
                    Company = expDto.Company,
                    Location = expDto.Location,
                    StartDate = expDto.StartDate,
                    EndDate = expDto.EndDate,
                    CurrentlyWorking = expDto.CurrentlyWorking,
                    Description = expDto.Description,
                    DisplayOrder = expDto.DisplayOrder
                };
                _context.Experiences.Add(experience);
            }

            // Create Educations
            foreach (var eduDto in dto.Educations)
            {
                var education = new Education
                {
                    CvId = cvId,
                    Degree = eduDto.Degree,
                    Institution = eduDto.Institution,
                    Location = eduDto.Location,
                    StartDate = eduDto.StartDate,
                    EndDate = eduDto.EndDate,
                    CurrentlyStudying = eduDto.CurrentlyStudying,
                    Description = eduDto.Description,
                    Grade = eduDto.Grade,
                    DisplayOrder = eduDto.DisplayOrder
                };
                _context.Educations.Add(education);
            }

            // Create Skills
            foreach (var skillDto in dto.Skills)
            {
                var skill = new Skill
                {
                    CvId = cvId,
                    Name = skillDto.Name,
                    Level = skillDto.Level,
                    Category = skillDto.Category,
                    YearsOfExperience = skillDto.YearsOfExperience,
                    DisplayOrder = skillDto.DisplayOrder
                };
                _context.Skills.Add(skill);
            }

            // Create Projects
            foreach (var projectDto in dto.Projects)
            {
                var project = new Project
                {
                    CvId = cvId,
                    Name = projectDto.Name,
                    Description = projectDto.Description,
                    Technologies = projectDto.Technologies,
                    ProjectLink = projectDto.ProjectLink,
                    GithubLink = projectDto.GithubLink,
                    StartDate = projectDto.StartDate,
                    EndDate = projectDto.EndDate,
                    DisplayOrder = projectDto.DisplayOrder
                };
                _context.Projects.Add(project);
            }

            // Create Certifications
            foreach (var certDto in dto.Certifications)
            {
                var certification = new Certification
                {
                    CvId = cvId,
                    Name = certDto.Name,
                    IssuingOrganization = certDto.IssuingOrganization,
                    IssueDate = certDto.IssueDate,
                    ExpirationDate = certDto.ExpirationDate,
                    CredentialId = certDto.CredentialId,
                    CredentialUrl = certDto.CredentialUrl,
                    DisplayOrder = certDto.DisplayOrder
                };
                _context.Certifications.Add(certification);
            }
        }

        private async Task DeleteCVRelatedData(int cvId)
        {
            await _context.PersonalInfos.Where(p => p.CvId == cvId).ExecuteDeleteAsync();
            await _context.Experiences.Where(e => e.CvId == cvId).ExecuteDeleteAsync();
            await _context.Educations.Where(e => e.CvId == cvId).ExecuteDeleteAsync();
            await _context.Skills.Where(s => s.CvId == cvId).ExecuteDeleteAsync();
            await _context.Projects.Where(p => p.CvId == cvId).ExecuteDeleteAsync();
            await _context.Certifications.Where(c => c.CvId == cvId).ExecuteDeleteAsync();
        }

        private async Task ResetPrimaryCVAsync(int userId)
        {
            var primaryCVs = await _context.CVs
                .Where(c => c.UserId == userId && c.IsPrimary)
                .ToListAsync();

            foreach (var cv in primaryCVs)
            {
                cv.IsPrimary = false;
            }
        }

        private int GetCurrentUserId()
        {
            return int.Parse(_userManager.GetUserId(User)!);
        }
    }
}