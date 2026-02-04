
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels.BankManagement
{
    public class BankBranch
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string BankBranchCode { get; set; }

        [Required]
        public string BankName { get; set; }

        [Required]
        public string BranchName { get; set; }

        [Required]
        public string IFSC { get; set; }

        [Required]
        public string MICR { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        public string ContactNumber { get; set; }
        public string BranchAddress {  get; set; }

        [Required]
        public string Email { get; set; }
        [Required]
        public string OrganizationCode {  get; set; }
        public Organization Organization { get; set; }
        public double TotalStorageSize { get; set; }
        public double UsedStorageSize { get; set; }
        public double AllocateStorageSize { get; set; }


        public bool IsActive { get; set; } = true;
    }
}
