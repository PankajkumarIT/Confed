using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class RegisterDepartmentOrganizationVM
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string OrganizationCode { get; set; }
        [Required]
        public string OrganizationName { get; set; }
        [Required]
        public string Address { get; set; }
        public string ShortName { get; set; }

        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string Email { get; set; }
        public string ParentOrganizationCode { get; set; }

        public string FileHeader { get; set; }
        public string OutputFileHeader {  get; set; }
        [RegularExpression("^(SFTP|SFTPForPaymentTransaction|SFTPForPaymentWithEncryption )$", ErrorMessage = "Invalid Feture Type.")]
        public string FetureType { get; set; }
        public double StorageSize { get; set; }
        public bool IsActive { get; set; }
    }
}
