using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MF_SecureApi.Models
{
    public class RequestLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(45)]
        public string IpAddress { get; set; }

        [StringLength(512)]
        public string UserAgent { get; set; }

        [Required, StringLength(255)]
        public string RequestPath { get; set; }

        [Required, StringLength(10)]
        public string RequestMethod { get; set; }

        [Required]
        public int StatusCode { get; set; }

        [Required, Column(TypeName = "datetime2")]
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        public int? ResponseTimeMs { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string QueryString { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Headers { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? RequestBody { get; set; }

        [StringLength(128)]
        public string? UserId { get; set; }
    }
}