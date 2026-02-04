using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels.DepartmentManagement
{
    public class Office
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string OfficeCode { get; set; }

        [Required]
        public string OfficeName { get; set; }
        [Required]
        public string ContactPersonName { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string ContactEmail { get; set; }
        [Required]
        public string OrganizationCode {  get; set; }
        public Organization Organization { get; set; }
        [Required]
        public string OfficeAddress {  get; set; }
        public double TotalStorageSize { get; set; }
        public double UsedStorageSize { get; set; }
        public double AllocateStorageSize { get; set; }
        public bool IsActive { get; set; }
    }
}
