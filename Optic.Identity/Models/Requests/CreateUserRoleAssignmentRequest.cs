using System.ComponentModel.DataAnnotations;

namespace Optic.Identity.Models.Requests
{
        public class CreateUserRoleAssignmentRequest
        {
            [Required]
            public int UserId { get; set; }

            [Required]
            public int ApplicationId { get; set; }

            [Required]
            public int RoleId { get; set; }

            [Required]
            public DateTime ValidFrom { get; set; }

            public DateTime? ValidTo { get; set; }
        }
}
