using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels
{
    public class DepartmentBankOrganizationMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string DepartmentBankOrganizationMapCode {  get; set; }
        [Required]
        public string DepartmentOrganizationCode {  get; set; }
        public Organization DepartmentOrganization { get; set; }
        [Required]
        public string BankOrganizationCode {  get; set; }
        public Organization BankOrganization { get; set; }
        [Required]
        public string InputFileHeader {  get; set; }

        public bool IsMapped {  get; set; }
    }
}
