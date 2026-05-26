using System.ComponentModel.DataAnnotations;

namespace TrelloMini.Models
{
    public class CreateProjectDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(10)]
        public string ProjectKey { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }


    }
}
