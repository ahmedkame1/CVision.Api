// Services/PdfService.cs
using CVision.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace CVision.Api.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GeneratePdf(CV cv)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        // Choose template based on CV template
                        if (cv.Template == "Classic")
                        {
                            ComposeClassicTemplate(page, cv);
                        }
                        else if (cv.Template == "Executive")
                        {
                            ComposeExecutiveTemplate(page, cv);
                        }
                        else
                        {
                            ComposeModernTemplate(page, cv); // Default
                        }
                    });
                });

                return document.GeneratePdf();
            });
        }

        private void ComposeModernTemplate(PageDescriptor page, CV cv)
        {
            page.Header().Element(container => ComposeHeader(container, cv));
            page.Content().Element(container => ComposeModernContent(container, cv));
        }

        private void ComposeClassicTemplate(PageDescriptor page, CV cv)
        {
            page.Header().Element(container => ComposeHeader(container, cv));
            page.Content().Element(container => ComposeClassicContent(container, cv));
        }

        private void ComposeExecutiveTemplate(PageDescriptor page, CV cv)
        {
            page.Header().Element(container => ComposeExecutiveHeader(container, cv));
            page.Content().Element(container => ComposeExecutiveContent(container, cv));
        }

        private void ComposeHeader(IContainer container, CV cv)
        {
            var personalInfo = cv.PersonalInfo;

            container.Column(column =>
            {
                column.Spacing(10);

                // Name and Job Title
                column.Item().AlignCenter().Text(text =>
                {
                    text.Span(personalInfo?.FullName?.ToUpper() ?? "CV").Bold().FontSize(20).FontColor(Colors.Blue.Darken3);
                    text.EmptyLine();
                    text.Span(personalInfo?.JobTitle ?? "Professional").FontSize(14).FontColor(Colors.Grey.Darken2);
                });

                // Contact Information
                if (personalInfo != null)
                {
                    column.Item().PaddingTop(10).AlignCenter().Text(text =>
                    {
                        if (!string.IsNullOrEmpty(personalInfo.Email))
                            text.Span($"📧 {personalInfo.Email}").FontSize(10);

                        if (!string.IsNullOrEmpty(personalInfo.Phone))
                            text.Span($"  📱 {personalInfo.Phone}").FontSize(10);

                        if (!string.IsNullOrEmpty(personalInfo.Location))
                            text.Span($"  📍 {personalInfo.Location}").FontSize(10);

                        if (!string.IsNullOrEmpty(personalInfo.LinkedIn))
                            text.Span($"  💼 {personalInfo.LinkedIn}").FontSize(10);
                    });
                }

                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Medium);
            });
        }

        private void ComposeExecutiveHeader(IContainer container, CV cv)
        {
            var personalInfo = cv.PersonalInfo;

            container.Row(row =>
            {
                row.RelativeItem(2).Column(column =>
                {
                    column.Item().Text(personalInfo?.FullName?.ToUpper() ?? "CV").Bold().FontSize(18).FontColor(Colors.Black);
                    column.Item().Text(personalInfo?.JobTitle ?? "Professional").FontSize(12).FontColor(Colors.Grey.Darken2);
                });

                row.RelativeItem().Column(column =>
                {
                    if (!string.IsNullOrEmpty(personalInfo?.Email))
                        column.Item().Text($"Email: {personalInfo.Email}").FontSize(9);

                    if (!string.IsNullOrEmpty(personalInfo?.Phone))
                        column.Item().Text($"Phone: {personalInfo.Phone}").FontSize(9);

                    if (!string.IsNullOrEmpty(personalInfo?.Location))
                        column.Item().Text($"Location: {personalInfo.Location}").FontSize(9);
                });
            });

            container.PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Black);
        }

        private void ComposeModernContent(IContainer container, CV cv)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // Summary
                if (!string.IsNullOrEmpty(cv.Summary))
                {
                    column.Item().Element(c => ComposeSection(c, "PROFILE", cv.Summary));
                }

                // Experience
                if (cv.Experiences.Any())
                {
                    column.Item().Element(c => ComposeExperiences(c, cv.Experiences.ToList()));
                }

                // Education
                if (cv.Educations.Any())
                {
                    column.Item().Element(c => ComposeEducations(c, cv.Educations.ToList()));
                }

                // Skills
                if (cv.Skills.Any())
                {
                    column.Item().Element(c => ComposeSkills(c, cv.Skills.ToList()));
                }

                // Projects
                if (cv.Projects.Any())
                {
                    column.Item().Element(c => ComposeProjects(c, cv.Projects.ToList()));
                }

                // Certifications
                if (cv.Certifications.Any())
                {
                    column.Item().Element(c => ComposeCertifications(c, cv.Certifications.ToList()));
                }
            });
        }

        private void ComposeClassicContent(IContainer container, CV cv)
        {
            container.PaddingVertical(10).Column(column =>
            {
                // Two-column layout for classic template
                column.Item().Row(row =>
                {
                    // Left Column (Skills, Certifications)
                    row.RelativeItem().PaddingRight(10).Column(leftColumn =>
                    {
                        if (cv.Skills.Any())
                        {
                            leftColumn.Item().Element(c => ComposeSkills(c, cv.Skills.ToList()));
                        }

                        if (cv.Certifications.Any())
                        {
                            leftColumn.Item().Element(c => ComposeCertifications(c, cv.Certifications.ToList()));
                        }
                    });

                    // Right Column (Experience, Education, Projects)
                    row.RelativeItem(2).Column(rightColumn =>
                    {
                        if (!string.IsNullOrEmpty(cv.Summary))
                        {
                            rightColumn.Item().Element(c => ComposeSection(c, "PROFESSIONAL SUMMARY", cv.Summary));
                        }

                        if (cv.Experiences.Any())
                        {
                            rightColumn.Item().Element(c => ComposeExperiences(c, cv.Experiences.ToList()));
                        }

                        if (cv.Educations.Any())
                        {
                            rightColumn.Item().Element(c => ComposeEducations(c, cv.Educations.ToList()));
                        }

                        if (cv.Projects.Any())
                        {
                            rightColumn.Item().Element(c => ComposeProjects(c, cv.Projects.ToList()));
                        }
                    });
                });
            });
        }

        private void ComposeExecutiveContent(IContainer container, CV cv)
        {
            container.PaddingVertical(10).Column(column =>
            {
                if (!string.IsNullOrEmpty(cv.Summary))
                {
                    column.Item().Element(c => ComposeSection(c, "EXECUTIVE SUMMARY", cv.Summary));
                }

                if (cv.Experiences.Any())
                {
                    column.Item().Element(c => ComposeExperiences(c, cv.Experiences.ToList()));
                }

                if (cv.Educations.Any())
                {
                    column.Item().Element(c => ComposeEducations(c, cv.Educations.ToList()));
                }

                // Skills and Certifications in row
                if (cv.Skills.Any() || cv.Certifications.Any())
                {
                    column.Item().Row(row =>
                    {
                        if (cv.Skills.Any())
                        {
                            row.RelativeItem().PaddingRight(10).Column(skillsCol =>
                            {
                                skillsCol.Item().Element(c => ComposeSkills(c, cv.Skills.ToList()));
                            });
                        }

                        if (cv.Certifications.Any())
                        {
                            row.RelativeItem().Column(certsCol =>
                            {
                                certsCol.Item().Element(c => ComposeCertifications(c, cv.Certifications.ToList()));
                            });
                        }
                    });
                }
            });
        }

        private void ComposeSection(IContainer container, string title, string content)
        {
            container.Column(column =>
            {
                column.Item().Text(title).Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten1);
                column.Item().PaddingTop(5).Text(content).FontSize(10).LineHeight(1.3f);
                column.Item().PaddingBottom(15);
            });
        }

        private void ComposeExperiences(IContainer container, List<Experience> experiences)
        {
            container.Column(column =>
            {
                column.Item().Text("PROFESSIONAL EXPERIENCE").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten1);
                column.Item().PaddingTop(5);

                foreach (var exp in experiences)
                {
                    column.Item().PaddingBottom(10).Column(expColumn =>
                    {
                        expColumn.Item().Row(row =>
                        {
                            row.RelativeItem(3).Text(exp.JobTitle).Bold().FontSize(11);
                            row.RelativeItem(2).AlignRight().Text(FormatDateRange(exp.StartDate, exp.EndDate, exp.CurrentlyWorking)).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        expColumn.Item().Text(exp.Company).Bold().FontSize(10).FontColor(Colors.Blue.Medium);

                        if (!string.IsNullOrEmpty(exp.Location))
                            expColumn.Item().Text(exp.Location).Italic().FontSize(9).FontColor(Colors.Grey.Darken1);

                        if (!string.IsNullOrEmpty(exp.Description))
                            expColumn.Item().PaddingTop(3).Text(exp.Description).FontSize(9).LineHeight(1.2f);
                    });
                }
            });
        }

        private void ComposeEducations(IContainer container, List<Education> educations)
        {
            container.Column(column =>
            {
                column.Item().Text("EDUCATION").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten1);
                column.Item().PaddingTop(5);

                foreach (var edu in educations)
                {
                    column.Item().PaddingBottom(10).Column(eduColumn =>
                    {
                        eduColumn.Item().Row(row =>
                        {
                            row.RelativeItem(3).Text(edu.Degree).Bold().FontSize(11);
                            row.RelativeItem(2).AlignRight().Text(FormatDateRange(edu.StartDate, edu.EndDate, edu.CurrentlyStudying)).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        eduColumn.Item().Text(edu.Institution).Bold().FontSize(10).FontColor(Colors.Blue.Medium);

                        if (!string.IsNullOrEmpty(edu.Location))
                            eduColumn.Item().Text(edu.Location).Italic().FontSize(9).FontColor(Colors.Grey.Darken1);

                        if (!string.IsNullOrEmpty(edu.Grade))
                            eduColumn.Item().Text($"Grade: {edu.Grade}").FontSize(9);

                        if (!string.IsNullOrEmpty(edu.Description))
                            eduColumn.Item().PaddingTop(3).Text(edu.Description).FontSize(9).LineHeight(1.2f);
                    });
                }
            });
        }

        private void ComposeSkills(IContainer container, List<Skill> skills)
        {
            container.Column(column =>
            {
                column.Item().Text("SKILLS").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten1);
                column.Item().PaddingTop(5);

                var groupedSkills = skills.GroupBy(s => s.Category);
                foreach (var group in groupedSkills)
                {
                    column.Item().PaddingBottom(5).Text(group.Key).SemiBold().FontSize(10);
                    foreach (var skill in group)
                    {
                        column.Item().PaddingLeft(10).Text($"{skill.Name} - {skill.Level}").FontSize(9);
                    }
                }
            });
        }

        private void ComposeProjects(IContainer container, List<Project> projects)
        {
            container.Column(column =>
            {
                column.Item().Text("PROJECTS").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten1);
                column.Item().PaddingTop(5);

                foreach (var project in projects)
                {
                    column.Item().PaddingBottom(10).Column(projColumn =>
                    {
                        projColumn.Item().Row(row =>
                        {
                            row.RelativeItem(3).Text(project.Name).Bold().FontSize(11);
                            row.RelativeItem(2).AlignRight().Text(FormatDateRange(project.StartDate, project.EndDate, false)).FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        if (!string.IsNullOrEmpty(project.Technologies))
                            projColumn.Item().Text($"Technologies: {project.Technologies}").FontSize(9).FontColor(Colors.Grey.Darken1);

                        if (!string.IsNullOrEmpty(project.Description))
                            projColumn.Item().PaddingTop(3).Text(project.Description).FontSize(9).LineHeight(1.2f);

                        if (!string.IsNullOrEmpty(project.GithubLink))
                            projColumn.Item().Text($"GitHub: {project.GithubLink}").FontSize(8).FontColor(Colors.Blue.Medium);
                    });
                }
            });
        }

        private void ComposeCertifications(IContainer container, List<Certification> certifications)
        {
            container.Column(column =>
            {
                column.Item().Text("CERTIFICATIONS").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten1);
                column.Item().PaddingTop(5);

                foreach (var cert in certifications)
                {
                    column.Item().PaddingBottom(5).Column(certColumn =>
                    {
                        certColumn.Item().Text(cert.Name).Bold().FontSize(10);
                        certColumn.Item().Text(cert.IssuingOrganization).FontSize(9);
                        certColumn.Item().Text($"Issued: {cert.IssueDate:MMM yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                }
            });
        }

        private string FormatDateRange(DateTime startDate, DateTime? endDate, bool isCurrent)
        {
            var end = isCurrent ? "Present" : endDate?.ToString("MMM yyyy");
            return $"{startDate:MMM yyyy} - {end}";
        }
    }
}