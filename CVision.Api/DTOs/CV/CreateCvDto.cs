// DTOs/CV/CreateCvDto.cs
namespace CVision.Api.DTOs.CV
{
    public class CreateCvDto
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Template { get; set; } = "Modern"; // تغيير من TemplateType إلى Template
        public bool IsPrimary { get; set; } // إضافة هذا الحقل

        public PersonalInfoDto PersonalInfo { get; set; } = new();
        public List<ExperienceDto> Experiences { get; set; } = new();
        public List<EducationDto> Educations { get; set; } = new();
        public List<SkillDto> Skills { get; set; } = new();
        public List<ProjectDto> Projects { get; set; } = new();
        public List<CertificationDto> Certifications { get; set; } = new();
    }

    public class PersonalInfoDto
    {
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty; // استخدام Location بدل Address
        public string? LinkedIn { get; set; } // استخدام LinkedIn بدل LinkedInUrl
        public string? GitHub { get; set; } // استخدام GitHub بدل GitHubUrl
        public string? Website { get; set; }
    }

    public class ExperienceDto
    {
        public string JobTitle { get; set; } = string.Empty; // استخدام JobTitle بدل Position
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyWorking { get; set; } // استخدام CurrentlyWorking بدل IsCurrent
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class EducationDto
    {
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyStudying { get; set; } // استخدام CurrentlyStudying بدل IsCurrent
        public string? Grade { get; set; }
        public string Description { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class SkillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = "Intermediate";
        public string Category { get; set; } = "Technical";
        public int? YearsOfExperience { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Technologies { get; set; }
        public string? ProjectLink { get; set; } // استخدام ProjectLink بدل ProjectUrl
        public string? GithubLink { get; set; } // استخدام GithubLink بدل GitHubUrl
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
        public int DisplayOrder { get; set; }
    }
}