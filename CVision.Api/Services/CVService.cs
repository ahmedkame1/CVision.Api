// Services/CVService.cs
using CVision.Api.Data;
using CVision.Api.DTOs.CV;
using CVision.Api.Models;
using CVision.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CVision.Api.Services
{
    public class CVService : ICVService
    {
        private readonly CVisionDbContext _context;
        private readonly IPdfService _pdfService;

        public CVService(CVisionDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<CV> CreateCVAsync(int userId, CreateCvDto createCvDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // إذا كان CV أساسي، إلغاء الأساسية من السجلات الأخرى
                if (createCvDto.IsPrimary)
                {
                    await ResetPrimaryCVAsync(userId);
                }

                // إنشاء الـ CV الأساسي
                var cv = new CV
                {
                    UserId = userId,
                    Title = createCvDto.Title,
                    Summary = createCvDto.Summary,
                    Template = createCvDto.Template, // تغيير من TemplateType إلى Template
                    IsPrimary = createCvDto.IsPrimary || !await _context.CVs.AnyAsync(c => c.UserId == userId),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.CVs.Add(cv);
                await _context.SaveChangesAsync();

                // حفظ البيانات في الجداول المنفصلة
                await SaveCVDataAsync(cv.Id, createCvDto);

                await transaction.CommitAsync();
                return await GetCVByIdAsync(cv.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CV> GetCVByIdAsync(int cvId)
        {
            return await _context.CVs
                .Include(c => c.PersonalInfo)
                .Include(c => c.Experiences.OrderBy(e => e.DisplayOrder))
                .Include(c => c.Educations.OrderBy(e => e.DisplayOrder))
                .Include(c => c.Skills.OrderBy(s => s.DisplayOrder))
                .Include(c => c.Projects.OrderBy(p => p.DisplayOrder))
                .Include(c => c.Certifications.OrderBy(c => c.DisplayOrder))
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == cvId);
        }

        public async Task<List<CV>> GetUserCVsAsync(int userId)
        {
            return await _context.CVs
                .Include(c => c.PersonalInfo)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IsPrimary)
                .ThenByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateCVAsync(int cvId, CreateCvDto updateCvDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cv = await _context.CVs.FindAsync(cvId);
                if (cv == null) return false;

                // إذا أصبح أساسي، إلغاء الأساسية من الآخرين
                if (updateCvDto.IsPrimary && !cv.IsPrimary)
                {
                    await ResetPrimaryCVAsync(cv.UserId);
                }

                // تحديث البيانات الأساسية
                cv.Title = updateCvDto.Title;
                cv.Summary = updateCvDto.Summary;
                cv.Template = updateCvDto.Template; // تغيير من TemplateType إلى Template
                cv.IsPrimary = updateCvDto.IsPrimary;
                cv.UpdatedAt = DateTime.Now;

                // حذف البيانات القديمة
                await DeleteCVDataAsync(cvId);

                // حفظ البيانات الجديدة
                await SaveCVDataAsync(cvId, updateCvDto);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteCVAsync(int cvId)
        {
            var cv = await _context.CVs.FindAsync(cvId);
            if (cv == null) return false;

            _context.CVs.Remove(cv);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetPrimaryCVAsync(int userId, int cvId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // إلغاء الأساسية من جميع السجلات
                await ResetPrimaryCVAsync(userId);

                // تعيين الأساسية للسجل المطلوب
                var cv = await _context.CVs
                    .Where(c => c.Id == cvId && c.UserId == userId)
                    .FirstOrDefaultAsync();

                if (cv == null) return false;

                cv.IsPrimary = true;
                cv.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<byte[]> GenerateCvPdfAsync(int cvId)
        {
            var cv = await GetCVByIdAsync(cvId);
            if (cv == null) throw new ArgumentException("CV not found");

            return await _pdfService.GeneratePdf(cv); // تغيير من GenerateCvPdfAsync إلى GeneratePdf
        }

        // ========== Methods المساعدة ==========

        private async Task SaveCVDataAsync(int cvId, CreateCvDto dto)
        {
            // حفظ PersonalInfo
            if (dto.PersonalInfo != null)
            {
                var personalInfo = new PersonalInfo
                {
                    CvId = cvId,
                    FullName = dto.PersonalInfo.FullName,
                    Email = dto.PersonalInfo.Email,
                    Phone = dto.PersonalInfo.Phone,
                    Location = dto.PersonalInfo.Location, // تغيير من Address إلى Location
                    JobTitle = dto.PersonalInfo.JobTitle,
                    LinkedIn = dto.PersonalInfo.LinkedIn, // تغيير من LinkedInUrl إلى LinkedIn
                    GitHub = dto.PersonalInfo.GitHub, // تغيير من GitHubUrl إلى GitHub
                    Website = dto.PersonalInfo.Website
                };
                _context.PersonalInfos.Add(personalInfo);
            }

            // حفظ Experiences
            foreach (var expDto in dto.Experiences)
            {
                var experience = new Experience
                {
                    CvId = cvId,
                    JobTitle = expDto.JobTitle, // تغيير من Position إلى JobTitle
                    Company = expDto.Company,
                    Location = expDto.Location,
                    StartDate = expDto.StartDate,
                    EndDate = expDto.EndDate,
                    CurrentlyWorking = expDto.CurrentlyWorking, // تغيير من IsCurrent إلى CurrentlyWorking
                    Description = expDto.Description,
                    DisplayOrder = expDto.DisplayOrder
                };
                _context.Experiences.Add(experience);
            }

            // حفظ Educations
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
                    CurrentlyStudying = eduDto.CurrentlyStudying, // تغيير من IsCurrent إلى CurrentlyStudying
                    Description = eduDto.Description,
                    Grade = eduDto.Grade,
                    DisplayOrder = eduDto.DisplayOrder
                };
                _context.Educations.Add(education);
            }

            // حفظ Skills
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

            // حفظ Projects
            foreach (var projectDto in dto.Projects)
            {
                var project = new Project
                {
                    CvId = cvId,
                    Name = projectDto.Name,
                    Description = projectDto.Description,
                    Technologies = projectDto.Technologies,
                    ProjectLink = projectDto.ProjectLink, // تغيير من ProjectUrl إلى ProjectLink
                    GithubLink = projectDto.GithubLink, // تغيير من GitHubUrl إلى GithubLink
                    StartDate = projectDto.StartDate,
                    EndDate = projectDto.EndDate,
                    DisplayOrder = projectDto.DisplayOrder
                };
                _context.Projects.Add(project);
            }

            // حفظ Certifications
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

            await _context.SaveChangesAsync();
        }

        private async Task DeleteCVDataAsync(int cvId)
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
    }
}