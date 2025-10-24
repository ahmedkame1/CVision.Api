using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CVision.Api.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}