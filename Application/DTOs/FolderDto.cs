using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class FolderDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Folder name is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Folder name must be between 1 and 255 characters")]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // UI-specific properties
        public bool IsEditing { get; set; } = false;

        public bool IsNew { get; set; } = false;
    }
}
