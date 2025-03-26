using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MF_SecureApi.Models
{
    public class ErrorLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Message { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string StackTrace { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Source { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string RequestPath { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string UserAgent { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string IpAddress { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    }
}